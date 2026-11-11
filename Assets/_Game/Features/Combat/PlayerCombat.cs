// PlayerCombat.cs
// Owner: Moustafa
// Layer: Feature, Combat (NetworkBehaviour on the player prefab, one per player)
//
// What this is
// The host side of combat: HP, alive or dead, spawn protection, damage, deaths and kills
// LaserGun (on the shooter's headset) reports what it hit, this file (on the host) decides what happens
// The maths lives in CombatRules, this file stores the state and does the networking
//
// One shot, on the host
//   1. ReportShotRpc arrives from the shooter
//   2. CombatRules.ShotCounts: the shooter is alive, or died at most 0.15 s ago (a trade)
//   3. Fire rate: refuse a shot closer than half of MinShotGap to the last one (Wi-Fi can squeeze two legal shots together)
//   4. The shot counts: ServerOnActed (ends the shooter's spawn protection), ShowShotRpc to everyone else (beam and sound)
//   5. If it hit a player: safety checks (IsHitInRange, IsOriginNearShooter), then victim.ServerApplyDamage
//   Every refused shot logs a warning, a legal shot is never refused, so a warning means a bug in our code
//
// Damage, on the host
//   ignored if the victim is dead (nobody gets a second kill) or spawn protected
//   HP goes down, regen restarts, HitRpc tells every headset (victim, attacker, new HP, headshot)
//   HP at 0: ServerDie, the attacker of this last hit gets the kill
//
// Trades
// A shot from a player who died at most 0.15 s ago still counts
// So if two players kill each other at the same moment, both die and both get a kill
//
// Who sees what
//   Hp and IsAlive: every headset (health bars, your board, hitboxes on or off)
//   Each counted hit: one small HitRpc to everyone, every headset raises PlayerDamaged from it
//     the attacker plays the hit marker, the victim gets the red screen edges, the synced HP alone can't say who hit
//   Everything else (times, spawn protection, last shot) only lives on the host
//
// Time
// The host uses the network server time for every check
//
// Template check (done)
//   the template's networked player prefab (XRI_Network_Player_Avatar) is where this component goes
//
// Editor setup
//   1. Add this component to the template's networked player prefab, next to PlayerMovement
//   2. Drag DefaultMatch into Settings
//   3. The hitboxes are found automatically (every Hitbox under the prefab)

using System.Collections.Generic;
using Game.Core;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class PlayerCombat : NetworkBehaviour
    {
        // Every player, on every headset
        public static readonly List<PlayerCombat> All = new List<PlayerCombat>();

        // The player on this headset
        public static PlayerCombat Local { get; private set; }

        // "No player was hit" in a shot report
        public const ulong NoVictim = ulong.MaxValue;

        [SerializeField] private MatchSettings _settings;

        // Synced values, only the host writes them, everyone reads them
        private readonly NetworkVariable<float> _hp = new NetworkVariable<float>(0f);
        private readonly NetworkVariable<bool> _isAlive = new NetworkVariable<bool>(false);

        // Read access
        public NetworkVariable<float> Hp { get { return _hp; } }
        public NetworkVariable<bool> IsAlive { get { return _isAlive; } }

        // The head hitbox, every headset: HealthBar sits above it, Feedback puts the death burst on it
        // Null until OnNetworkSpawn has found the hitboxes
        public Transform Head { get { return _head; } }

        // Every headset
        private Hitbox[] _hitboxes;
        private Transform _head;  // the head hitbox, the host also uses its position for IsOriginNearShooter

        // Host only
        private double _spawnTime;
        private bool _hasActedSinceSpawn;
        private double _lastDamageTime = double.NegativeInfinity;
        private double _deathTime = double.NegativeInfinity;
        private double _lastShotTime = double.NegativeInfinity;

        // Add to All, set Local if IsOwner
        // Find the hitboxes (GetComponentsInChildren<Hitbox>), _head = the one with IsHead
        // Every hitbox: ApplyLayer(IsOwner), SetEnabled(IsAlive)
        // Subscribe to IsAlive changes: turn the hitboxes on or off
        // TODO(Moustafa)
        public override void OnNetworkSpawn()
        {
        }

        // Remove from All, clear Local, unsubscribe
        // TODO(Moustafa)
        public override void OnNetworkDespawn()
        {
        }

        // The PlayerCombat of this client, or null, works on every headset
        // TODO(Moustafa)
        public static PlayerCombat Get(ulong clientId)
        {
            return null;
        }

        // Shooter to host

        // The shooter reports one shot (see "One shot, on the host")
        // InvokePermission Owner: only this player's own headset can report shots for this player
        // victimClientId = NoVictim for a miss, hitPoint = where the laser stopped
        // TODO(Moustafa)
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void ReportShotRpc(Vector3 origin, Vector3 hitPoint, ulong victimClientId, bool headshot)
        {
        }

        // Host API, called by MatchManager and PlayerMovement

        // Spawn (respawn or match start): HP = MaxHp, IsAlive = true, _spawnTime = now,
        // _hasActedSinceSpawn = false, _lastDamageTime = minus infinity
        // TODO(Moustafa)
        public void ServerResetForSpawn()
        {
        }

        // The player shot or teleported, their spawn protection ends now
        // PlayerMovement calls it on a confirmed teleport, ReportShotRpc on a counted shot
        // TODO(Moustafa)
        public void ServerOnActed()
        {
        }

        // The match ended: IsAlive = false, no death and no kill (MatchManager sends everyone to the lobby area)
        // TODO(Moustafa)
        public void ServerClearForLobby()
        {
        }

        // Host only

        // One hit on this player
        //   ignore if not IsAlive, or CombatRules.IsSpawnProtected(_spawnTime, now, SpawnProtectionSeconds, _hasActedSinceSpawn)
        //   HP = max(0, HP - CombatRules.Damage(headshot, BodyDamage, HeadDamage)), _lastDamageTime = now
        //   HitRpc(attackerClientId, new HP, headshot)
        //   HP at 0: ServerDie(attackerClientId)
        // TODO(Moustafa)
        private void ServerApplyDamage(ulong attackerClientId, bool headshot)
        {
        }

        // This player died, killed by killerClientId
        //   IsAlive = false, _deathTime = now
        //   killer: PlayerProfile.AddKill, PlayerMovement.ServerAddCharge (kill bonus, even if the killer died in a trade)
        //   We need to make sure a killer who is no longer in the match (Get returns null) is skipped, the victim still dies and respawns
        //   DeathRpc(killerClientId)
        //   MatchManager.ServerOnPlayerKilled(OwnerClientId, killerClientId): lobby area, respawn timer, win check
        // TODO(Moustafa)
        private void ServerDie(ulong killerClientId)
        {
        }

        // Host, every frame while alive: Hp = CombatRules.RegenHp(...), only write it when the value changed
        // TODO(Moustafa)
        private void Update()
        {
        }

        // Host to headsets, they only show things

        // Everyone except the shooter: RaiseShotFired (beam and sound), the shooter already showed it
        // TODO(Moustafa)
        [Rpc(SendTo.NotOwner)]
        private void ShowShotRpc(Vector3 from, Vector3 to, bool hitSomething)
        {
        }

        // Everyone (the host too, it runs right away there), sent on the victim's object:
        // RaisePlayerDamaged(OwnerClientId, attackerClientId, newHp, headshot)
        // TODO(Moustafa)
        [Rpc(SendTo.Everyone)]
        private void HitRpc(ulong attackerClientId, float newHp, bool headshot)
        {
        }

        // Everyone: RaisePlayerKilled(OwnerClientId, killerClientId), Feedback plays the death burst at this player
        // TODO(Moustafa)
        [Rpc(SendTo.Everyone)]
        private void DeathRpc(ulong killerClientId)
        {
        }
    }
}
