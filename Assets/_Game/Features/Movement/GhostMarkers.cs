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
        private readonly List<GameObject> _shownObjects = new List<GameObject>();  // same order as _shown
        private MaterialPropertyBlock _block;

        // Show one marker per target (disc or arrow), reusing pooled objects, hide the rest
        // TODO(Joe) [X] Completed
        public void Show(List<MarkerTarget> targets)
        {
            if (_block == null)
            {
                _block = new MaterialPropertyBlock();
            }
            Hide();

            int discs = 0;
            int arrows = 0;
            foreach (MarkerTarget target in targets)
            {
                GameObject marker = target.isElevator
                    ? FromPool(_arrowPool, _arrowPrefab, arrows++)
                    : FromPool(_discPool, _discPrefab, discs++);
                if (marker == null)
                {
                    continue;
                }
                marker.transform.position = target.position;
                marker.SetActive(true);
                SetColor(marker, _normalColor);
                _shown.Add(target);
                _shownObjects.Add(marker);
            }
        }

        // Highlight this target (null = none), the line follows it every frame
        // TODO(Joe) [X] Completed
        public void SetSelected(MarkerTarget? target)
        {
            _selected = target;
            for (int i = 0; i < _shown.Count; i++)
            {
                bool isSelected = target.HasValue && _shown[i].pad == target.Value.pad && _shown[i].isElevator == target.Value.isElevator;
                SetColor(_shownObjects[i], isSelected ? _selectedColor : _normalColor);
            }
            if (_selectionLine != null)
            {
                _selectionLine.enabled = target.HasValue;
            }
        }

        // Hide every marker and the line
        // TODO(Joe) [X] Completed
        public void Hide()
        {
            foreach (GameObject marker in _discPool)
            {
                marker.SetActive(false);
            }
            foreach (GameObject marker in _arrowPool)
            {
                marker.SetActive(false);
            }
            _shown.Clear();
            _shownObjects.Clear();
            _selected = null;
            if (_selectionLine != null)
            {
                _selectionLine.enabled = false;
            }
        }

        // The index-th marker of a pool, made from the prefab the first time it is needed
        // The prefab is sized for 1 m at its own scale, LateUpdate scales it to the right angular size
        private GameObject FromPool(List<GameObject> pool, GameObject prefab, int index)
        {
            if (prefab == null)
            {
                return null;
            }
            while (pool.Count <= index)
            {
                GameObject marker = Instantiate(prefab);
                foreach (Collider collider in marker.GetComponentsInChildren<Collider>())
                {
                    Destroy(collider);
                }
                marker.SetActive(false);
                pool.Add(marker);
            }
            return pool[index];
        }

        // Tint one marker without making a new material
        private void SetColor(GameObject marker, Color color)
        {
            foreach (Renderer renderer in marker.GetComponentsInChildren<Renderer>())
            {
                renderer.GetPropertyBlock(_block);
                _block.SetColor("_BaseColor", color);
                renderer.SetPropertyBlock(_block);
            }
        }

        // Every frame while shown:
        //   scale each marker so it stays _angularSize degrees in your view: size = distance to your head * tan(angle)
        //   arrows turn to face you
        //   the line goes from the left controller to the selected marker
        // TODO(Joe) [X] Completed
        private void LateUpdate()
        {
            if (_shown.Count == 0 || LocalRig.Instance == null || LocalRig.Instance.Head == null)
            {
                return;
            }

            Transform head = LocalRig.Instance.Head;
            float sizePerMetre = Mathf.Tan(_angularSize * Mathf.Deg2Rad);
            for (int i = 0; i < _shownObjects.Count; i++)
            {
                Transform marker = _shownObjects[i].transform;
                float distance = Vector3.Distance(head.position, marker.position);
                GameObject prefab = _shown[i].isElevator ? _arrowPrefab : _discPrefab;
                marker.localScale = prefab.transform.localScale * (distance * sizePerMetre);

                if (_shown[i].isElevator)
                {
                    // Arrows point up above eye height and down below it, and turn to face you
                    Vector3 toHead = head.position - marker.position;
                    toHead.y = 0f;
                    Quaternion facing = toHead.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(toHead) : Quaternion.identity;
                    bool pointsUp = marker.position.y >= head.position.y;
                    marker.rotation = pointsUp ? facing : facing * Quaternion.Euler(0f, 0f, 180f);
                }
            }

            if (_selectionLine != null && _selected.HasValue && LocalRig.Instance.LeftController != null)
            {
                _selectionLine.positionCount = 2;
                _selectionLine.SetPosition(0, LocalRig.Instance.LeftController.position);
                _selectionLine.SetPosition(1, _selected.Value.position);
            }
        }
    }
}
