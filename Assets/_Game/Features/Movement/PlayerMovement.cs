// PlayerMovement.cs
// Owner: Joe
// Layer: Feature, Movement (NetworkBehaviour on the player prefab, one per player)
//
// What this is
// The host side of teleporting: which pad each player is on, their charges, and whether a teleport is allowed
// TeleportController (on your headset) asks, this file (on the host) decides
//
// The flow: request, check, commit, react
//   1. Your headset teleports you instantly (no waiting) and sends RequestTeleportRpc
//   2. The host checks: you are alive, the match is Playing, the pad is in your moveset from your confirmed pad,
//      nobody is on it, you have a charge, and the gap has passed
//   3. Allowed: the host spends the charge and writes CurrentPad, every headset sees you on the new pad
//      Refused: the host tells only you, and your headset snaps you back (no charge spent)
// First request wins: the host handles requests one at a time, so if two players want the same pad,
// the first one gets it and the second one is refused because the pad is now taken
//
// Who sees what
//   CurrentPad: every headset (everyone needs to know where everyone stands)
//   Charges and RechargeStartTime: only you (your board) and the host (the check), nobody else uses them
//   the brief asks to sync ammunition/charge "as relevant to each player", your charges are only relevant to you
//
// Network timing
// Your headset enforces the real 0.3 s teleport gap, the host's charges use 0.2 s (Wi-Fi can squeeze two legal requests together)
//
// Character
// The character is read from PlayerProfile when the player spawns (moveset + charges)
// Changing character in the lobby area takes effect at the next spawn, nothing changes while you are on a pad
//
// Time
// Everyone uses the network server time, so the HUD charge bar fills in step with the host
//
// Template check (done)
//   the template's networked player prefab (XRI_Network_Player_Avatar) is where this component goes
//
// Editor setup
//   1. Add this component to the template's networked player prefab, next to PlayerProfile
//   2. Drag DefaultMatch into Settings

using System.Collections.Generic;
using Game.Core;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class PlayerMovement : NetworkBehaviour
    {
        // Every player, on every headset
        public static readonly List<PlayerMovement> All = new List<PlayerMovement>();

        // The player on this headset
        public static PlayerMovement Local { get; private set; }

        [SerializeField] private MatchSettings _settings;

        // How much shorter the host's teleport gap is than the real one, for Wi-Fi jitter
        private const float HostGapSlack = 0.1f;

        // Synced values, only the host writes them
        // CurrentPad: everyone reads it, Charges and RechargeStartTime: only the owner reads them
        private readonly NetworkVariable<Vector3Int> _currentPad = new NetworkVariable<Vector3Int>(UnityConversions.NoPad);
        private readonly NetworkVariable<int> _charges = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<double> _rechargeStartTime = new NetworkVariable<double>(
            0, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

        // Read access
        public NetworkVariable<Vector3Int> CurrentPad { get { return _currentPad; } }   // NoPad when dead or in the lobby area
        public NetworkVariable<int> Charges { get { return _charges; } }                // owner and host only
        public NetworkVariable<double> RechargeStartTime { get { return _rechargeStartTime; } }  // owner and host only, for the HUD charge bar

        // Host and owner: the moveset, made from PlayerProfile.Character at each spawn
        // the owner's headset needs it for the markers, the host for the check, same moveset on both
        private IMoveset _moveset;

        // Host only
        private TeleportCharging _charging;

        // On a pad (alive and playing), not dead or in the lobby area
        public bool IsOnBoard
        {
            get { return _currentPad.Value != UnityConversions.NoPad; }
        }

        public GridCoord CurrentGrid
        {
            get { return _currentPad.Value.ToGrid(); }
        }

        // Add to All, set Local if IsOwner
        // Every headset: subscribe to CurrentPad changes
        //   NoPad -> pad: RaisePlayerSpawned
        //   pad -> pad: RaisePlayerTeleported, except on the owner (its headset already raised it when it predicted the teleport)
        //   pad -> NoPad: nothing (PlayerKilled already covers death)
        // TODO(Joe)
        public override void OnNetworkSpawn()
        {
        }

        // Remove from All, clear Local, unsubscribe
        // TODO(Joe)
        public override void OnNetworkDespawn()
        {
        }

        // True if another player is standing on coord (their CurrentPad, players in the lobby area don't count)
        // Works on every headset, it only reads synced values, movesets use it as isOccupied
        // TODO(Joe)
        public static bool IsOccupiedByOther(GridCoord coord, PlayerMovement except)
        {
            return false;
        }

        // Pads this player can reach from a given pad, using their character's moveset
        // moveset.GetReachable(from, map layout, coord => IsOccupiedByOther(coord, this))
        // TeleportController calls it with the predicted pad (markers), the host with the confirmed pad (the check)
        // Same function both times, so the markers and the host never disagree about the rules
        // TODO(Joe)
        public List<GridCoord> GetReachablePads(GridCoord from)
        {
            return new List<GridCoord>();
        }

        // Your headset asks to teleport
        // Host: not on board (you died or the match ended before the request arrived): ignore it, no reply,
        //   your headset was already sent to the lobby area
        // Otherwise, if ServerCanTeleportTo(target): spend the charge, sync Charges and RechargeStartTime,
        //   CurrentPad = target, PlayerCombat.ServerOnActed (ends spawn protection)
        //   otherwise: TeleportRejectedRpc(CurrentPad) to the owner only
        // InvokePermission Owner: only this player's own headset can ask to move this player
        // TODO(Joe)
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void RequestTeleportRpc(Vector3Int target)
        {
        }

        // Host check, in this order: on board, match is Playing, target in GetReachablePads(CurrentGrid),
        // _charging.CanSpend(server time)
        // Refuses with no message (VR mid-fight is no place for text), your headset snaps back and plays a "denied" sound
        // Logs the failed check in the editor
        // TODO(Joe)
        private bool ServerCanTeleportTo(GridCoord target)
        {
            return false;
        }

        // Owner only: the host refused, TeleportController.OnTeleportRejected(confirmedPad)
        // TODO(Joe)
        [Rpc(SendTo.Owner)]
        private void TeleportRejectedRpc(Vector3Int confirmedPad)
        {
        }

        // Owner only: the host spawned you on a pad
        // _moveset = PlayerProfile.Character.CreateMoveset() (for the markers), then TeleportController.PlaceAt(pad)
        // TODO(Joe)
        [Rpc(SendTo.Owner)]
        private void PlaceAtRpc(Vector3Int pad)
        {
        }

        // Owner only: you died or the match is over, TeleportController.GoToLobbyArea()
        // TODO(Joe)
        [Rpc(SendTo.Owner)]
        private void SendToLobbyAreaRpc()
        {
        }

        // Owner only: your kill gave you +1 charge, RaiseChargeGained (Feedback plays the chime)
        // Needed because the charge number alone can't tell a kill bonus from a normal recharge
        // TODO(Joe)
        [Rpc(SendTo.Owner)]
        private void ChargeGainedRpc()
        {
        }

        // Host API, called by MatchManager and PlayerCombat

        // Spawn on a pad: read PlayerProfile.Character, make its moveset and a new TeleportCharging
        // (gap = MinTeleportGap - HostGapSlack), Reset to full, sync Charges and RechargeStartTime, CurrentPad = pad, PlaceAtRpc
        // TODO(Joe)
        public void ServerPlaceAt(GridCoord pad)
        {
        }

        // Died or the match ended: CurrentPad = NoPad, SendToLobbyAreaRpc
        // TODO(Joe)
        public void ServerSendToLobbyArea()
        {
        }

        // Kill bonus: _charging.AddCharge(), if it added one: sync Charges and ChargeGainedRpc
        // TODO(Joe)
        public void ServerAddCharge()
        {
        }

        // Host only, every frame: skip if _charging doesn't exist yet (before the first spawn),
        // otherwise _charging.Tick(server time), if it changed: sync Charges and RechargeStartTime
        // TODO(Joe)
        private void Update()
        {
        }
    }
}
