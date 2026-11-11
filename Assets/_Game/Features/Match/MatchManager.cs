// MatchManager.cs
// Owner: Karim
// Layer: Feature, Match (NetworkBehaviour on one scene object, there is one MatchManager per game)
//
// What this is
// The host runs the match: the phases, the timer, spawning and respawning, and who won
// Other headsets only read Phase, PhaseEndTime and the result to show them (your board, the lobby area status line)
// The rules live in MatchRules (CheckWin, SpawnSelector), this file runs them and syncs the answers
//
// The phases
//   Lobby: everyone in the lobby area picking characters, the host presses Start (alone is allowed, for testing)
//   Countdown: 5 s, still in the lobby area, kills reset to 0
//   Playing: everyone spawned on spawn pads, the 9 minute timer runs
//     a death: lobby area, respawn after 4 s on the safest free spawn pad
//     the match ends at 15 kills, or when time runs out with one clear leader
//     time runs out on a tie: sudden death, Playing keeps going and the next kill wins
//   Ended: everyone back in the lobby area, the result shows for 5 s, then Lobby again (kills stay visible until the next Start)
//
// Players joining and leaving
//   joining mid-match: spawns straight in with 0 kills (Update notices a player who is not alive and has no respawn time)
//   leaving: SessionGuard tells the host, the player is removed from the match and every scoreboard (PlayerLeftRpc)
//     their pad frees up by itself (their objects despawn), rejoining with the code is a fresh join with 0 kills
//
// Who sees what
//   Phase, PhaseEndTime, Winner, EndKind: every headset
//   respawn times and the last kill time: host only
//
// Time
// The host uses the network server time, PhaseEndTime is in server time so every headset shows the same timer
//
// Template check
//   none, the NetworkObject on the scene object is enough (NetworkManager spawns in-scene objects for everyone)
//
// Editor setup
//   1. Create an empty object in Main named MatchManager, add a NetworkObject and this component
//   2. Drag DefaultMatch into Settings

using System.Collections.Generic;
using Game.Core;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class MatchManager : NetworkBehaviour
    {
        public static MatchManager Instance { get; private set; }

        // "Nobody won yet"
        public const ulong NoWinner = ulong.MaxValue;

        [SerializeField] private MatchSettings _settings;

        // Synced values, only the host writes them, everyone reads them
        private readonly NetworkVariable<MatchPhase> _phase = new NetworkVariable<MatchPhase>(MatchPhase.Lobby);
        private readonly NetworkVariable<double> _phaseEndTime = new NetworkVariable<double>(0);   // countdown end, time limit, or end screen end
        private readonly NetworkVariable<ulong> _winner = new NetworkVariable<ulong>(NoWinner);
        private readonly NetworkVariable<MatchEndKind> _endKind = new NetworkVariable<MatchEndKind>(MatchEndKind.NotEnded);

        // Read access
        public NetworkVariable<MatchPhase> Phase { get { return _phase; } }
        public NetworkVariable<double> PhaseEndTime { get { return _phaseEndTime; } }
        public NetworkVariable<ulong> Winner { get { return _winner; } }
        public NetworkVariable<MatchEndKind> EndKind { get { return _endKind; } }
        public MatchSettings Settings { get { return _settings; } }

        // Host only
        private readonly Dictionary<ulong, double> _respawnAt = new Dictionary<ulong, double>();
        private double _lastKillTime = double.NegativeInfinity;  // "no kill yet", so the trade window never blocks the first check

        // Every headset, for the HUD

        // Seconds left on the current phase, never below 0, and 0 in Lobby (PhaseEndTime is left over from the last match there)
        // TODO(Karim)
        public float SecondsLeft()
        {
            return 0f;
        }

        // True while Playing after the time limit, the match is waiting for the next kill
        // TODO(Karim)
        public bool IsSuddenDeath()
        {
            return false;
        }

        // Instance = this
        // Every headset: subscribe to Phase changes -> RaiseMatchPhaseChanged
        // TODO(Karim)
        public override void OnNetworkSpawn()
        {
        }

        // Unsubscribe, clear Instance
        // TODO(Karim)
        public override void OnNetworkDespawn()
        {
        }

        // Host API

        // The host's Start button in the lobby area (LobbyExtras shows it only on the host's headset)
        // Allowed in Lobby or Ended, otherwise ignored: ServerStartCountdown
        // TODO(Karim)
        public void ServerStartMatch()
        {
        }

        // Called by PlayerCombat when a player died, after it counted the kill and the kill bonus
        //   _lastKillTime = now
        //   victim: PlayerMovement.ServerSendToLobbyArea, _respawnAt[victim] = now + RespawnDelaySeconds
        // The win check runs in Update, it has to wait out the trade window anyway
        // TODO(Karim)
        public void ServerOnPlayerKilled(ulong victimClientId, ulong killerClientId)
        {
        }

        // Called by SessionGuard when a client left
        //   remove them from _respawnAt, PlayerLeftRpc(clientId)
        // Their objects despawn by themselves, so their pad and their kills disappear from everything
        // TODO(Karim)
        public void ServerHandleDisconnect(ulong clientId)
        {
        }

        // Host flow

        // Every player: PlayerProfile.ResetKills, in the lobby area
        // _respawnAt cleared, _lastKillTime = minus infinity, Winner = NoWinner, EndKind = NotEnded
        // Phase = Countdown, PhaseEndTime = now + CountdownSeconds
        // TODO(Karim)
        private void ServerStartCountdown()
        {
        }

        // Phase = Playing, PhaseEndTime = now + TimeLimitSeconds
        // Spawn every player one after another, players already placed count as enemies so everyone spreads out
        // TODO(Karim)
        private void ServerBeginPlaying()
        {
        }

        // Spawn one player
        //   enemies: CurrentGrid of every other player who is on board
        //   pad = SpawnSelector.Pick(map.Layout.SpawnPads(), enemies, coord => PlayerMovement.IsOccupiedByOther(coord, player))
        //   null: log an error and try again next frame (keep their _respawnAt), this only happens if something is broken
        //   otherwise: PlayerCombat.ServerResetForSpawn, then PlayerMovement.ServerPlaceAt(pad), remove them from _respawnAt
        //   (alive first, then the pad, so a player is already alive the moment they appear)
        // The map comes from MapBuilder.Instance.Map
        // TODO(Karim)
        private void ServerSpawnPlayer(PlayerMovement player)
        {
        }

        // Winner and EndKind from the result, Phase = Ended, PhaseEndTime = now + EndScreenSeconds
        // Every player: PlayerCombat.ServerClearForLobby, PlayerMovement.ServerSendToLobbyArea, _respawnAt cleared
        // TODO(Karim)
        private void ServerEndMatch(MatchResult result)
        {
        }

        // After the end screen: Phase = Lobby, the host can press Start again
        // TODO(Karim)
        private void ServerReturnToLobby()
        {
        }

        // Host only, every frame
        //   Countdown: PhaseEndTime passed -> ServerBeginPlaying
        //   Playing:
        //     a player who is not alive and not in _respawnAt joined mid-match: _respawnAt[them] = now
        //     respawn every player whose _respawnAt has passed (ServerSpawnPlayer)
        //     MatchRules.CheckWin(CollectKills(), KillsToWin, now, PhaseEndTime, _lastKillTime, TradeWindowSeconds)
        //     ended -> ServerEndMatch
        //   Ended: PhaseEndTime passed -> ServerReturnToLobby
        // TODO(Karim)
        private void Update()
        {
        }

        // clientId -> kills for every player in the match, from PlayerProfile.All
        // TODO(Karim)
        private Dictionary<ulong, int> CollectKills()
        {
            return new Dictionary<ulong, int>();
        }

        // Host to headsets

        // Everyone: RaisePlayerLeft(clientId), Minimap and the scoreboard remove the player
        // TODO(Karim)
        [Rpc(SendTo.Everyone)]
        private void PlayerLeftRpc(ulong clientId)
        {
        }
    }
}
