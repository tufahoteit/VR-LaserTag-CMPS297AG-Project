// LobbyExtras.cs
// Owner: Karim
// Layer: Feature, UI (local only, a small canvas in the lobby area, view only, sends requests)
//
// What this is
// The buttons our game adds in the lobby area, next to the template's own menus
// The template already has create, join by code, the keyboard, the room code display and leave, we don't rebuild those
//   Start / Restart: host only, while the match is in Lobby or Ended
//   Height: stand up straight and press it, your eyes go to 1.65 m (LocalRig.CalibrateHeight)
//   one button per character: pick what you play next spawn
//   status line: the room code, the phase, "3/5 players", and why you were last disconnected
//
// Who does what
// Start goes straight to MatchManager.ServerStartMatch (only the host sees the button, so no RPC is needed)
// A character button asks the host (PlayerProfile.RequestCharacterRpc), the host decides
//
// Template check (done)
//   the room code: XRINetworkGameManager.ConnectedRoomCode
//   world-space buttons need a Tracked Device Graphic Raycaster on the canvas, so the controller rays can press them
//
// Editor setup
//   1. In the lobby area, create a World Space Canvas named LobbyExtras, scale 0.001, add a Tracked Device Graphic Raycaster
//   2. Children: StartButton (with a TextMeshPro label), HeightButton, one button per character (same order as Characters),
//      StatusText (TextMeshPro)
//   3. Add this component to the canvas, drag everything into its slots and DefaultMatch into Settings
//   The lobby platform has a see-through floor, place the canvas so it doesn't hide the arena

using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using XRMultiplayer;

namespace Game
{
    public class LobbyExtras : MonoBehaviour
    {
        [SerializeField] private MatchSettings _settings;
        [SerializeField] private Button _startButton;
        [SerializeField] private TMP_Text _startButtonLabel;
        [SerializeField] private Button _heightButton;

        [Tooltip("One per character, same order as Characters in DefaultMatch")]
        [SerializeField] private Button[] _characterButtons;

        [SerializeField] private TMP_Text _statusText;

        // Connect the buttons: Start -> OnStartPressed, Height -> OnHeightPressed, character i -> OnCharacterPressed(i)
        // We need to make sure each character button passes its own index (copy i into a local before the lambda)
        // TODO(Karim)
        private void Start()
        {
        }

        // Every frame:
        //   Start visible only on the host while the phase is Lobby or Ended, label "Start" (Lobby) or "Restart" (Ended)
        //   character buttons: interactable only while you are in the lobby area (dead, or Lobby or Countdown),
        //     the one at PlayerProfile.Local.CharacterIndex highlighted
        //   status: "Room ABC123   Lobby   3/5 players" (XRINetworkGameManager.ConnectedRoomCode),
        //     plus SessionGuard.LastDisconnectReason on a second line if it isn't empty
        //   not connected (PlayerProfile.Local is null): only the status line, "Not connected"
        // TODO(Karim)
        private void Update()
        {
        }

        // Host only: MatchManager.Instance.ServerStartMatch()
        // TODO(Karim)
        private void OnStartPressed()
        {
        }

        // LocalRig.Instance.CalibrateHeight()
        // TODO(Karim)
        private void OnHeightPressed()
        {
        }

        // PlayerProfile.Local.RequestCharacterRpc(index)
        // TODO(Karim)
        private void OnCharacterPressed(int index)
        {
        }
    }
}
