// MapValidator.cs
// Owner: Moustafa
// Layer: Core, pure C#, no Unity
//
// Checks a map against the map rules and lists every problem it finds
// Used by the Validate button in the editor and when generating maps (generate, validate, fix)
//
// Errors (the map is broken)
//   a pad floor is Open (a pad floor must be Solid or Hatch)
//   a Hatch pad is anchored to Center (the hole is in the middle, the player must stand beside it, not on it)
//   fewer than 8 spawn pads
//   spawn ranks have a gap or a repeat (they must be 1, 2, 3 ...)
//   a pad is exposed from fewer than 2 directions (a safe camping spot)
//   a pad can't be reached by some character
// Warnings (check it, maybe fine)
//   a pad is exposed from more than 5 directions (a death trap)
//   two spawn pads are right next to each other
// Info
//   where pads are stacked in columns (the elevators for Ray, Cluster and Ring)
//
// Exposure
// A direction is where the threat comes from, not how many pads see it
// Two shooters looking through the same window = 1 direction
// Directions are 8 horizontal sectors + "from above" + "from below" = 10 possible
// Measured for standing players (eye height), crouching to hide is not counted
//
// Geometry
// Line of sight needs real sizes (cell size, wall heights, eye height), Core has no Unity
// so MapDefinition passes them in as plain numbers (MapGeometry), all in metres

using System;
using System.Collections.Generic;

namespace Game.Core
{
    // Real sizes the validator needs, filled in by MapDefinition
    public struct MapGeometry
    {
        public float cellSize;           // 3 m
        public float levelHeight;        // 3 m
        public float padAnchorDistance;  // 0.9 m, how far an anchored pad is pushed from the cell centre
        public float eyeHeight;          // 1.65 m, standing eye height above the pad
        public float halfWallHeight;     // 1.1 to 1.3 m
        public float windowBottom;       // height of the bottom of a window opening
        public float windowTop;          // height of the top of a window opening
    }

    // Everything Validate found, as readable lines like "Pad (3,L1,2) exposed from 1 direction (needs 2 to 5)"
    public class ValidationReport
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public List<string> Info { get; } = new List<string>();

        public bool IsValid
        {
            get { return Errors.Count == 0; }
        }
    }

    public static class MapValidator
    {
        public const int MinSpawnPads = 8;
        public const int MinExposure = 2;
        public const int MaxExposure = 5;

        // Runs every check below and collects the messages
        // Order: pad floors, hatch anchors (not Center), spawns, exposure, reachability, stacked columns
        // We need to make sure one bad pad doesn't stop the other checks, collect everything, never stop at the first error
        // TODO(Moustafa) [X] Completed
        public static ValidationReport Validate(MapLayout map, IReadOnlyList<IMoveset> movesets, MapGeometry geometry)
        {
            ValidationReport report = new ValidationReport();

            foreach (GridCoord pad in map.AllPads())
            {
                FloorKind floor = map.GetFloor(pad);
                if (floor == FloorKind.Open)
                {
                    report.Errors.Add($"Pad {pad} has an Open floor (must be Solid or Hatch)");
                }
                if (floor == FloorKind.Hatch && map.GetAnchor(pad) == PadAnchor.Center)
                {
                    report.Errors.Add($"Pad {pad} has a Hatch floor but is anchored to Center (push it to a side or corner)");
                }
            }

            CheckSpawns(map, report);

            foreach (GridCoord pad in map.AllPads())
            {
                int directions = CountExposureDirections(map, pad, geometry);
                if (directions < MinExposure)
                {
                    report.Errors.Add($"Pad {pad} exposed from {directions} direction(s) (needs {MinExposure} to {MaxExposure})");
                }
                else if (directions > MaxExposure)
                {
                    report.Warnings.Add($"Pad {pad} exposed from {directions} directions (more than {MaxExposure})");
                }
            }

            CheckReachability(map, movesets, report);
            ReportStackedColumns(map, report);
            return report;
        }

        // Spawn checks
        //   error if fewer than MinSpawnPads spawn pads
        //   error if the ranks are not exactly 1, 2, 3 ... N (sort them, each must equal its position + 1)
        //   warning for every two spawn pads with PadDistance 1 on the same level
        // TODO(Moustafa) [X] Completed
        public static void CheckSpawns(MapLayout map, ValidationReport report)
        {
            List<GridCoord> spawns = map.SpawnPads();
            if (spawns.Count < MinSpawnPads)
            {
                report.Errors.Add($"Only {spawns.Count} spawn pads (needs at least {MinSpawnPads})");
            }

            // SpawnPads is already sorted by rank, so the ranks must read 1, 2, 3 ...
            for (int i = 0; i < spawns.Count; i++)
            {
                int rank = map.GetSpawnRank(spawns[i]);
                if (rank != i + 1)
                {
                    report.Errors.Add($"Spawn ranks have a gap or a repeat: expected rank {i + 1}, found {rank} at {spawns[i]}");
                    break;
                }
            }

            for (int i = 0; i < spawns.Count; i++)
            {
                for (int j = i + 1; j < spawns.Count; j++)
                {
                    if (spawns[i].level == spawns[j].level && GridCoord.PadDistance(spawns[i], spawns[j]) == 1)
                    {
                        report.Warnings.Add($"Spawn pads {spawns[i]} and {spawns[j]} are right next to each other");
                    }
                }
            }
        }

        // World position of a standing player's eyes on this pad, in metres
        //   cell centre: (x + 0.5) * cellSize, level * levelHeight, (z + 0.5) * cellSize
        //   plus MapLayout.AnchorDirection(anchor) * padAnchorDistance on the floor
        //   plus eyeHeight up
        // Returned as (x, y, z) floats, Core has no Vector3
        // We need to make sure this matches MapDefinition.PadWorldPosition, or the validator checks different spots than the game uses
        // TODO(Moustafa) [X] Completed
        public static (float x, float y, float z) EyePosition(MapLayout map, GridCoord pad, MapGeometry geometry)
        {
            (int dx, int dz) = MapLayout.AnchorDirection(map.GetAnchor(pad));
            float x = (pad.x + 0.5f) * geometry.cellSize + dx * geometry.padAnchorDistance;
            float y = pad.level * geometry.levelHeight + geometry.eyeHeight;
            float z = (pad.z + 0.5f) * geometry.cellSize + dz * geometry.padAnchorDistance;
            return (x, y, z);
        }

        // True if a standing player on pad from can see the eyes of a standing player on pad to
        // Walk along the line from EyePosition(from) to EyePosition(to) in small steps (about 0.1 m)
        // Every time the step moves into a new cell, check what was crossed:
        //   into a Solid cell: blocked
        //   a side: Solid blocks, Open passes,
        //     HalfWall blocks only if the line crosses below halfWallHeight,
        //     Window passes only if the line crosses between windowBottom and windowTop
        //   a floor or ceiling: Solid blocks, Hatch and Open pass
        // If one step crosses two sides at once (a corner), check both, blocked if either blocks
        // Players never block sight, only the map does
        // TODO(Moustafa) [X] Completed
        public static bool HasLineOfSight(MapLayout map, GridCoord from, GridCoord to, MapGeometry geometry)
        {
            const float StepSize = 0.1f;
            (float x, float y, float z) a = EyePosition(map, from, geometry);
            (float x, float y, float z) b = EyePosition(map, to, geometry);
            float distance = (float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z));
            int steps = Math.Max(1, (int)Math.Ceiling(distance / StepSize));

            GridCoord previous = CellAt(a.x, a.y, a.z, geometry);
            float previousY = a.y;
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                float x = a.x + (b.x - a.x) * t;
                float y = a.y + (b.y - a.y) * t;
                float z = a.z + (b.z - a.z) * t;
                GridCoord current = CellAt(x, y, z, geometry);
                if (current == previous)
                {
                    previousY = y;
                    continue;
                }

                // Height of the crossing above the floor of the level it happens on
                float crossingY = (previousY + y) * 0.5f - previous.level * geometry.levelHeight;

                if (current.x != previous.x)
                {
                    Direction dir = current.x > previous.x ? Direction.East : Direction.West;
                    if (SideBlocks(map.GetSide(previous, dir), crossingY, geometry))
                    {
                        return false;
                    }
                }
                if (current.z != previous.z)
                {
                    Direction dir = current.z > previous.z ? Direction.North : Direction.South;
                    if (SideBlocks(map.GetSide(previous, dir), crossingY, geometry))
                    {
                        return false;
                    }
                }
                if (current.x != previous.x && current.z != previous.z)
                {
                    // A corner: the line passes where four walls meet, the walls on the far cell count too
                    Direction backX = current.x > previous.x ? Direction.West : Direction.East;
                    Direction backZ = current.z > previous.z ? Direction.South : Direction.North;
                    GridCoord far = new GridCoord(current.x, previous.level, current.z);
                    if (SideBlocks(map.GetSide(far, backX), crossingY, geometry)
                        || SideBlocks(map.GetSide(far, backZ), crossingY, geometry))
                    {
                        return false;
                    }
                }
                if (current.level != previous.level)
                {
                    // Going up crosses the floor of the upper cell, going down crosses the floor of the cell we leave
                    int upperLevel = Math.Max(current.level, previous.level);
                    GridCoord upper = new GridCoord(current.x, upperLevel, current.z);
                    if (map.GetFloor(upper) == FloorKind.Solid)
                    {
                        return false;
                    }
                }
                if (map.GetKind(current) == CellKind.Solid)
                {
                    return false;
                }

                previous = current;
                previousY = y;
            }
            return true;
        }

        // Number of different directions pad can be shot from (0 to 10)
        // For every other pad otherPad with HasLineOfSight(otherPad, pad):
        //   take the direction from pad to otherPad
        //   if it points more than 45 degrees up: "from above", more than 45 degrees down: "from below"
        //   otherwise one of 8 horizontal sectors of 45 degrees each
        // Count how many different buckets were hit
        // TODO(Moustafa) [X] Completed
        public static int CountExposureDirections(MapLayout map, GridCoord pad, MapGeometry geometry)
        {
            const int Above = 8;
            const int Below = 9;
            HashSet<int> buckets = new HashSet<int>();
            (float x, float y, float z) eye = EyePosition(map, pad, geometry);

            foreach (GridCoord otherPad in map.AllPads())
            {
                if (otherPad == pad || !HasLineOfSight(map, otherPad, pad, geometry))
                {
                    continue;
                }
                (float x, float y, float z) other = EyePosition(map, otherPad, geometry);
                double dx = other.x - eye.x;
                double dy = other.y - eye.y;
                double dz = other.z - eye.z;
                double horizontal = Math.Sqrt(dx * dx + dz * dz);
                double elevation = Math.Atan2(dy, horizontal) * 180.0 / Math.PI;

                if (elevation > 45.0)
                {
                    buckets.Add(Above);
                }
                else if (elevation < -45.0)
                {
                    buckets.Add(Below);
                }
                else
                {
                    double heading = Math.Atan2(dz, dx) * 180.0 / Math.PI;
                    int sector = (int)Math.Floor((heading + 360.0 + 22.5) / 45.0) % 8;
                    buckets.Add(sector);
                }
            }
            return buckets.Count;
        }

        // Every pad this moveset can reach starting from start, any number of jumps
        // Breadth-first search: a queue of pads, for each one add moveset.GetReachable(pad, map, coord => false)
        // isOccupied is always false here, the validator checks the empty map
        // TODO(Moustafa) [X] Completed
        public static HashSet<GridCoord> Reachable(MapLayout map, IMoveset moveset, GridCoord start)
        {
            HashSet<GridCoord> visited = new HashSet<GridCoord>();
            Queue<GridCoord> queue = new Queue<GridCoord>();
            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                GridCoord pad = queue.Dequeue();
                foreach (GridCoord next in moveset.GetReachable(pad, map, NobodyThere))
                {
                    if (visited.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }
            return visited;
        }

        // Reachability check, for each moveset:
        //   run Reachable from the rank 1 spawn pad
        //   error for every pad not in the result, e.g. "Ring can't reach (4,L2,1)"
        // One search per moveset is enough: every moveset is symmetric (if A can jump to B, B can jump to A),
        // so if every pad is reachable from one spawn, every pad is reachable from every spawn
        // TODO(Moustafa) [X] Completed
        public static void CheckReachability(MapLayout map, IReadOnlyList<IMoveset> movesets, ValidationReport report)
        {
            List<GridCoord> spawns = map.SpawnPads();
            GridCoord? start = null;
            if (spawns.Count > 0)
            {
                start = spawns[0];
            }
            else
            {
                foreach (GridCoord pad in map.AllPads())
                {
                    start = pad;
                    break;
                }
            }
            if (start == null)
            {
                report.Errors.Add("The map has no pads");
                return;
            }

            foreach (IMoveset moveset in movesets)
            {
                HashSet<GridCoord> reached = Reachable(map, moveset, start.Value);
                foreach (GridCoord pad in map.AllPads())
                {
                    if (!reached.Contains(pad))
                    {
                        report.Errors.Add($"{moveset.Kind} can't reach {pad}");
                    }
                }
            }
        }

        // The cell a world point is in, the same rounding down as MapDefinition.WorldToGrid
        private static GridCoord CellAt(float x, float y, float z, MapGeometry geometry)
        {
            return new GridCoord((int)Math.Floor(x / geometry.cellSize),
                                 (int)Math.Floor(y / geometry.levelHeight),
                                 (int)Math.Floor(z / geometry.cellSize));
        }

        // Does this side stop a line that crosses it at this height above the floor
        private static bool SideBlocks(SideKind side, float crossingHeight, MapGeometry geometry)
        {
            switch (side)
            {
                case SideKind.Solid:
                    return true;
                case SideKind.HalfWall:
                    return crossingHeight < geometry.halfWallHeight;
                case SideKind.Window:
                    return crossingHeight < geometry.windowBottom || crossingHeight > geometry.windowTop;
                default:
                    return false;
            }
        }

        // The validator checks the empty map, nobody stands anywhere
        private static bool NobodyThere(GridCoord coord)
        {
            return false;
        }

        // Info line for every column (same x and z) that has pads on 2 or more levels
        // e.g. "Column (2,4): pads on L0, L1, L2"
        // TODO(Moustafa) [X] Completed
        public static void ReportStackedColumns(MapLayout map, ValidationReport report)
        {
            for (int x = 0; x < map.width; x++)
            {
                for (int z = 0; z < map.depth; z++)
                {
                    List<string> levels = new List<string>();
                    for (int level = 0; level < map.levels; level++)
                    {
                        if (map.IsPad(new GridCoord(x, level, z)))
                        {
                            levels.Add("L" + level);
                        }
                    }
                    if (levels.Count >= 2)
                    {
                        report.Info.Add($"Column ({x},{z}): pads on {string.Join(", ", levels)}");
                    }
                }
            }
        }
    }
}
