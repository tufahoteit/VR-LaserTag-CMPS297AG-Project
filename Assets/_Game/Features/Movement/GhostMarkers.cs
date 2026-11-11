// GhostMarkers.cs
// Owner: Joe
// Layer: Feature, Movement (local only, on your own XR rig, visuals only)
//
// What this is
// Draws the teleport markers, nothing else: no rules, no network, no decisions
// TeleportController tells it what to show and which one is selected
//
// What it draws
//   pad markers: flat discs on the pad floor, never at head height
//     (on a straight line, floor discs sit at different heights in your view, head-height markers would hide behind each other)
//   elevator arrows: small up or down arrows in front of you, for pads above or below you in your own column
//   both are visible through walls, and keep the same size in your view at any distance (about 2 to 3 degrees)
//   the selected one is highlighted, with a line from your left controller to it
//   only while the teleport stick is held
//
// Template check
//   none
//
// Editor setup
//   1. Add this component to the XR Interaction Setup (MP Variant) object, next to TeleportController
//   2. Make a disc marker prefab (flat cylinder) and an arrow marker prefab (a cone works), drag them in
//      Give both a material that draws on top of walls: a transparent unlit material with its depth test set to Always
//      (in URP: an Unlit shader with ZTest Always, or a Render Objects renderer feature that draws the markers last)
//   3. Add a LineRenderer to the same object for the selection line

using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GhostMarkers : MonoBehaviour
    {
        [SerializeField] private GameObject _discPrefab;
        [SerializeField] private GameObject _arrowPrefab;
        [SerializeField] private LineRenderer _selectionLine;
        [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color _selectedColor = new Color(0.3f, 1f, 0.4f, 1f);

        [Tooltip("Marker size in your view, in degrees, the same at any distance")]
        [SerializeField, Range(1f, 5f)] private float _angularSize = 2.5f;

        private readonly List<GameObject> _discPool = new List<GameObject>();
        private readonly List<GameObject> _arrowPool = new List<GameObject>();
        private readonly List<MarkerTarget> _shown = new List<MarkerTarget>();
        private MarkerTarget? _selected;

        // Show one marker per target (disc or arrow), reusing pooled objects, hide the rest
        // TODO(Joe)
        public void Show(List<MarkerTarget> targets)
        {
        }

        // Highlight this target (null = none), the line follows it every frame
        // TODO(Joe)
        public void SetSelected(MarkerTarget? target)
        {
        }

        // Hide every marker and the line
        // TODO(Joe)
        public void Hide()
        {
        }

        // Every frame while shown:
        //   scale each marker so it stays _angularSize degrees in your view: size = distance to your head * tan(angle)
        //   arrows turn to face you
        //   the line goes from the left controller to the selected marker
        // TODO(Joe)
        private void LateUpdate()
        {
        }
    }
}
