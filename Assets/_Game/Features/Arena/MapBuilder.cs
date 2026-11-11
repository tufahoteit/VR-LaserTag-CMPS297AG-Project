// MapBuilder.cs
// Owner: Joseph
// Layer: Feature, Arena (local, runs the same on every headset, nothing networked)
//
// What this is
// Builds the 3D arena from a map asset (MapDefinition)
// Same map data + same code = the same arena on every headset, the brief requires every player to see the same layout
// Nothing here is sent over the network, every headset builds its own copy
//
// How it builds
// Every piece is a plain cube, scaled in code to the sizes in MapDefinition (no prefab per piece)
// so the walls in the game are exactly the sizes MapValidator checks, and there are no prefabs to set up
//   floor: Solid = one flat slab, Hatch = slabs around a 1 m hole in the middle, Open = nothing
//   side: Solid = full wall, HalfWall = low wall, Window = wall pieces around a narrow opening, Open = nothing
//   Solid cell: one big block filling the whole cell
//   pad: a flat disc at MapDefinition.PadWorldPosition, with a Pad component
//   corner posts and rails for the soft-play look
// Walls, floors, blocks and posts are on the Cover layer and have colliders, so they block shots
// Pads and rails have no collider, they never block anything
//
// Build is the boss, the helpers each build one kind of piece:
//   Build clears the old arena, then for every cell calls BuildFloor, BuildSides, then BuildSolidBlock or BuildPad
//   and BuildPosts once at the end, every helper places its cubes with PlaceCube
//
// Store-once rule
// Each cell owns its north side, east side and floor, so each wall is built once:
//   every cell builds its north and east sides
//   cells on the south edge (z = 0) also build their south side, cells on the west edge (x = 0) their west side
// Read sides and floors with GetSide and GetFloor, never the stored fields
//
// Where pieces sit
//   walls are centred on the line between two cells, half their thickness in each cell, so every room is the same size
//   floor slabs sit with their top surface exactly at the level height, so feet, pads and markers stand on the floor, not in it
//   a Hatch hole is always in the middle of the cell, a hatch pad is always pushed to a side or corner, so it stands beside the hole
//
// Solid cells
// A Solid cell is one big block, it already blocks everything around it
// So no wall is built on any side touching a Solid cell, and the Solid cell gets no floor slab
// Otherwise a wall centred on the line would poke half its thickness into the block, and two surfaces in the same spot flicker
//
// Real arena and minimap copies
// The minimap shows one small floor plan per level, BuildCopy builds one level at a time for it
// Every helper takes three values that say where and how to build:
//   parent: the object the pieces go under (this arena, or a minimap floor plan)
//   scale: 1 for the real arena, tiny for the minimap
//   forGame: true = colliders, Cover layer, Pad components; false = visuals only (minimap)
// Positions are local to parent, multiplied by scale, so the same code builds both
//
// When it builds
//   in the editor: the Build button, so you can see the map while designing
//   when the game starts: Build, which clears first, so every headset builds from the same data even if the scene holds an old copy
//   between matches: never, the same map stays
//
// Editor setup
//   1. In Main.unity, create an empty object called Arena at position (0, 0, 0)
//   2. Add the MapBuilder component
//   3. Drag Map_Test into Map, DefaultMatch into Settings, Map_Test.json into Map Json
//   4. In Assets/_Game/Materials create 5 materials: Floor, Wall, Frame, Pad, SpawnPad (any soft-play colours), drag them in
//   5. Press Import map JSON, then Build to see the arena, then Validate to check it

using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game
{
    public class MapBuilder : MonoBehaviour
    {
        [SerializeField] private MapDefinition _map;

        [Tooltip("Only used by the Validate button, for the standing eye height")]
        [SerializeField] private MatchSettings _settings;

        [Tooltip("Editor only: the map file the Import button reads")]
        [SerializeField] private TextAsset _mapJson;

        [Header("Materials")]
        [SerializeField] private Material _floorMaterial;
        [SerializeField] private Material _wallMaterial;
        [SerializeField] private Material _frameMaterial;      // posts and rails
        [SerializeField] private Material _padMaterial;
        [SerializeField] private Material _spawnPadMaterial;

        [Header("Piece sizes (m)")]
        [SerializeField, Range(0.05f, 0.3f)] private float _wallThickness = 0.1f;
        [SerializeField, Range(0.05f, 0.3f)] private float _floorThickness = 0.1f;
        [SerializeField, Range(0.05f, 0.4f)] private float _postSize = 0.15f;
        [SerializeField, Range(0.6f, 1.2f)] private float _railHeight = 1.0f;
        [SerializeField, Range(0.2f, 0.6f)] private float _padRadius = 0.4f;
        [Tooltip("Hatch hole, always in the middle of the cell: 1 m, a pad pushed to any side or corner stays clear of it")]
        [SerializeField, Range(0.6f, 1.2f)] private float _hatchHoleSize = 1.0f;

        public static MapBuilder Instance { get; private set; }

        public MapDefinition Map { get { return _map; } }
        public MatchSettings Settings { get { return _settings; } }
        public TextAsset MapJson { get { return _mapJson; } }

        // Raised after Build finishes, Minimap builds its floor plans on it
        public event Action Built;

        private readonly Dictionary<GridCoord, Pad> _pads = new Dictionary<GridCoord, Pad>();

        // When the game starts: Instance = this, then Build (which clears first)
        // TODO(Joseph) [X] Completed
        private void Awake()
        {
            Instance = this;
            if (_map == null)
            {
                Debug.LogError("[MapBuilder] No map assigned", this);
                return;
            }
            Build();
        }

        // Clears the old arena first, then builds the full-size one under this object, with colliders, and registers every Pad
        // Goes through every cell: BuildFloor, BuildSides, then BuildSolidBlock or BuildPad, then BuildPosts once at the end
        // All helpers are called with (transform, 1, true)
        // Raises Built at the end
        // TODO(Joseph) [X] Completed
        public void Build()
        {
            Clear();
            if (_map == null)
            {
                Debug.LogError("[MapBuilder] No map assigned", this);
                return;
            }

            MapLayout layout = _map.Layout;
            HashSet<Vector3> postCorners = new HashSet<Vector3>();
            for (int i = 0; i < layout.CellCount; i++)
            {
                GridCoord coord = layout.CoordOf(i);
                BuildFloor(coord, transform, 1f, true);
                BuildSides(coord, transform, 1f, true, postCorners);
                CellKind kind = layout.GetKind(coord);
                if (kind == CellKind.Solid)
                {
                    BuildSolidBlock(coord, transform, 1f, true);
                }
                else if (kind == CellKind.Pad)
                {
                    BuildPad(coord, transform, 1f, true);
                }
            }
            BuildPosts(postCorners, transform, 1f, true);

            Built?.Invoke();
        }

        // Builds a small copy of one level under parent, for the minimap's floor plans
        // Same helpers as Build, for the cells on onlyLevel only, called with (a new child of parent, scale, false):
        //   floors, walls and pads, no colliders, no Pad components, no posts or rails (too small to see)
        // Then puts every piece of the copy on layerName
        // Returns the copy's root object
        // TODO(Joseph) [X] Completed
        public GameObject BuildCopy(Transform parent, float scale, string layerName, int onlyLevel)
        {
            GameObject root = new GameObject($"MapCopy L{onlyLevel}");
            root.transform.SetParent(parent, false);
            if (_map == null)
            {
                return root;
            }

            MapLayout layout = _map.Layout;
            HashSet<Vector3> unusedCorners = new HashSet<Vector3>();
            for (int i = 0; i < layout.CellCount; i++)
            {
                GridCoord coord = layout.CoordOf(i);
                if (coord.level != onlyLevel)
                {
                    continue;
                }
                BuildFloor(coord, root.transform, scale, false);
                BuildSides(coord, root.transform, scale, false, unusedCorners);
                CellKind kind = layout.GetKind(coord);
                if (kind == CellKind.Solid)
                {
                    BuildSolidBlock(coord, root.transform, scale, false);
                }
                else if (kind == CellKind.Pad)
                {
                    BuildPad(coord, root.transform, scale, false);
                }
            }

            int layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0)
            {
                foreach (Transform piece in root.GetComponentsInChildren<Transform>(true))
                {
                    piece.gameObject.layer = layer;
                }
            }
            return root;
        }

        // Removes every built piece and clears the pad list
        // Used by the editor buttons (Build and Clear) and once when the game starts (inside Build)
        // It does not run between matches: the same map stays, only players respawn
        // It is also ready for later: staying in the lobby after a match and picking the next map
        // would be Clear, then Build with the new map
        // We need to make sure it uses DestroyImmediate in the editor (not playing) and Destroy while playing
        // TODO(Joseph) [X] Completed
        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
            _pads.Clear();
        }

        // The Pad component on a cell, or null if that cell is not a pad
        // TODO(Joseph) [X] Completed
        public Pad GetPad(GridCoord coord)
        {
            Pad pad;
            if (_pads.TryGetValue(coord, out pad))
            {
                return pad;
            }
            return null;
        }

        // Floor of one cell, from GetFloor:
        //   Solid: one slab, cellSize x floorThickness x cellSize
        //   Hatch: slabs around a square hole of hatchHoleSize, always in the middle of the cell
        //   Open: nothing
        // Skip it for a Solid cell (the block covers it)
        // We need to make sure the slab's top is at the level height: its centre goes half a thickness lower
        // TODO(Joseph) [X] Completed
        private void BuildFloor(GridCoord coord, Transform parent, float scale, bool forGame)
        {
            MapLayout layout = _map.Layout;
            if (layout.GetKind(coord) == CellKind.Solid)
            {
                return;
            }

            float cs = _map.CellSize;
            Vector3 floorCentre = _map.GridToWorld(coord) + Vector3.down * (_floorThickness * 0.5f);
            FloorKind floor = layout.GetFloor(coord);
            if (floor == FloorKind.Solid)
            {
                PlaceCube("Floor", floorCentre, new Vector3(cs, _floorThickness, cs), _floorMaterial, parent, scale, forGame, true);
            }
            else if (floor == FloorKind.Hatch)
            {
                // Four slabs around a square hole in the middle of the cell
                float hole = _hatchHoleSize;
                float strip = (cs - hole) * 0.5f;
                float offset = (cs + hole) * 0.25f;
                PlaceCube("Floor N", floorCentre + new Vector3(0f, 0f, offset), new Vector3(cs, _floorThickness, strip), _floorMaterial, parent, scale, forGame, true);
                PlaceCube("Floor S", floorCentre + new Vector3(0f, 0f, -offset), new Vector3(cs, _floorThickness, strip), _floorMaterial, parent, scale, forGame, true);
                PlaceCube("Floor E", floorCentre + new Vector3(offset, 0f, 0f), new Vector3(strip, _floorThickness, hole), _floorMaterial, parent, scale, forGame, true);
                PlaceCube("Floor W", floorCentre + new Vector3(-offset, 0f, 0f), new Vector3(strip, _floorThickness, hole), _floorMaterial, parent, scale, forGame, true);
            }
        }

        // North and east sides of one cell, plus south on the south edge and west on the west edge (store-once rule)
        // Skip any side where this cell or the neighbour is a Solid cell (the block already is the wall)
        // Each wall is centred on the line between the two cells
        // For each side, from GetSide:
        //   Solid: one wall, cellSize wide, levelHeight tall
        //   HalfWall: one wall, halfWallHeight tall
        //   Window: four pieces around the opening
        //     below: full width, windowBottom tall
        //     above: full width, from windowTop to levelHeight
        //     left and right: (cellSize - windowWidth) / 2 wide, from windowBottom to windowTop
        //   Open: nothing, but if the cell on the other side has an Open floor (a drop), add a rail at railHeight
        // Every non-Open side also adds its two end corners to postCorners, for the posts
        // TODO(Joseph) [X] Completed
        private void BuildSides(GridCoord coord, Transform parent, float scale, bool forGame, HashSet<Vector3> postCorners)
        {
            MapLayout layout = _map.Layout;
            if (layout.GetKind(coord) == CellKind.Solid)
            {
                return;
            }

            List<Direction> sides = new List<Direction> { Direction.North, Direction.East };
            if (coord.z == 0)
            {
                sides.Add(Direction.South);
            }
            if (coord.x == 0)
            {
                sides.Add(Direction.West);
            }

            float cs = _map.CellSize;
            float lh = _map.LevelHeight;
            Vector3 floorCentre = _map.GridToWorld(coord);

            foreach (Direction dir in sides)
            {
                GridCoord neighbour = Neighbour(coord, dir);
                if (layout.IsInBounds(neighbour) && layout.GetKind(neighbour) == CellKind.Solid)
                {
                    continue;
                }

                Vector3 outward = DirectionVector(dir);
                Vector3 along = (dir == Direction.North || dir == Direction.South) ? Vector3.right : Vector3.forward;
                Vector3 sideCentre = floorCentre + outward * (cs * 0.5f);
                SideKind side = layout.GetSide(coord, dir);

                switch (side)
                {
                    case SideKind.Solid:
                        PlaceWallPiece("Wall", sideCentre, along, 0f, cs, 0f, lh, parent, scale, forGame);
                        break;
                    case SideKind.HalfWall:
                        PlaceWallPiece("Half wall", sideCentre, along, 0f, cs, 0f, _map.HalfWallHeight, parent, scale, forGame);
                        break;
                    case SideKind.Window:
                        float bottom = _map.WindowBottom;
                        float top = _map.WindowTop;
                        float window = _map.WindowWidth;
                        float sideWidth = (cs - window) * 0.5f;
                        float sideOffset = (cs + window) * 0.25f;
                        PlaceWallPiece("Window below", sideCentre, along, 0f, cs, 0f, bottom, parent, scale, forGame);
                        PlaceWallPiece("Window above", sideCentre, along, 0f, cs, top, lh, parent, scale, forGame);
                        PlaceWallPiece("Window left", sideCentre, along, -sideOffset, sideWidth, bottom, top, parent, scale, forGame);
                        PlaceWallPiece("Window right", sideCentre, along, sideOffset, sideWidth, bottom, top, parent, scale, forGame);
                        break;
                    default:
                        // Open side: a rail if the cell on the other side is a drop
                        if (layout.IsInBounds(neighbour) && layout.GetFloor(neighbour) == FloorKind.Open)
                        {
                            Vector3 railSize = along == Vector3.right
                                ? new Vector3(cs, 0.05f, _postSize * 0.5f)
                                : new Vector3(_postSize * 0.5f, 0.05f, cs);
                            PlaceCube("Rail", sideCentre + Vector3.up * _railHeight, railSize, _frameMaterial, parent, scale, forGame, false);
                        }
                        break;
                }

                if (side != SideKind.Open)
                {
                    postCorners.Add(RoundCorner(sideCentre - along * (cs * 0.5f)));
                    postCorners.Add(RoundCorner(sideCentre + along * (cs * 0.5f)));
                }
            }
        }

        // A Solid cell: one block filling the whole cell
        // TODO(Joseph) [X] Completed
        private void BuildSolidBlock(GridCoord coord, Transform parent, float scale, bool forGame)
        {
            float cs = _map.CellSize;
            float lh = _map.LevelHeight;
            Vector3 centre = _map.GridToWorld(coord) + Vector3.up * (lh * 0.5f);
            PlaceCube("Block", centre, new Vector3(cs, lh, cs), _wallMaterial, parent, scale, forGame, true);
        }

        // A pad: a flat disc of padRadius at map.PadWorldPosition(coord), never a collider
        // SpawnPad material if it has a spawn rank, Pad material otherwise
        // forGame only: add a Pad component, call Init(coord, spawn rank), and add it to _pads
        // TODO(Joseph) [X] Completed
        private void BuildPad(GridCoord coord, Transform parent, float scale, bool forGame)
        {
            MapLayout layout = _map.Layout;
            int rank = layout.GetSpawnRank(coord);

            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = $"Pad {coord}";
            disc.transform.SetParent(parent, false);
            disc.transform.localPosition = (_map.PadWorldPosition(coord) + Vector3.up * 0.01f) * scale;
            disc.transform.localScale = new Vector3(_padRadius * 2f, 0.01f, _padRadius * 2f) * scale;
            RemoveCollider(disc);

            Renderer renderer = disc.GetComponent<Renderer>();
            Material material = rank > 0 ? _spawnPadMaterial : _padMaterial;
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }

            if (forGame)
            {
                Pad pad = disc.AddComponent<Pad>();
                pad.Init(coord, rank);
                _pads[coord] = pad;
            }
        }

        // One post at every corner in postCorners, postSize square, levelHeight tall, Frame material
        // The HashSet means a corner shared by two walls gets only one post
        // TODO(Joseph) [X] Completed
        private void BuildPosts(HashSet<Vector3> postCorners, Transform parent, float scale, bool forGame)
        {
            float lh = _map.LevelHeight;
            foreach (Vector3 corner in postCorners)
            {
                PlaceCube("Post", corner + Vector3.up * (lh * 0.5f), new Vector3(_postSize, lh, _postSize), _frameMaterial, parent, scale, forGame, true);
            }
        }

        // One piece of a wall on a side: offset along the wall, width, from bottom to top above the floor
        private void PlaceWallPiece(string name, Vector3 sideCentre, Vector3 along, float offset, float width,
                                    float bottom, float top, Transform parent, float scale, bool forGame)
        {
            Vector3 centre = sideCentre + along * offset + Vector3.up * ((bottom + top) * 0.5f);
            bool northSouth = along == Vector3.right;
            Vector3 size = northSouth
                ? new Vector3(width, top - bottom, _wallThickness)
                : new Vector3(_wallThickness, top - bottom, width);
            PlaceCube(name, centre, size, _wallMaterial, parent, scale, forGame, true);
        }

        // The cell next to coord in dir (North = +z, East = +x)
        private static GridCoord Neighbour(GridCoord coord, Direction dir)
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

        private static Vector3 DirectionVector(Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return Vector3.forward;
                case Direction.East:
                    return Vector3.right;
                case Direction.South:
                    return Vector3.back;
                default:
                    return Vector3.left;
            }
        }

        // Rounded to the millimetre, so the same corner reached from two cells is one entry in the HashSet
        private static Vector3 RoundCorner(Vector3 corner)
        {
            return new Vector3(Mathf.Round(corner.x * 1000f) / 1000f,
                               Mathf.Round(corner.y * 1000f) / 1000f,
                               Mathf.Round(corner.z * 1000f) / 1000f);
        }

        // Colliders must go at once in the editor (DestroyImmediate), Destroy is fine while playing
        private static void RemoveCollider(GameObject piece)
        {
            Collider collider = piece.GetComponent<Collider>();
            if (collider == null)
            {
                return;
            }
            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }

        // Makes one cube under parent: local position and size (both multiplied by scale), material
        // blocksShots and forGame both true: keep the collider and put it on the Cover layer
        // otherwise: remove the collider
        // TODO(Joseph) [X] Completed
        private GameObject PlaceCube(string name, Vector3 center, Vector3 size, Material material,
                                     Transform parent, float scale, bool forGame, bool blocksShots)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = center * scale;
            cube.transform.localScale = size * scale;
            if (material != null)
            {
                cube.GetComponent<Renderer>().sharedMaterial = material;
            }

            if (blocksShots && forGame)
            {
                int cover = LayerMask.NameToLayer(Layers.Cover);
                if (cover >= 0)
                {
                    cube.layer = cover;
                }
                else
                {
                    Debug.LogWarning("[MapBuilder] The Cover layer doesn't exist, add it in Project Settings > Tags and Layers", this);
                }
            }
            else
            {
                RemoveCollider(cube);
            }
            return cube;
        }
    }
}
