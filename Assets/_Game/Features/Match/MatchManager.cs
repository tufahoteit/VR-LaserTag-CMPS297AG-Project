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
        // TODO(Karim) [X] Completed
        public float SecondsLeft()
        {
            if (!IsSpawned || _phase.Value == MatchPhase.Lobby)
            {
                return 0f;
            }
            return Mathf.Max(0f, (float)(_phaseEndTime.Value - Now));
        }

        // True while Playing after the time limit, the match is waiting for the next kill
        // TODO(Karim) [X] Completed
        public bool IsSuddenDeath()
        {
            return IsSpawned && _phase.Value == MatchPhase.Playing && Now >= _phaseEndTime.Value;
        }

        // Instance = this
        // Every headset: subscribe to Phase changes -> RaiseMatchPhaseChanged
        // TODO(Karim) [X] Completed
        public override void OnNetworkSpawn()
        {
            Instance = this;
            _phase.OnValueChanged += OnPhaseChanged;
        }

        // Unsubscribe, clear Instance
        // TODO(Karim) [X] Completed
        public override void OnNetworkDespawn()
        {
            _phase.OnValueChanged -= OnPhaseChanged;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Host API

        // The host's Start button in the lobby area (LobbyExtras shows it only on the host's headset)
        // Allowed in Lobby or Ended, otherwise ignored: ServerStartCountdown
        // TODO(Karim) [X] Completed
        public void ServerStartMatch()
        {
            if (!IsServer)
            {
                return;
            }
            if (_phase.Value == MatchPhase.Lobby || _phase.Value == MatchPhase.Ended)
            {
                ServerStartCountdown();
            }
        }

        // Called by PlayerCombat when a player died, after it counted the kill and the kill bonus
        //   _lastKillTime = now
        //   victim: PlayerMovement.ServerSendToLobbyArea, _respawnAt[victim] = now + RespawnDelaySeconds
        // The win check runs in Update, it has to wait out the trade window anyway
        // TODO(Karim) [X] Completed
        public void ServerOnPlayerKilled(ulong victimClientId, ulong killerClientId)
        {
            double now = Now;
            _lastKillTime = now;
            PlayerMovement victim = MovementOf(victimClientId);
            if (victim != null)
            {
                victim.ServerSendToLobbyArea();
            }
            _respawnAt[victimClientId] = now + _settings.RespawnDelaySeconds;
        }

        // Called by SessionGuard when a client left
        //   remove them from _respawnAt, PlayerLeftRpc(clientId)
        // Their objects despawn by themselves, so their pad and their kills disappear from everything
        // TODO(Karim) [X] Completed
        public void ServerHandleDisconnect(ulong clientId)
        {
            _respawnAt.Remove(clientId);
            PlayerLeftRpc(clientId);
        }

        // Host flow

        // Every player: PlayerProfile.ResetKills, in the lobby area
        // _respawnAt cleared, _lastKillTime = minus infinity, Winner = NoWinner, EndKind = NotEnded
        // Phase = Countdown, PhaseEndTime = now + CountdownSeconds
        // TODO(Karim) [X] Completed
        private void ServerStartCountdown()
        {
            foreach (PlayerProfile profile in PlayerProfile.All)
            {
                profile.ResetKills();
            }
            foreach (PlayerCombat combat in PlayerCombat.All)
            {
                combat.ServerClearForLobby();
            }
            foreach (PlayerMovement player in PlayerMovement.All)
            {
                player.ServerSendToLobbyArea();
            }

            _respawnAt.Clear();
            _lastKillTime = double.NegativeInfinity;
            _winner.Value = NoWinner;
            _endKind.Value = MatchEndKind.NotEnded;
            _phase.Value = MatchPhase.Countdown;
            _phaseEndTime.Value = Now + _settings.CountdownSeconds;
        }

        // Phase = Playing, PhaseEndTime = now + TimeLimitSeconds
        // Spawn every player one after another, players already placed count as enemies so everyone spreads out
        // TODO(Karim) [X] Completed
        private void ServerBeginPlaying()
        {
            _phase.Value = MatchPhase.Playing;
            _phaseEndTime.Value = Now + _settings.TimeLimitSeconds;
            List<PlayerMovement> players = new List<PlayerMovement>(PlayerMovement.All);
            foreach (PlayerMovement player in players)
            {
                ServerSpawnPlayer(player);
            }
        }

        // Spawn one player
        //   enemies: CurrentGrid of every other player who is on board
        //   pad = SpawnSelector.Pick(map.Layout.SpawnPads(), enemies, coord => PlayerMovement.IsOccupiedByOther(coord, player))
        //   null: log an error and try again next frame (keep their _respawnAt), this only happens if something is broken
        //   otherwise: PlayerCombat.ServerResetForSpawn, then PlayerMovement.ServerPlaceAt(pad), remove them from _respawnAt
        //   (alive first, then the pad, so a player is already alive the moment they appear)
        // The map comes from MapBuilder.Instance.Map
        // TODO(Karim) [X] Completed
        private void ServerSpawnPlayer(PlayerMovement player)
        {
            if (MapBuilder.Instance == null || MapBuilder.Instance.Map == null)
            {
                Debug.LogError("[MatchManager] No built map to spawn on", this);
                return;
            }

            List<GridCoord> enemies = new List<GridCoord>();
            foreach (PlayerMovement other in PlayerMovement.All)
            {
                if (other != player && other.IsOnBoard)
                {
                    enemies.Add(other.CurrentGrid);
                }
            }

            GridCoord? pad = SpawnSelector.Pick(MapBuilder.Instance.Map.Layout.SpawnPads(), enemies,
                                                coord => PlayerMovement.IsOccupiedByOther(coord, player));
            if (pad == null)
            {
                Debug.LogError($"[MatchManager] No free spawn pad for client {player.OwnerClientId}, retrying next frame", this);
                _respawnAt[player.OwnerClientId] = Now;
                return;
            }

            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.ServerResetForSpawn();
            }
            player.ServerPlaceAt(pad.Value);
            _respawnAt.Remove(player.OwnerClientId);
        }

        // Winner and EndKind from the result, Phase = Ended, PhaseEndTime = now + EndScreenSeconds
        // Every player: PlayerCombat.ServerClearForLobby, PlayerMovement.ServerSendToLobbyArea, _respawnAt cleared
        // TODO(Karim) [X] Completed
        private void ServerEndMatch(MatchResult result)
        {
            _winner.Value = result.winnerClientId;
            _endKind.Value = result.endKind;
            _phase.Value = MatchPhase.Ended;
            _phaseEndTime.Value = Now + _settings.EndScreenSeconds;

            foreach (PlayerCombat combat in PlayerCombat.All)
            {
                combat.ServerClearForLobby();
            }
            foreach (PlayerMovement player in PlayerMovement.All)
            {
                player.ServerSendToLobbyArea();
            }
            _respawnAt.Clear();
        }

        // After the end screen: Phase = Lobby, the host can press Start again
        // TODO(Karim) [X] Completed
        private void ServerReturnToLobby()
        {
            _phase.Value = MatchPhase.Lobby;
        }

        // Host only, every frame
        //   Countdown: PhaseEndTime passed -> ServerBeginPlaying
        //   Playing:
        //     a player who is not alive and not in _respawnAt joined mid-match: _respawnAt[them] = now
        //     respawn every player whose _respawnAt has passed (ServerSpawnPlayer)
        //     MatchRules.CheckWin(CollectKills(), KillsToWin, now, PhaseEndTime, _lastKillTime, TradeWindowSeconds)
        //     ended -> ServerEndMatch
        //   Ended: PhaseEndTime passed -> ServerReturnToLobby
        // TODO(Karim) [X] Completed
        private void Update()
        {
            if (!IsServer || !IsSpawned)
            {
                return;
            }

            double now = Now;
            switch (_phase.Value)
            {
                case MatchPhase.Countdown:
                    if (now >= _phaseEndTime.Value)
                    {
                        ServerBeginPlaying();
                    }
                    break;

                case MatchPhase.Playing:
                    foreach (PlayerMovement player in PlayerMovement.All)
                    {
                        PlayerCombat combat = player.GetComponent<PlayerCombat>();
                        bool alive = combat != null && combat.IsAlive.Value;
                        if (!alive && !_respawnAt.ContainsKey(player.OwnerClientId))
                        {
                            // Joined mid-match: spawn right away
                            _respawnAt[player.OwnerClientId] = now;
                        }
                    }

                    List<PlayerMovement> due = new List<PlayerMovement>();
                    foreach (PlayerMovement player in PlayerMovement.All)
                    {
                        double respawnTime;
                        if (_respawnAt.TryGetValue(player.OwnerClientId, out respawnTime) && now >= respawnTime)
                        {
                            due.Add(player);
                        }
                    }
                    foreach (PlayerMovement player in due)
                    {
                        ServerSpawnPlayer(player);
                    }

                    MatchResult result = MatchRules.CheckWin(CollectKills(), _settings.KillsToWin, now, _phaseEndTime.Value,
                                                             _lastKillTime, _settings.TradeWindowSeconds);
                    if (result.endKind != MatchEndKind.NotEnded)
                    {
                        ServerEndMatch(result);
                    }
                    break;

                case MatchPhase.Ended:
                    if (now >= _phaseEndTime.Value)
                    {
                        ServerReturnToLobby();
                    }
                    break;
            }
        }

        // clientId -> kills for every player in the match, from PlayerProfile.All
        // TODO(Karim) [X] Completed
        private Dictionary<ulong, int> CollectKills()
        {
            Dictionary<ulong, int> kills = new Dictionary<ulong, int>();
            foreach (PlayerProfile profile in PlayerProfile.All)
            {
                kills[profile.OwnerClientId] = profile.Kills.Value;
            }
            return kills;
        }

        // Helpers

        private double Now
        {
            get { return NetworkManager.ServerTime.Time; }
        }

        private static PlayerMovement MovementOf(ulong clientId)
        {
            foreach (PlayerMovement player in PlayerMovement.All)
            {
                if (player.OwnerClientId == clientId)
                {
                    return player;
                }
            }
            return null;
        }

        // Listener for Phase changes (every headset)
        private void OnPhaseChanged(MatchPhase previous, MatchPhase current)
        {
            GameEvents.RaiseMatchPhaseChanged(current);
        }

        // Host to headsets

        // Everyone: RaisePlayerLeft(clientId), Minimap and the scoreboard remove the player
        // TODO(Karim) [X] Completed
        [Rpc(SendTo.Everyone)]
        private void PlayerLeftRpc(ulong clientId)
        {
            GameEvents.RaisePlayerLeft(clientId);
        }
    }
}
