// GameEvents.cs
// Owner: Moustafa
// Layer: Shared
//
// A local event hub, so features don't need to know about each other
// PlayerCombat says "someone died", Feedback, PlayerHUD and Minimap each react on their own
//
// These are local C# events on one headset, not network messages
//   on the host they are raised where the decision happens (PlayerCombat, MatchManager...)
//   on the other headsets they are raised when the synced value or RPC arrives
//   (a NetworkVariable OnValueChanged, or a Show...Rpc)
// So every headset raises the same events, at about the same moment
//
// Listeners only show things (sounds, effects, HUD, minimap), they never change game state
// Game state only changes on the host, in the code that decides it

using System;
using Game.Core;
using UnityEngine;

namespace Game
{
    public static class GameEvents
    {
        // (victimClientId, killerClientId)
        public static event Action<ulong, ulong> PlayerKilled;

        // (clientId, spawnPad)
        public static event Action<ulong, GridCoord> PlayerSpawned;

        // (clientId, fromPad, toPad), Feedback plays the depart sound at fromPad and the arrive sound at toPad
        public static event Action<ulong, GridCoord, GridCoord> PlayerTeleported;

        // (shooterClientId, from, to, hitSomething), every headset draws the beam from this
        public static event Action<ulong, Vector3, Vector3, bool> ShotFired;

        // (victimClientId, attackerClientId, newHp, headshot)
        // Raised on every headset by PlayerCombat's HitRpc, which the host sends to everyone (itself too) for each counted hit
        // (the synced HP alone says nothing about who hit, so the host sends one small message per counted hit)
        // The attacker's headset shows the hit marker, a bigger sound on a headshot
        public static event Action<ulong, ulong, float, bool> PlayerDamaged;

        // (clientId), a kill gave this player +1 teleport charge, Feedback plays a small chime
        // Raised only on that player's own headset (the host sends it to the owner), nobody else hears it
        public static event Action<ulong> ChargeGained;

        // (clientId), a player disconnected, Minimap and the scoreboard remove them
        public static event Action<ulong> PlayerLeft;

        // The host refused your teleport and you snapped back, Feedback plays the denied sound
        // Raised only on your own headset, by TeleportController.OnTeleportRejected
        public static event Action TeleportDenied;

        // (newPhase): Lobby, Countdown, Playing, Ended
        public static event Action<MatchPhase> MatchPhaseChanged;

        // Raise helpers, each one calls its event with ?.Invoke(...) so a missing listener never crashes
        // TODO(Moustafa)
        public static void RaisePlayerKilled(ulong victim, ulong killer)
        {
        }

        // TODO(Moustafa)
        public static void RaisePlayerSpawned(ulong clientId, GridCoord pad)
        {
        }

        // TODO(Moustafa)
        public static void RaisePlayerTeleported(ulong clientId, GridCoord from, GridCoord to)
        {
        }

        // TODO(Moustafa)
        public static void RaiseShotFired(ulong shooter, Vector3 from, Vector3 to, bool hit)
        {
        }

        // TODO(Moustafa)
        public static void RaisePlayerDamaged(ulong victim, ulong attacker, float newHp, bool headshot)
        {
        }

        // TODO(Moustafa)
        public static void RaiseChargeGained(ulong clientId)
        {
        }

        // TODO(Moustafa)
        public static void RaisePlayerLeft(ulong clientId)
        {
        }

        // TODO(Moustafa)
        public static void RaiseTeleportDenied()
        {
        }

        // TODO(Moustafa)
        public static void RaiseMatchPhaseChanged(MatchPhase phase)
        {
        }

        // Clears every listener when play mode starts
        // Unity can keep static values between play sessions (when Domain Reload is off),
        // old listeners from the last session would then fire on destroyed objects
        // We need to make sure every event above is set to null here, a new event added later must be added here too
        // TODO(Moustafa)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
        }
    }
}
