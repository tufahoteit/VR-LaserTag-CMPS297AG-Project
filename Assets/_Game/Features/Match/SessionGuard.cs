// SessionGuard.cs
// Owner: Karim
// Layer: Feature, Match (on the template's Network Manager object, runs on the host and on clients)
//
// What this is
// Our rules on top of the template's connection code
// The template already does sign-in, Relay, the join code and the join keyboard, this file adds:
//   at most 5 players: the 6th is refused with a reason
//   disconnects: the host tells MatchManager, a client remembers why it was disconnected
//
// Why this file is required
// The Network Manager has Connection Approval on, so the host must answer every join request
// Without an approval callback nobody can join, the join just hangs
//
// Template check (done)
//   the template uses Multiplayer Services sessions, changed to Relay (client-host) in SessionManager
//   the Network Manager prefab: Client Server, Connection Approval on
//   the template's max players is 5 (XRINetworkGameManager.maxPlayers), this check is the second line of defence
//
// Editor setup
//   1. Open the Network Manager VR Multiplayer prefab, add this component to its root object
//   2. Drag DefaultMatch into Settings

using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class SessionGuard : MonoBehaviour
    {
        [SerializeField] private MatchSettings _settings;

        private NetworkManager _manager;

        // Why this headset was last disconnected, for the lobby status line ("Match is full", "Host left")
        public static string LastDisconnectReason { get; private set; } = "";

        // NetworkManager.Singleton (on this object): ConnectionApprovalCallback = Approve,
        // subscribe to OnClientDisconnectCallback
        // We need to make sure this runs before the template starts a session, Awake does
        // TODO(Karim) [X] Completed
        private void Awake()
        {
            NetworkManager manager = GetComponent<NetworkManager>();
            if (manager == null)
            {
                manager = NetworkManager.Singleton;
            }
            if (manager == null)
            {
                Debug.LogError("[SessionGuard] No NetworkManager found, put this on the Network Manager object", this);
                return;
            }
            _manager = manager;
            _manager.ConnectionApprovalCallback = Approve;
            _manager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // Clear ConnectionApprovalCallback, unsubscribe
        // TODO(Karim) [X] Completed
        private void OnDestroy()
        {
            if (_manager == null)
            {
                return;
            }
            _manager.ConnectionApprovalCallback = null;
            _manager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        // Host only, once per join request
        //   fewer than _settings.MaxPlayers connected: Approved = true, CreatePlayerObject = true
        //   otherwise: Approved = false, Reason = "Match is full"
        // TODO(Karim) [X] Completed
        private void Approve(NetworkManager.ConnectionApprovalRequest request,
                             NetworkManager.ConnectionApprovalResponse response)
        {
            int connected = _manager.ConnectedClientsIds.Count;
            if (connected < _settings.MaxPlayers)
            {
                response.Approved = true;
                response.CreatePlayerObject = true;
            }
            else
            {
                response.Approved = false;
                response.Reason = "Match is full";
            }
            response.Pending = false;
        }

        // Listener for NetworkManager.OnClientDisconnectCallback
        //   on the host, someone else left: MatchManager.Instance.ServerHandleDisconnect(clientId)
        //   on a client, we were disconnected (clientId is our own): LastDisconnectReason = NetworkManager.DisconnectReason,
        //   or "Host left" if it is empty
        // The template takes the player back to its own menu, nothing else to do here
        // TODO(Karim) [X] Completed
        private void OnClientDisconnected(ulong clientId)
        {
            if (_manager.IsServer)
            {
                if (clientId != _manager.LocalClientId && MatchManager.Instance != null)
                {
                    MatchManager.Instance.ServerHandleDisconnect(clientId);
                }
                return;
            }

            string reason = _manager.DisconnectReason;
            LastDisconnectReason = string.IsNullOrEmpty(reason) ? "Host left" : reason;
        }
    }
}
