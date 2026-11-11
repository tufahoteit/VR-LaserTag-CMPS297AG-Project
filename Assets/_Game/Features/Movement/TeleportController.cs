// TeleportController.cs
// Owner: Moustafa
// Layer: Feature, Movement (local only, on your own XR rig, one per headset)
//
// What this is
// Your side of teleporting: what you can reach, what you are pointing at, and moving you
// It decides, GhostMarkers only draws, PlayerMovement (on the host) has the final word
//
// What the player does
//   hold the left thumbstick forward: markers appear on every pad you can reach, in every direction, through walls
//   point the left controller: the marker closest to where you point highlights, with a line to it
//   release: you are there instantly (the picture just changes)
//   cancel: release with nothing highlighted, or press the left grip while aiming, nothing happens and no charge is spent
//   no charges, or no reachable pad: no markers, releasing does nothing
//
// Picking by angle
// No raycast: for every marker, take the angle between where your left controller points and the direction to the marker
// The smallest angle wins, if it is under MaxSelectAngle
// On a straight line, floor markers are at different heights in your view (close = low, far = higher), so each has its own angle
//
// Going up and down your own column (the elevator)
// Pads straight above or below you would all sit in the same direction (straight up or down), impossible to point at
// So they get their own markers: small arrows floating 1 m in front of you, stacked (one per reachable level up or down)
// You point at an arrow and release, exactly like a pad, no extra button
// Like elevator buttons in other VR games: normal VR teleport never targets a spot straight above your head either
//
// Prediction
// You move instantly and the request goes to the host at the same time, if the host refuses you snap back
// Your headset enforces the real 0.3 s gap and counts charges still waiting for the host, so you can't spend one twice
//
// Facing
// A normal teleport never turns you, you keep facing the way you were (turning you mid-fight would lose track of enemies)
// Only a spawn turns you to face the arena centre, so you never spawn staring at a wall
//
// Template check (done)
//   aiming: XRI Left Locomotion/Teleport Mode, the left stick pushed forward (a Sector interaction, forward only)
//     it is "performed" while the stick is in the forward sector, aiming ends when it leaves (released or pulled away)
//   cancel: XRI Left Locomotion/Teleport Mode Cancel, the left grip
//   the template's own teleport (Teleportation provider and Teleport Interactor) is turned off in the cleanup,
//   otherwise the same stick would also fire the template's teleport ray
//
// Editor setup
//   1. Add this component to the XR Interaction Setup (MP Variant) object, next to LocalRig
//   2. Drag DefaultMatch into Settings, XRI Left Locomotion/Teleport Mode into Aim Action,
//      XRI Left Locomotion/Teleport Mode Cancel into Cancel Action
//   3. Create an empty object above the lobby area platform called LobbyAreaPoint, drag it into Lobby Area Point

using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    // One marker to draw and to point at
    public struct MarkerTarget
    {
        public GridCoord pad;        // where you teleport if you pick it
        public Vector3 position;     // where the marker is drawn and where you point
        public bool isElevator;      // true: an arrow in front of you (own column), false: a disc on the pad
    }

    public class TeleportController : MonoBehaviour
    {
        [SerializeField] private MatchSettings _settings;

        [Tooltip("Left stick forward: XRI Left Locomotion/Teleport Mode")]
        [SerializeField] private InputActionReference _aimAction;

        [Tooltip("Left grip: XRI Left Locomotion/Teleport Mode Cancel")]
        [SerializeField] private InputActionReference _cancelAction;

        [Tooltip("Where you stand while dead or waiting, in the lobby area")]
        [SerializeField] private Transform _lobbyAreaPoint;

        [Tooltip("Your lobby panel, moved to the front left of your own lobby spot (local to your headset)")]
        [SerializeField] private Transform _lobbyPanels;

        [Tooltip("The template's room code sign, moved to the front right of your own lobby spot (local to your headset)")]
        [SerializeField] private Transform _roomCodeSign;

        [Tooltip("Largest angle (degrees) between your pointing and a marker for it to be picked")]
        [SerializeField, Range(2f, 20f)] private float _maxSelectAngle = 8f;

        [Header("Elevator arrows")]
        [SerializeField, Range(0.5f, 2f)] private float _arrowDistance = 1f;      // in front of your head
        [SerializeField, Range(0.15f, 0.6f)] private float _arrowSpacing = 0.3f;  // between stacked arrows

        public static TeleportController Instance { get; private set; }

        // The pad you are on as far as your headset knows (moved instantly, confirmed later), null in the lobby area
        public GridCoord? PredictedPad { get; private set; }

        // Teleports your headset already made but the host hasn't answered yet
        // PlayerHUD subtracts them from Charges, so a charge light goes off the moment you teleport
        public int PendingRequests { get { return _pendingRequests; } }

        private GhostMarkers _markers;
        private readonly List<MarkerTarget> _targets = new List<MarkerTarget>();
        private MarkerTarget? _selected;
        private double _lastTeleportTime = double.NegativeInfinity;
        private int _pendingRequests;
        private bool _wasAiming;     // aiming last frame, to see when it starts and ends
        private bool _placedInLobby; // moved to your own lobby spot once after joining
        private int _placedSlot = -1; // the colour slot of that spot, the host's value may arrive a frame later

        // Lobby spots: one per colour slot (0 to 4) in a row, so players never stand inside each other
        private const float LobbySpotSpacing = 1f;
        // Where your lobby panels float: in front of your spot, the panel to the left and the room code to the right,
        // so the middle stays free for the template's own menus further away
        private const float LobbyPanelDistance = 1.5f;
        private const float LobbyPanelHeight = 1.4f;
        private const float LobbyPanelSide = 0.8f;
        private const float LobbyPanelTurn = 20f;
        private bool _aimCancelled;  // the grip cancelled this aim, nothing happens until the stick is released

        // Instance = this, find GhostMarkers on this object
        // TODO(Moustafa) [X] Completed
        private void Awake()
        {
            Instance = this;
            _markers = GetComponent<GhostMarkers>();
            if (_markers == null)
            {
                Debug.LogError("[TeleportController] No GhostMarkers on this object", this);
            }
        }

        // Enable the aim and cancel actions
        // TODO(Moustafa) [X] Completed
        private void OnEnable()
        {
            if (_aimAction != null)
            {
                _aimAction.action.Enable();
            }
            if (_cancelAction != null)
            {
                _cancelAction.action.Enable();
            }
        }

        // Disable them
        // TODO(Moustafa) [X] Completed
        private void OnDisable()
        {
            if (_aimAction != null)
            {
                _aimAction.action.Disable();
            }
            if (_cancelAction != null)
            {
                _cancelAction.action.Disable();
            }
        }

        // Every frame, aiming = _aimAction.action.phase == InputActionPhase.Performed, compared with last frame:
        //   aiming started: BuildTargets, if any: _markers.Show(_targets)
        //   still aiming: UpdateSelection, cancel pressed this frame: clear _selected, _markers.Hide(), _aimCancelled = true until aiming ends
        //   aiming ended: if _selected has a value: TryTeleport, then _markers.Hide()
        // TODO(Moustafa) [X] Completed
        private void Update()
        {
            // Right after you join, move to your own lobby spot (the template starts everyone on the same spot)
            // Again if your colour slot arrives a frame later, while you are still waiting in the lobby area
            if (PlayerMovement.Local == null || PlayerProfile.Local == null)
            {
                _placedInLobby = false;
                _placedSlot = -1;
            }
            else if (LocalRig.Instance != null && !PlayerMovement.Local.IsOnBoard
                     && (!_placedInLobby || PlayerProfile.Local.ColorIndex.Value != _placedSlot))
            {
                _placedInLobby = true;
                _placedSlot = PlayerProfile.Local.ColorIndex.Value;
                GoToLobbyArea();
            }

            if (_aimAction == null || PlayerMovement.Local == null || LocalRig.Instance == null)
            {
                return;
            }

            bool aiming = _aimAction.action.phase == InputActionPhase.Performed;
            if (aiming && !_wasAiming)
            {
                _aimCancelled = false;
                _selected = null;
                BuildTargets();
                if (_targets.Count > 0)
                {
                    _markers.Show(_targets);
                }
            }
            else if (aiming && !_aimCancelled)
            {
                if (_targets.Count > 0)
                {
                    UpdateSelection();
                }
                if (_cancelAction != null && _cancelAction.action.WasPressedThisFrame())
                {
                    _selected = null;
                    _markers.Hide();
                    _aimCancelled = true;
                }
            }
            else if (!aiming && _wasAiming)
            {
                if (!_aimCancelled && _selected.HasValue && CanTeleportNow())
                {
                    TryTeleport(_selected.Value.pad);
                }
                _selected = null;
                _markers.Hide();
            }
            _wasAiming = aiming;
        }

        // Can you teleport right now: on a pad, match Playing, Charges - _pendingRequests above 0, 0.3 s since the last one
        // TODO(Moustafa) [X] Completed
        private bool CanTeleportNow()
        {
            PlayerMovement player = PlayerMovement.Local;
            if (player == null || !PredictedPad.HasValue)
            {
                return false;
            }
            if (MatchManager.Instance != null && MatchManager.Instance.Phase.Value != MatchPhase.Playing)
            {
                return false;
            }
            if (player.Charges.Value - _pendingRequests <= 0)
            {
                return false;
            }
            return Time.timeAsDouble - _lastTeleportTime >= _settings.MinTeleportGap;
        }

        // Fill _targets from PlayerMovement.Local.GetReachablePads(PredictedPad)
        //   pad in another column: a disc at map.PadWorldPosition(pad)
        //   pad in your own column: an arrow in front of your head, _arrowDistance away,
        //     stacked up or down by _arrowSpacing per level (one level up = first arrow above eye height)
        // Empty if CanTeleportNow is false
        // TODO(Moustafa) [X] Completed
        private void BuildTargets()
        {
            _targets.Clear();
            if (!CanTeleportNow() || MapBuilder.Instance == null)
            {
                return;
            }

            MapDefinition map = MapBuilder.Instance.Map;
            GridCoord from = PredictedPad.Value;
            Transform head = LocalRig.Instance.Head;
            Vector3 forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            forward.Normalize();

            foreach (GridCoord pad in PlayerMovement.Local.GetReachablePads(from))
            {
                MarkerTarget target = new MarkerTarget();
                target.pad = pad;
                if (pad.x == from.x && pad.z == from.z)
                {
                    // Own column: an arrow in front of you, one step up or down per level
                    int levels = pad.level - from.level;
                    target.isElevator = true;
                    target.position = head.position + forward * _arrowDistance + Vector3.up * (levels * _arrowSpacing);
                }
                else
                {
                    target.isElevator = false;
                    target.position = map.PadWorldPosition(pad);
                }
                _targets.Add(target);
            }
        }

        // The target with the smallest angle to where the left controller points, if under _maxSelectAngle
        // Then _markers.SetSelected(that target, or null)
        // TODO(Moustafa) [X] Completed
        private void UpdateSelection()
        {
            Transform controller = LocalRig.Instance.LeftController;
            if (controller == null)
            {
                return;
            }

            MarkerTarget? best = null;
            float bestAngle = _maxSelectAngle;
            foreach (MarkerTarget target in _targets)
            {
                float angle = Vector3.Angle(controller.forward, target.position - controller.position);
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    best = target;
                }
            }

            _selected = best;
            _markers.SetSelected(best);
        }

        // Move instantly and ask the host
        // LocalRig.TeleportTo(map.PadWorldPosition(target)), PredictedPad = target, RaisePlayerTeleported (sounds play now),
        // _pendingRequests + 1, _lastTeleportTime = now, PlayerMovement.Local.RequestTeleportRpc(target.ToVector3Int())
        // _pendingRequests goes back down when the host's answer arrives (CurrentPad changes or OnTeleportRejected)
        // TODO(Moustafa) [X] Completed
        private void TryTeleport(GridCoord target)
        {
            MapDefinition map = MapBuilder.Instance.Map;
            GridCoord from = PredictedPad.Value;

            LocalRig.Instance.TeleportTo(map.PadWorldPosition(target));
            PredictedPad = target;
            GameEvents.RaisePlayerTeleported(PlayerMovement.Local.OwnerClientId, from, target);
            _pendingRequests++;
            _lastTeleportTime = Time.timeAsDouble;
            PlayerMovement.Local.RequestTeleportRpc(target.ToVector3Int());
        }

        // The host refused: snap back to confirmedPad (LocalRig.TeleportTo), PredictedPad = confirmedPad, _pendingRequests - 1
        // RaiseTeleportDenied, Feedback plays the "denied" sound (no text, VR mid-fight is no place for reading)
        // TODO(Moustafa) [X] Completed
        public void OnTeleportRejected(GridCoord confirmedPad)
        {
            if (MapBuilder.Instance != null && LocalRig.Instance != null)
            {
                LocalRig.Instance.TeleportTo(MapBuilder.Instance.Map.PadWorldPosition(confirmedPad));
            }
            PredictedPad = confirmedPad;
            _pendingRequests = Mathf.Max(0, _pendingRequests - 1);
            GameEvents.RaiseTeleportDenied();
        }

        // The host confirmed one of your teleports (PlayerMovement sees your CurrentPad change on your headset)
        // _pendingRequests - 1, never below 0, so the charge lights stop counting that request
        // TODO(Moustafa) [X] Completed
        public void OnTeleportConfirmed()
        {
            _pendingRequests = Mathf.Max(0, _pendingRequests - 1);
        }

        // The host spawned you: LocalRig.TeleportTo(pad), LocalRig.FaceTowards(arena centre), PredictedPad = pad, _pendingRequests = 0
        // Arena centre = halfway between map.GridToWorld((0, level, 0)) and map.GridToWorld((width - 1, level, depth - 1)) at your level
        // TODO(Moustafa) [X] Completed
        public void PlaceAt(GridCoord pad)
        {
            _pendingRequests = 0;
            _selected = null;
            if (_markers != null)
            {
                _markers.Hide();
            }
            PredictedPad = pad;
            if (MapBuilder.Instance == null || LocalRig.Instance == null)
            {
                return;
            }

            MapDefinition map = MapBuilder.Instance.Map;
            LocalRig.Instance.TeleportTo(map.PadWorldPosition(pad));
            Vector3 corner = map.GridToWorld(new GridCoord(0, pad.level, 0));
            Vector3 farCorner = map.GridToWorld(new GridCoord(map.Layout.width - 1, pad.level, map.Layout.depth - 1));
            LocalRig.Instance.FaceTowards((corner + farCorner) * 0.5f);
        }

        // You died or the match ended: LocalRig.TeleportTo(lobby area point), PredictedPad = null, _markers.Hide(), _pendingRequests = 0
        // We need to make sure _pendingRequests is reset here, a request still waiting when you died never gets an answer
        // TODO(Moustafa) [X] Completed
        public void GoToLobbyArea()
        {
            _pendingRequests = 0;
            _selected = null;
            PredictedPad = null;
            if (_markers != null)
            {
                _markers.Hide();
            }
            if (LocalRig.Instance != null && _lobbyAreaPoint != null)
            {
                Vector3 spot = LobbySpot();
                LocalRig.Instance.TeleportTo(spot);
                LocalRig.Instance.FaceTowards(spot + _lobbyAreaPoint.forward * 2f);
                // Each headset has its own copy of the panels, so every player sees them in front of their own spot
                Vector3 front = spot + _lobbyAreaPoint.forward * LobbyPanelDistance + Vector3.up * LobbyPanelHeight;
                if (_lobbyPanels != null)
                {
                    _lobbyPanels.position = front - _lobbyAreaPoint.right * LobbyPanelSide;
                    _lobbyPanels.rotation = _lobbyAreaPoint.rotation * Quaternion.Euler(0f, -LobbyPanelTurn, 0f);
                }
                if (_roomCodeSign != null)
                {
                    _roomCodeSign.position = front + _lobbyAreaPoint.right * LobbyPanelSide;
                    _roomCodeSign.rotation = _lobbyAreaPoint.rotation * Quaternion.Euler(0f, LobbyPanelTurn, 0f);
                }
            }
        }

        // Your own spot in the lobby: in a row along the lobby point's right, LobbySpotSpacing apart, by colour slot (0 to 4)
        // Slot 2 stands on the lobby point itself, the others to its left and right
        // TODO(Moustafa) [X] Completed
        private Vector3 LobbySpot()
        {
            int slot = PlayerProfile.Local != null ? PlayerProfile.Local.ColorIndex.Value : 2;
            return _lobbyAreaPoint.position + _lobbyAreaPoint.right * ((slot - 2) * LobbySpotSpacing);
        }
    }
}
