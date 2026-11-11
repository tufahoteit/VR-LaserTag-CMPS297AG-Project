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

        // Subscribe to MapBuilder.Instance.Built (or BuildPlans now if the arena is already built) and GameEvents.PlayerLeft
        // TODO(Ali)
        private void Start()
        {
        }

        // Unsubscribe
        // TODO(Ali)
        private void OnDestroy()
        {
        }

        // _scale from _planSize, then for every level:
        //   a plan object under this one, moved along x so the plans sit side by side, centred, L0 on the left
        //   its height squashed (localScale y = _heightSquash)
        //   MapBuilder.Instance.BuildCopy(plan, _scale, layer Default, that level)
        //   the copy shifted so the level's floor centre sits on the plan object
        // Then MakeOutline, and MakeArrow for your own dot
        // TODO(Ali)
        private void BuildPlans()
        {
        }

        // Every frame, after everything has moved:
        //   for every PlayerMovement in All: on a pad (yours: PredictedPad has a value) -> its dot on _plans[pad.level],
        //     at that plan's point for MapDefinition.PadWorldPosition(pad) * _scale, a little above the plan, shown; otherwise hidden
        //   a player with no dot yet gets one (MakeDot in their colour)
        //   your arrow: on your dot, turned to where your head faces on the floor (LocalRig.Instance.Head forward, ignoring height)
        //   the outline: around the plan of your current level, hidden while you are in the lobby area
        // TODO(Ali)
        private void LateUpdate()
        {
        }

        // Listener for GameEvents.PlayerLeft (runs by itself when a player disconnects, connected in Start)
        // Destroy their dot and forget it
        // TODO(Ali)
        private void OnPlayerLeft(ulong clientId)
        {
        }

        // A small sphere under this object in color, no collider
        // TODO(Ali)
        private Transform MakeDot(Color color)
        {
            return null;
        }

        // A small flat triangle or cone under this object for your facing, no collider
        // TODO(Ali)
        private Transform MakeArrow()
        {
            return null;
        }

        // A thin square frame (four flat cubes) around one plan, white, no collider, moved to your level's plan every frame
        // TODO(Ali)
        private Transform MakeOutline()
        {
            return null;
        }
    }
}
