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
        private bool _aimCancelled;  // the grip cancelled this aim, nothing happens until the stick is released

        // Instance = this, find GhostMarkers on this object
        // TODO(Moustafa)
        private void Awake()
        {
        }

        // Enable the aim and cancel actions
        // TODO(Moustafa)
        private void OnEnable()
        {
        }

        // Disable them
        // TODO(Moustafa)
        private void OnDisable()
        {
        }

        // Every frame, aiming = _aimAction.action.phase == InputActionPhase.Performed, compared with last frame:
        //   aiming started: BuildTargets, if any: _markers.Show(_targets)
        //   still aiming: UpdateSelection, cancel pressed this frame: clear _selected, _markers.Hide(), _aimCancelled = true until aiming ends
        //   aiming ended: if _selected has a value: TryTeleport, then _markers.Hide()
        // TODO(Moustafa)
        private void Update()
        {
        }

        // Can you teleport right now: on a pad, match Playing, Charges - _pendingRequests above 0, 0.3 s since the last one
        // TODO(Moustafa)
        private bool CanTeleportNow()
        {
            return false;
        }

        // Fill _targets from PlayerMovement.Local.GetReachablePads(PredictedPad)
        //   pad in another column: a disc at map.PadWorldPosition(pad)
        //   pad in your own column: an arrow in front of your head, _arrowDistance away,
        //     stacked up or down by _arrowSpacing per level (one level up = first arrow above eye height)
        // Empty if CanTeleportNow is false
        // TODO(Moustafa)
        private void BuildTargets()
        {
        }

        // The target with the smallest angle to where the left controller points, if under _maxSelectAngle
        // Then _markers.SetSelected(that target, or null)
        // TODO(Moustafa)
        private void UpdateSelection()
        {
        }

        // Move instantly and ask the host
        // LocalRig.TeleportTo(map.PadWorldPosition(target)), PredictedPad = target, RaisePlayerTeleported (sounds play now),
        // _pendingRequests + 1, _lastTeleportTime = now, PlayerMovement.Local.RequestTeleportRpc(target.ToVector3Int())
        // _pendingRequests goes back down when the host's answer arrives (CurrentPad changes or OnTeleportRejected)
        // TODO(Moustafa)
        private void TryTeleport(GridCoord target)
        {
        }

        // The host refused: snap back to confirmedPad (LocalRig.TeleportTo), PredictedPad = confirmedPad, _pendingRequests - 1
        // RaiseTeleportDenied, Feedback plays the "denied" sound (no text, VR mid-fight is no place for reading)
        // TODO(Moustafa)
        public void OnTeleportRejected(GridCoord confirmedPad)
        {
        }

        // The host confirmed one of your teleports (PlayerMovement sees your CurrentPad change on your headset)
        // _pendingRequests - 1, never below 0, so the charge lights stop counting that request
        // TODO(Moustafa)
        public void OnTeleportConfirmed()
        {
        }

        // The host spawned you: LocalRig.TeleportTo(pad), LocalRig.FaceTowards(arena centre), PredictedPad = pad, _pendingRequests = 0
        // Arena centre = halfway between map.GridToWorld((0, level, 0)) and map.GridToWorld((width - 1, level, depth - 1)) at your level
        // TODO(Moustafa)
        public void PlaceAt(GridCoord pad)
        {
        }

        // You died or the match ended: LocalRig.TeleportTo(lobby area point), PredictedPad = null, _markers.Hide(), _pendingRequests = 0
        // We need to make sure _pendingRequests is reset here, a request still waiting when you died never gets an answer
        // TODO(Moustafa)
        public void GoToLobbyArea()
        {
        }
    }
}
