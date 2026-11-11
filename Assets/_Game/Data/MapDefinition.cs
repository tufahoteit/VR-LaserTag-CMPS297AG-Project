// MapDefinition.cs
// Owner: Joseph
// Layer: Data, ScriptableObject
//
// What this is
// The bridge between the pure map and Unity
// The pure side (Core) only knows cells: MapLayout says "pad (2,L1,4) has a window on its north side"
// Unity needs real things: a file it can save, and positions in metres like (7.5, 3, 13.5)
// This file does both:
//   saving: MapLayout is pure C#, so it can't be an asset by itself, this file wraps it as one (e.g. Map_Test)
//   converting: it turns a cell into a position in metres, and a position back into a cell
//
// Pure side (cells)              this file                         Unity side (metres)
//   MapLayout                    saves it as an asset              a map file in the Project window
//   GridCoord (2,L1,4)           GridToWorld, PadWorldPosition     Vector3 (7.5, 3, 13.5)
//   cell (2,L1,4)                WorldToGrid                       a point in the world
//   "Window", "HalfWall"         real sizes (1.2 m, 0.6 m wide)    MapBuilder makes the wall objects
//
// The rules (movesets, MapValidator, SpawnSelector) never see metres, they only think in cells
// Anything that puts something in the world asks this file, so nobody does their own maths
// and a ghost marker always sits exactly where you land
//
// Not to be confused with UnityConversions
//   MapDefinition: cell <-> position in metres (where things are in the world)
//   UnityConversions: GridCoord <-> Vector3Int, only so a cell can be sent over the network, no metres
//
// Positions
// origin is the corner of cell (0,L0,0), a cell centre is half a cell in from its corner
// A pad's floor position = its cell centre, pushed padAnchorDistance toward its anchor
//
// Who reads it
//   MapBuilder: builds the arena from the layout and the sizes
//   TeleportController, Minimap: PadWorldPosition (TeleportController passes marker positions on to GhostMarkers)
//   MapBuilderEditor: Geometry, for the Validate button
//
// Editor setup
//   1. In the Project window, open Assets/_Game/Data and create a folder called Maps
//   2. Right-click inside it, Create, Game, Map, name the new asset Map_Test
//   3. The default sizes are already the decided ones
//   4. The layout is not filled in by hand in the Inspector, it comes from map data through the MapBuilder buttons

using Game.Core;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Game/Map", fileName = "NewMap")]
    public class MapDefinition : ScriptableObject
    {
        [SerializeField] private MapLayout _layout = new MapLayout();

        [Header("Grid size")]
        [Tooltip("Width and depth of one cell in metres: 3")]
        [SerializeField, Range(1.5f, 6f)] private float _cellSize = 3f;
        [Tooltip("Height of one level in metres: 3")]
        [SerializeField, Range(2f, 5f)] private float _levelHeight = 3f;
        [Tooltip("World position of the corner of cell (0,L0,0)")]
        [SerializeField] private Vector3 _origin = Vector3.zero;

        [Header("Pads")]
        [Tooltip("How far an anchored pad is pushed from the cell centre toward its side or corner: 0.9 m")]
        [SerializeField, Range(0f, 1.4f)] private float _padAnchorDistance = 0.9f;

        [Header("Walls")]
        [Tooltip("Half wall height: crouch to hide, stand to shoot over: 1.2 m")]
        [SerializeField, Range(0.8f, 1.6f)] private float _halfWallHeight = 1.2f;
        [Tooltip("Bottom of a window opening, above a crouched head: 1.2 m")]
        [SerializeField, Range(0.5f, 2f)] private float _windowBottom = 1.2f;
        [Tooltip("Top of a window opening, above a standing head (eyes at 1.65 m): 1.8 m")]
        [SerializeField, Range(1f, 2.5f)] private float _windowTop = 1.8f;
        [Tooltip("Width of a window opening, narrow so you lean to peek: 0.6 m")]
        [SerializeField, Range(0.2f, 2f)] private float _windowWidth = 0.6f;

        public MapLayout Layout { get { return _layout; } }
        public float CellSize { get { return _cellSize; } }
        public float LevelHeight { get { return _levelHeight; } }
        public float HalfWallHeight { get { return _halfWallHeight; } }
        public float WindowBottom { get { return _windowBottom; } }
        public float WindowTop { get { return _windowTop; } }
        public float WindowWidth { get { return _windowWidth; } }

        // Floor centre of a cell in world space
        //   origin + ((x + 0.5) * cellSize, level * levelHeight, (z + 0.5) * cellSize)
        // TODO(Joseph)
        public Vector3 GridToWorld(GridCoord coord)
        {
            return Vector3.zero;
        }

        // The cell a world position is in, the reverse of GridToWorld
        // Round down: x = floor((world.x - origin.x) / cellSize), same for level and z
        // TODO(Joseph)
        public GridCoord WorldToGrid(Vector3 world)
        {
            return default;
        }

        // Where a player's feet go on this pad
        //   GridToWorld(coord) + (dx, 0, dz) * padAnchorDistance, with (dx, dz) = MapLayout.AnchorDirection(anchor)
        // Ghost markers, spawning, teleporting and the minimap all use this
        // We need to make sure this matches MapValidator.EyePosition (same maths, minus the eye height)
        // TODO(Joseph)
        public Vector3 PadWorldPosition(GridCoord coord)
        {
            return Vector3.zero;
        }

        // The sizes MapValidator needs for line of sight, as plain numbers
        // eyeHeight comes from MatchSettings.StandardEyeHeight
        // TODO(Joseph)
        public MapGeometry Geometry(float eyeHeight)
        {
            return default;
        }

        // Unity calls this in the editor whenever a value changes, never during the game
        // Warn if: windowBottom is not below windowTop, the standing eye height (1.65) is not inside the window opening,
        // halfWallHeight is above the standing eye height (you could never shoot over it)
        // TODO(Joseph)
        private void OnValidate()
        {
        }
    }
}
