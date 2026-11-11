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
        // TODO(Joseph)
        private void Awake()
        {
        }

        // Clears the old arena first, then builds the full-size one under this object, with colliders, and registers every Pad
        // Goes through every cell: BuildFloor, BuildSides, then BuildSolidBlock or BuildPad, then BuildPosts once at the end
        // All helpers are called with (transform, 1, true)
        // Raises Built at the end
        // TODO(Joseph)
        public void Build()
        {
        }

        // Builds a small copy of one level under parent, for the minimap's floor plans
        // Same helpers as Build, for the cells on onlyLevel only, called with (a new child of parent, scale, false):
        //   floors, walls and pads, no colliders, no Pad components, no posts or rails (too small to see)
        // Then puts every piece of the copy on layerName
        // Returns the copy's root object
        // TODO(Joseph)
        public GameObject BuildCopy(Transform parent, float scale, string layerName, int onlyLevel)
        {
            return null;
        }

        // Removes every built piece and clears the pad list
        // Used by the editor buttons (Build and Clear) and once when the game starts (inside Build)
        // It does not run between matches: the same map stays, only players respawn
        // It is also ready for later: staying in the lobby after a match and picking the next map
        // would be Clear, then Build with the new map
        // We need to make sure it uses DestroyImmediate in the editor (not playing) and Destroy while playing
        // TODO(Joseph)
        public void Clear()
        {
        }

        // The Pad component on a cell, or null if that cell is not a pad
        // TODO(Joseph)
        public Pad GetPad(GridCoord coord)
        {
            return null;
        }

        // Floor of one cell, from GetFloor:
        //   Solid: one slab, cellSize x floorThickness x cellSize
        //   Hatch: slabs around a square hole of hatchHoleSize, always in the middle of the cell
        //   Open: nothing
        // Skip it for a Solid cell (the block covers it)
        // We need to make sure the slab's top is at the level height: its centre goes half a thickness lower
        // TODO(Joseph)
        private void BuildFloor(GridCoord coord, Transform parent, float scale, bool forGame)
        {
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
        // TODO(Joseph)
        private void BuildSides(GridCoord coord, Transform parent, float scale, bool forGame, HashSet<Vector3> postCorners)
        {
        }

        // A Solid cell: one block filling the whole cell
        // TODO(Joseph)
        private void BuildSolidBlock(GridCoord coord, Transform parent, float scale, bool forGame)
        {
        }

        // A pad: a flat disc of padRadius at map.PadWorldPosition(coord), never a collider
        // SpawnPad material if it has a spawn rank, Pad material otherwise
        // forGame only: add a Pad component, call Init(coord, spawn rank), and add it to _pads
        // TODO(Joseph)
        private void BuildPad(GridCoord coord, Transform parent, float scale, bool forGame)
        {
        }

        // One post at every corner in postCorners, postSize square, levelHeight tall, Frame material
        // The HashSet means a corner shared by two walls gets only one post
        // TODO(Joseph)
        private void BuildPosts(HashSet<Vector3> postCorners, Transform parent, float scale, bool forGame)
        {
        }

        // Makes one cube under parent: local position and size (both multiplied by scale), material
        // blocksShots and forGame both true: keep the collider and put it on the Cover layer
        // otherwise: remove the collider
        // TODO(Joseph)
        private GameObject PlaceCube(string name, Vector3 center, Vector3 size, Material material,
                                     Transform parent, float scale, bool forGame, bool blocksShots)
        {
            return null;
        }
    }
}
