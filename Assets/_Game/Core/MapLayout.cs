// MapLayout.cs
// Owner: Moustafa
// Layer: Core, pure C#, no Unity (Game.Core.asmdef blocks Unity references)
//
// The map as data: a 3D grid of cells
// A cell is a box: 3 m wide, 3 m deep, one level high
// A room is one or more cells joined by open sides, the code only knows cells
// Every system uses GridCoord to talk about positions
//
// Storage
// One array with an entry for every cell: width * depth * levels
// Index of a cell = x + width * (z + depth * level)
//
// Store-once rule
// A wall between two cells belongs to both, so it is stored only once
// Each cell stores its north wall, its east wall and its floor
// Its south wall is the north wall of the cell south of it
// Its west wall is the east wall of the cell west of it
// Its ceiling is the floor of the cell above it
//
// Two sides that are always Solid, whatever is stored
//   the outer edge: if the cell on the other side is outside the map, the side is Solid, on all four edges
//   so the arena is a closed box on every level, with open sky above (the stored north and east of edge cells are ignored)
//   next to a Solid cell: if either cell on a side is a Solid cell (a pillar), the side is Solid
//   you can't see or shoot into a solid block, and MapBuilder draws no wall there because the block already is one
// Never read stored fields for gameplay, always ask the map (GetSide, GetSurfaces...)
//
// Floors and ceilings
// A room's ceiling is the floor of the cell above it, so every cell has a floor, even empty air
// Example: a pad room on level 0 with an Empty cell above it on level 1
//   level 1 floor Solid -> the room has a roof
//   level 1 floor Open  -> the room is open to the sky
//   level 1 floor Hatch -> the room has a hatch in its roof, people above can shoot down into it
// Nobody stands on that roof because the cell above is not a pad
// The top level has no level above it, so it has no ceiling (open sky)
// That never exposes anyone, nobody can be higher than the top level
// To give top-level rooms a roof, add one more level of Empty cells with Solid floors
// A pad's floor is Solid or Hatch
// A Hatch hole is always in the middle of the cell (1 m square)
// A pad with a Hatch floor must be pushed to any side or corner (not Center), so the player stands beside the hole, not on it
// Non-pad cells can have any floor, a Hatch or Open floor there is a missing square in the board
//
// Spawn pads
// A spawn pad is a normal pad with a spawn rank above 0 (1 = best spawn, 2 = next best...)
// Players can still teleport onto spawn pads like any other pad
// A map needs at least 8 spawn pads, ranked 1, 2, 3... with no gaps or repeats (MapValidator checks this)
//
// Pad placement
// A pad sits in the middle of its cell or is pushed toward a side or corner (PadAnchor)
// Pushed pads put the player next to a wall so leaning and peeking work
// The push distance is a map setting in MapDefinition, this file only stores the direction
//
// Style exception
// GridCoord, Cell and MapLayout use public fields instead of private _camelCase fields
// Unity only saves public fields, or private fields marked [SerializeField]
// [SerializeField] belongs to Unity, and Core is set up with no access to Unity (to stay pure and testable)
// So public fields are the only way for Unity to save map data from Core

using System;
using System.Collections.Generic;

namespace Game.Core
{
    // Position of a cell on the grid
    // x and z = position on the floor, level = height floor (0 = ground)
    [Serializable]
    public struct GridCoord : IEquatable<GridCoord>
    {
        public int x;
        public int level;
        public int z;

        public GridCoord(int x, int level, int z)
        {
            this.x = x;
            this.level = level;
            this.z = z;
        }

        // Distance in pads on the floor, counted like a chess king (a diagonal step is 1 pad)
        // Ignores level
        // Every movement rule uses this (Cluster "1 away", Ring "exactly 2 away", Ray "lines from 2 pads")
        public static int PadDistance(GridCoord a, GridCoord b)
        {
            return Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.z - b.z));
        }

        // A new coord shifted by the given amounts
        public GridCoord Offset(int dx, int dLevel, int dz)
        {
            return new GridCoord(x + dx, level + dLevel, z + dz);
        }

        // Equality, so coords can be compared with == and used in lists and dictionaries
        public bool Equals(GridCoord other)
        {
            return x == other.x && level == other.level && z == other.z;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (x * 73856093) ^ (level * 19349663) ^ (z * 83492791);
        }

        public static bool operator ==(GridCoord a, GridCoord b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GridCoord a, GridCoord b)
        {
            return !a.Equals(b);
        }

        // Readable form for logs, like (2,L1,4)
        public override string ToString()
        {
            return $"({x},L{level},{z})";
        }
    }

    // What is inside a cell
    public enum CellKind
    {
        Empty,  // air, nothing to stand on
        Pad,    // a teleport pad you can stand on
        Solid   // filled block, like a pillar
    }

    // Type of one wall of a cell
    // Teleports ignore walls, shots and sight do not
    // Heights and opening sizes are map settings in MapDefinition
    public enum SideKind
    {
        Open,      // no wall
        Solid,     // full wall, blocks shots and sight
        HalfWall,  // waist-high wall across the whole side, crouch to hide, stand to shoot over
        Window     // solid wall with a narrow opening at chest height, lean to peek, crouch to hide
    }

    // Type of a cell's floor, which is also the ceiling of the cell below
    public enum FloorKind
    {
        Solid,  // blocks shots and sight
        Hatch,  // framed hole, shoot up and down through it
        Open    // no floor, open air
    }

    public enum Direction { North, East, South, West }

    // Where the pad sits inside its cell
    // North = +z, East = +x
    public enum PadAnchor
    {
        Center,
        North, East, South, West,
        NorthEast, SouthEast, SouthWest, NorthWest
    }

    // One cell of the grid, its position is its index in MapLayout.cells
    // Storage only, gameplay code reads the map functions instead of these fields
    // storedNorth, storedEast and storedFloor are only 3 of the cell's 6 surfaces
    // The other 3 live in neighbouring cells (see the store-once rule at the top)
    [Serializable]
    public class Cell
    {
        public CellKind kind = CellKind.Empty;
        public SideKind storedNorth = SideKind.Open;
        public SideKind storedEast = SideKind.Open;
        public FloorKind storedFloor = FloorKind.Solid;
        public PadAnchor padAnchor = PadAnchor.Center;  // pads only
        public int spawnRank;                           // pads only, 0 = not a spawn, 1 = best spawn, 2 = next best...
    }

    // All six surfaces of a cell with the store-once rule already applied
    // MapBuilder and the validator use this to get everything in one call
    public struct CellSurfaces
    {
        public SideKind north;
        public SideKind east;
        public SideKind south;
        public SideKind west;
        public FloorKind floor;
        public FloorKind ceiling;
    }

    [Serializable]
    public class MapLayout
    {
        public int width;   // cells along x
        public int depth;   // cells along z
        public int levels;  // height floors
        public Cell[] cells = Array.Empty<Cell>();

        // Total number of cells in the grid
        public int CellCount
        {
            get { return width * depth * levels; }
        }

        // Building the map

        // Wipe the map and make a new empty grid of this size
        // Every cell starts as Empty with open walls
        // Level 0 floors start Solid (the ground), higher levels start Open (air)
        // We need to make sure every slot gets its own new Cell(), otherwise all slots would share one cell
        // TODO(Moustafa) [X] Completed
        public void CreateEmpty(int newWidth, int newDepth, int newLevels)
        {
            width = newWidth;
            depth = newDepth;
            levels = newLevels;
            cells = new Cell[CellCount];
            for (int i = 0; i < cells.Length; i++)
            {
                Cell cell = new Cell();
                cell.storedFloor = CoordOf(i).level == 0 ? FloorKind.Solid : FloorKind.Open;
                cells[i] = cell;
            }
        }

        // Raw cell access for building maps only: editor tools, generator, tests
        // Gameplay code never calls this, it uses the query functions below
        // Returns null if coord is outside the map
        // TODO(Moustafa) [X] Completed
        public Cell EditCell(GridCoord coord)
        {
            if (!IsInBounds(coord))
            {
                return null;
            }
            return cells[IndexOf(coord)];
        }

        // Grid helpers

        // True if coord is inside the map on all three axes
        // Always check this before using an index, a coord just past the east edge
        // would otherwise point at a cell on the next row
        // TODO(Moustafa) [X] Completed
        public bool IsInBounds(GridCoord coord)
        {
            return coord.x >= 0 && coord.x < width
                && coord.z >= 0 && coord.z < depth
                && coord.level >= 0 && coord.level < levels;
        }

        // Array index of coord: x + width * (z + depth * level)
        // Only valid when IsInBounds(coord)
        // TODO(Moustafa) [X] Completed
        public int IndexOf(GridCoord coord)
        {
            return coord.x + width * (coord.z + depth * coord.level);
        }

        // Grid position of an array index, the reverse of IndexOf
        // TODO(Moustafa) [X] Completed
        public GridCoord CoordOf(int index)
        {
            int x = index % width;
            int rest = index / width;
            int z = rest % depth;
            int level = rest / depth;
            return new GridCoord(x, level, z);
        }

        // Queries for gameplay

        // What is inside the cell, Solid if outside the map
        // TODO(Moustafa) [X] Completed
        public CellKind GetKind(GridCoord coord)
        {
            if (!IsInBounds(coord))
            {
                return CellKind.Solid;
            }
            return cells[IndexOf(coord)].kind;
        }

        // True if coord is inside the map and is a pad
        // Every moveset filters its targets with this
        // TODO(Moustafa) [X] Completed
        public bool IsPad(GridCoord coord)
        {
            return IsInBounds(coord) && cells[IndexOf(coord)].kind == CellKind.Pad;
        }

        // Where the pad sits in its cell, Center for non-pads
        // TODO(Moustafa) [X] Completed
        public PadAnchor GetAnchor(GridCoord coord)
        {
            if (!IsPad(coord))
            {
                return PadAnchor.Center;
            }
            return cells[IndexOf(coord)].padAnchor;
        }

        // True if the cell is a pad with a spawn rank above 0
        // TODO(Moustafa) [X] Completed
        public bool IsSpawn(GridCoord coord)
        {
            return GetSpawnRank(coord) > 0;
        }

        // The spawn rank of the pad, 1 = best spawn, 0 if it is not a spawn or not a pad
        // When two spawn pads are equally far from enemies, the lower rank wins
        // TODO(Moustafa) [X] Completed
        public int GetSpawnRank(GridCoord coord)
        {
            if (!IsPad(coord))
            {
                return 0;
            }
            return Math.Max(0, cells[IndexOf(coord)].spawnRank);
        }

        // The wall of coord facing dir
        // First the two always-Solid cases:
        //   the neighbour in dir is outside the map: Solid (all four directions, the outer edge of the arena)
        //   coord or the neighbour in dir is a Solid cell: Solid
        // Otherwise the stored value, following the store-once rule:
        //   North and East come from the cell itself
        //   South comes from the north wall of coord.Offset(0, 0, -1)
        //   West comes from the east wall of coord.Offset(-1, 0, 0)
        // We need to make sure the outside check comes first: without it the north and east edges read the edge cell's
        // own stored value (Open by default) and the arena would have walls on two edges and holes on the other two
        // TODO(Moustafa) [X] Completed
        public SideKind GetSide(GridCoord coord, Direction dir)
        {
            GridCoord neighbour = Step(coord, dir);
            if (!IsInBounds(coord) || !IsInBounds(neighbour))
            {
                return SideKind.Solid;
            }
            if (GetKind(coord) == CellKind.Solid || GetKind(neighbour) == CellKind.Solid)
            {
                return SideKind.Solid;
            }
            switch (dir)
            {
                case Direction.North:
                    return cells[IndexOf(coord)].storedNorth;
                case Direction.East:
                    return cells[IndexOf(coord)].storedEast;
                case Direction.South:
                    return cells[IndexOf(neighbour)].storedNorth;
                default:
                    return cells[IndexOf(neighbour)].storedEast;
            }
        }

        // The neighbouring cell in dir (North = +z, East = +x)
        private static GridCoord Step(GridCoord coord, Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return coord.Offset(0, 0, 1);
                case Direction.East:
                    return coord.Offset(1, 0, 0);
                case Direction.South:
                    return coord.Offset(0, 0, -1);
                default:
                    return coord.Offset(-1, 0, 0);
            }
        }

        // Floor of coord, Solid if outside the map
        // TODO(Moustafa) [X] Completed
        public FloorKind GetFloor(GridCoord coord)
        {
            if (!IsInBounds(coord))
            {
                return FloorKind.Solid;
            }
            return cells[IndexOf(coord)].storedFloor;
        }

        // Ceiling of coord = the floor of the cell above
        // So a ceiling can be Solid (roof), Hatch (hole you can be shot through from above) or Open (no roof)
        // On the top level there is no cell above, so there is no ceiling, return Open
        // TODO(Moustafa) [X] Completed
        public FloorKind GetCeiling(GridCoord coord)
        {
            GridCoord above = coord.Offset(0, 1, 0);
            if (!IsInBounds(above))
            {
                return FloorKind.Open;
            }
            return GetFloor(above);
        }

        // All six surfaces of coord in one call
        // We need to make sure all six come from GetSide, GetFloor and GetCeiling so the store-once rule is applied
        // TODO(Moustafa) [X] Completed
        public CellSurfaces GetSurfaces(GridCoord coord)
        {
            CellSurfaces surfaces = new CellSurfaces();
            surfaces.north = GetSide(coord, Direction.North);
            surfaces.east = GetSide(coord, Direction.East);
            surfaces.south = GetSide(coord, Direction.South);
            surfaces.west = GetSide(coord, Direction.West);
            surfaces.floor = GetFloor(coord);
            surfaces.ceiling = GetCeiling(coord);
            return surfaces;
        }

        // Direction of an anchor as a step on the floor, each part -1, 0 or 1
        // Center = (0, 0), NorthEast = (1, 1), West = (-1, 0)
        // MapDefinition multiplies this by the push distance to get the pad position
        // TODO(Moustafa) [X] Completed
        public static (int dx, int dz) AnchorDirection(PadAnchor anchor)
        {
            switch (anchor)
            {
                case PadAnchor.North:
                    return (0, 1);
                case PadAnchor.East:
                    return (1, 0);
                case PadAnchor.South:
                    return (0, -1);
                case PadAnchor.West:
                    return (-1, 0);
                case PadAnchor.NorthEast:
                    return (1, 1);
                case PadAnchor.SouthEast:
                    return (1, -1);
                case PadAnchor.SouthWest:
                    return (-1, -1);
                case PadAnchor.NorthWest:
                    return (-1, 1);
                default:
                    return (0, 0);
            }
        }

        // Lists

        // Every pad on the map, used by the validator and the minimap
        // TODO(Moustafa) [X] Completed
        public IEnumerable<GridCoord> AllPads()
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null && cells[i].kind == CellKind.Pad)
                {
                    yield return CoordOf(i);
                }
            }
        }

        // Every spawn pad, sorted by rank (rank 1 first), the brief requires predetermined spawns
        // SpawnSelector relies on this order: on a tie it picks the first one in the list, so the best rank wins
        // We need to make sure the list is sorted by rank, not by grid position
        // TODO(Moustafa) [X] Completed
        public List<GridCoord> SpawnPads()
        {
            List<GridCoord> spawns = new List<GridCoord>();
            foreach (GridCoord pad in AllPads())
            {
                if (IsSpawn(pad))
                {
                    spawns.Add(pad);
                }
            }
            spawns.Sort((a, b) => GetSpawnRank(a).CompareTo(GetSpawnRank(b)));
            return spawns;
        }
    }
}
