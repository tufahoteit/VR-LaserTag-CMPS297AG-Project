// Minimap.cs
// Owner: Ali
// Layer: Feature, UI (local only, on the MinimapRoot of your board, view only)
//
// What this is
// The map on your board: one floor plan per level, side by side (L0, L1, L2), like the floor directory in a mall
// A dot for every living player in their colour, on the floor plan of the level they are on
// Full information: every player is always visible here, the minimap is the position info of this game
// (there is no through-wall highlight in 3D, decided)
//
// Why one plan per level
// A single 3D copy would stack the levels on top of each other, from above you couldn't tell who is on which level
// With one plan per level, one glance tells you everything, and it reads clearly in a screen recording
// Each plan shows only its own level, so nothing hides anything: floors, walls (seen from above, thin lines) and pads
// So each plan is a real floor plan: you see the rooms, the windows and the openings
//
// How it looks
//   each plan is MapBuilder.BuildCopy of one level, its height squashed (about 0.2) so it reads like a flat plan
//   your current level's plan has an outline, so you always know which floor you are on
//   north stays up on the board: the maps never turn when you turn, so a plan always looks the same
//   your own dot has a small arrow showing where you are facing
//
// The dots
//   one per player on a pad, at their CurrentPad, your own at TeleportController.PredictedPad (it moves the moment you teleport)
//   hidden while a player is dead or in the lobby area, removed when a player leaves (GameEvents.PlayerLeft)
//   built in code: small spheres, coloured with a MaterialPropertyBlock (PlayerProfile.Color)
//   the dots are not squashed: they sit under MinimapRoot and are placed on the right plan each frame
//
// Size
// Each plan is _planSize wide: scale = _planSize / (map width * cell size), the plans are _planGap apart
//
// Template check
//   none
//
// Editor setup
//   1. On the board, select MinimapRoot (top part of the board) and add this component
//   2. Turn MinimapRoot so its green arrow (up) points out of the board toward your eyes and its blue arrow (forward, north)
//      points to the top of the board
//   3. Drag an unlit material into Dot Material

using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game
{
    public class Minimap : MonoBehaviour
    {
        [Tooltip("Width of one level's floor plan on the board, in metres: 0.1")]
        [SerializeField, Range(0.04f, 0.2f)] private float _planSize = 0.1f;

        [Tooltip("Space between two floor plans, in metres")]
        [SerializeField, Range(0f, 0.05f)] private float _planGap = 0.01f;

        [Tooltip("How much the plans are flattened, 1 = true height, 0.2 = reads like a flat plan")]
        [SerializeField, Range(0.05f, 1f)] private float _heightSquash = 0.2f;

        [Tooltip("Unlit material for the dots, your facing arrow and the level outline, coloured in code")]
        [SerializeField] private Material _dotMaterial;

        [SerializeField, Range(0.003f, 0.02f)] private float _dotSize = 0.008f;

        private float _scale;
        private readonly List<Transform> _plans = new List<Transform>();   // one per level, index = level
        private readonly Dictionary<ulong, Transform> _dots = new Dictionary<ulong, Transform>();
        private Transform _arrow;
        private Transform _outline;
        private MaterialPropertyBlock _block;
        private readonly List<Vector3> _levelCentres = new List<Vector3>();  // world centre of each level's floor

        // Subscribe to MapBuilder.Instance.Built (or BuildPlans now if the arena is already built) and GameEvents.PlayerLeft
        // TODO(Ali) [X] Completed
        private void Start()
        {
            _block = new MaterialPropertyBlock();
            GameEvents.PlayerLeft += OnPlayerLeft;
            if (MapBuilder.Instance == null)
            {
                Debug.LogError("[Minimap] No MapBuilder in the scene", this);
                return;
            }
            MapBuilder.Instance.Built += BuildPlans;
            if (MapBuilder.Instance.transform.childCount > 0)
            {
                BuildPlans();
            }
        }

        // Unsubscribe
        // TODO(Ali) [X] Completed
        private void OnDestroy()
        {
            GameEvents.PlayerLeft -= OnPlayerLeft;
            if (MapBuilder.Instance != null)
            {
                MapBuilder.Instance.Built -= BuildPlans;
            }
        }

        // _scale from _planSize, then for every level:
        //   a plan object under this one, moved along x so the plans sit side by side, centred, L0 on the left
        //   its height squashed (localScale y = _heightSquash)
        //   MapBuilder.Instance.BuildCopy(plan, _scale, layer Default, that level)
        //   the copy shifted so the level's floor centre sits on the plan object
        // Then MakeOutline, and MakeArrow for your own dot
        // TODO(Ali) [X] Completed
        private void BuildPlans()
        {
            foreach (Transform plan in _plans)
            {
                if (plan != null)
                {
                    Destroy(plan.gameObject);
                }
            }
            _plans.Clear();
            _levelCentres.Clear();

            MapDefinition map = MapBuilder.Instance.Map;
            MapLayout layout = map.Layout;
            _scale = _planSize / (layout.width * map.CellSize);
            float total = layout.levels * _planSize + (layout.levels - 1) * _planGap;

            for (int level = 0; level < layout.levels; level++)
            {
                Transform plan = new GameObject($"Plan L{level}").transform;
                plan.SetParent(transform, false);
                plan.localPosition = new Vector3(-total * 0.5f + _planSize * 0.5f + level * (_planSize + _planGap), 0f, 0f);
                plan.localScale = new Vector3(1f, _heightSquash, 1f);

                Vector3 corner = map.GridToWorld(new GridCoord(0, level, 0));
                Vector3 farCorner = map.GridToWorld(new GridCoord(layout.width - 1, level, layout.depth - 1));
                Vector3 centre = (corner + farCorner) * 0.5f;
                _levelCentres.Add(centre);

                GameObject copy = MapBuilder.Instance.BuildCopy(plan, _scale, "Default", level);
                copy.transform.localPosition = -centre * _scale;
                _plans.Add(plan);
            }

            if (_outline == null)
            {
                _outline = MakeOutline();
            }
            if (_arrow == null)
            {
                _arrow = MakeArrow();
            }
        }

        // Every frame, after everything has moved:
        //   for every PlayerMovement in All: on a pad (yours: PredictedPad has a value) -> its dot on _plans[pad.level],
        //     at that plan's point for MapDefinition.PadWorldPosition(pad) * _scale, a little above the plan, shown; otherwise hidden
        //   a player with no dot yet gets one (MakeDot in their colour)
        //   your arrow: on your dot, turned to where your head faces on the floor (LocalRig.Instance.Head forward, ignoring height)
        //   the outline: around the plan of your current level, hidden while you are in the lobby area
        // TODO(Ali) [X] Completed
        private void LateUpdate()
        {
            if (_plans.Count == 0 || MapBuilder.Instance == null)
            {
                return;
            }

            MapDefinition map = MapBuilder.Instance.Map;
            GridCoord? myPad = null;
            Vector3 myDot = Vector3.zero;

            foreach (PlayerMovement player in PlayerMovement.All)
            {
                GridCoord? pad = null;
                if (player == PlayerMovement.Local)
                {
                    pad = TeleportController.Instance != null ? TeleportController.Instance.PredictedPad : null;
                }
                else if (player.IsOnBoard)
                {
                    pad = player.CurrentGrid;
                }

                PlayerProfile profile = player.GetComponent<PlayerProfile>();
                Color color = profile != null ? profile.Color : Color.white;
                Transform dot;
                if (!_dots.TryGetValue(player.OwnerClientId, out dot) || dot == null)
                {
                    dot = MakeDot(color);
                    _dots[player.OwnerClientId] = dot;
                }

                if (!pad.HasValue || pad.Value.level < 0 || pad.Value.level >= _plans.Count)
                {
                    dot.gameObject.SetActive(false);
                    continue;
                }

                int level = pad.Value.level;
                Vector3 offset = (map.PadWorldPosition(pad.Value) - _levelCentres[level]) * _scale;
                dot.localPosition = _plans[level].localPosition + new Vector3(offset.x, _dotSize, offset.z);
                dot.gameObject.SetActive(true);
                SetColor(dot, color);

                if (player == PlayerMovement.Local)
                {
                    myPad = pad;
                    myDot = dot.localPosition;
                }
            }

            bool showMine = myPad.HasValue && LocalRig.Instance != null && LocalRig.Instance.Head != null;
            _arrow.gameObject.SetActive(showMine);
            _outline.gameObject.SetActive(showMine);
            if (showMine)
            {
                Vector3 facing = LocalRig.Instance.Head.forward;
                float yaw = Mathf.Atan2(facing.x, facing.z) * Mathf.Rad2Deg;
                Quaternion turn = Quaternion.Euler(0f, yaw, 0f);
                _arrow.localRotation = turn;
                _arrow.localPosition = myDot + turn * Vector3.forward * (_dotSize * 0.8f);
                _outline.localPosition = _plans[myPad.Value.level].localPosition;
            }
        }

        // Listener for GameEvents.PlayerLeft (runs by itself when a player disconnects, connected in Start)
        // Destroy their dot and forget it
        // TODO(Ali) [X] Completed
        private void OnPlayerLeft(ulong clientId)
        {
            Transform dot;
            if (_dots.TryGetValue(clientId, out dot))
            {
                if (dot != null)
                {
                    Destroy(dot.gameObject);
                }
                _dots.Remove(clientId);
            }
        }

        // Tint a piece without making a new material
        private void SetColor(Transform piece, Color color)
        {
            Renderer renderer = piece.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }
            renderer.GetPropertyBlock(_block);
            _block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(_block);
        }

        // A small sphere under this object in color, no collider
        // TODO(Ali) [X] Completed
        private Transform MakeDot(Color color)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = "Dot";
            Destroy(dot.GetComponent<Collider>());
            dot.transform.SetParent(transform, false);
            dot.transform.localScale = Vector3.one * _dotSize;
            if (_dotMaterial != null)
            {
                dot.GetComponent<Renderer>().sharedMaterial = _dotMaterial;
            }
            SetColor(dot.transform, color);
            return dot.transform;
        }

        // A short flat nose sticking out of your dot for your facing, no collider
        // TODO(Ali) [X] Completed
        private Transform MakeArrow()
        {
            GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "Facing";
            Destroy(arrow.GetComponent<Collider>());
            arrow.transform.SetParent(transform, false);
            arrow.transform.localScale = new Vector3(_dotSize * 0.4f, _dotSize * 0.2f, _dotSize * 0.7f);
            if (_dotMaterial != null)
            {
                arrow.GetComponent<Renderer>().sharedMaterial = _dotMaterial;
            }
            SetColor(arrow.transform, Color.white);
            return arrow.transform;
        }

        // A thin square frame (four flat cubes) around one plan, white, no collider, moved to your level's plan every frame
        // TODO(Ali) [X] Completed
        private Transform MakeOutline()
        {
            Transform outline = new GameObject("Level outline").transform;
            outline.SetParent(transform, false);
            float size = _planSize * 1.05f;
            float thickness = _planSize * 0.015f;
            Vector3[] positions = { new Vector3(0f, 0f, size * 0.5f), new Vector3(0f, 0f, -size * 0.5f),
                                    new Vector3(size * 0.5f, 0f, 0f), new Vector3(-size * 0.5f, 0f, 0f) };
            Vector3[] scales = { new Vector3(size, thickness, thickness), new Vector3(size, thickness, thickness),
                                 new Vector3(thickness, thickness, size), new Vector3(thickness, thickness, size) };
            for (int i = 0; i < 4; i++)
            {
                GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                edge.name = "Edge";
                Destroy(edge.GetComponent<Collider>());
                edge.transform.SetParent(outline, false);
                edge.transform.localPosition = positions[i];
                edge.transform.localScale = scales[i];
                if (_dotMaterial != null)
                {
                    edge.GetComponent<Renderer>().sharedMaterial = _dotMaterial;
                }
                SetColor(edge.transform, Color.white);
            }
            return outline;
        }
    }
}
