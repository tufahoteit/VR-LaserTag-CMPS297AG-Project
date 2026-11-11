// Feedback.cs
// Owner: Joe
// Layer: Feature, Feedback (one object in the scene, local on every headset, sounds and effects only)
//
// What this is
// Every sound, vibration and small effect in the game, in one place
// It listens to GameEvents and reacts on this headset, it never changes the game
//
// Who hears what
//   teleports (the footsteps of this game):
//     the teleporter hears only the arrive sound, flat, as confirmation
//     everyone else hears depart at the old pad and arrive at the new pad, both 3D
//     leaving and arriving sound different on purpose: an arrival near you is a threat, a departure means it's gone
//   spawns: a soft spawn sound at the pad, 3D for everyone else (they hear someone appear), flat for you ("you're back")
//     it sounds different from teleport arrive, so players can tell a fresh (spawn protected) player from one who teleported in
//   shots: the fire sound at the shooter's muzzle, 3D, for every shot including yours
//   hits: the hit marker only for the attacker (a sharper one on a headshot), the hurt sound only for the victim, both flat
//   kills: the kill sound only for the killer, flat
//     the death sound and a death burst in the player's colour at the victim's head, 3D, for everyone
//   kill bonus charge: a small chime, only for you (ChargeGained is only raised on your own headset)
//   refused teleport: the denied sound, only for you, flat (no text in VR mid-fight)
//   match: beeps on the last 3 seconds of the countdown, a start sound and an end sound, flat, for everyone
// Flat (Spatial Blend 0, straight in your ears) is for things about you, 3D is for things about other players
//
// Vibration (Quest controllers have vibration motors, touch is a third feedback channel next to sight and sound)
//   your shot: a short light pulse on the right controller, so the gun feels like it fired
//   you got hit: a stronger pulse on both controllers, felt even when the sound is drowned out or you look elsewhere
//
// 3D sound settings, the same for every 3D sound, set in code
//   Spatial Blend 1, full volume up to 2 m, fading to silent at 25 m (Linear rolloff), Doppler 0
//   so you hear where a player is and roughly how far, Doppler would make teleports warble
//   Unity's audio engine does the rest: louder or quieter with distance, left or right from where the sound is
//   a small pool of AudioSources is reused, AudioSource.PlayClipAtPoint can't set these values
//
// The gun's empty click is not here, LaserGun plays it on the gun itself
//
// Template check
//   none
//
// Editor setup
//   1. Create an empty object in Main named Feedback, add this component
//   2. Drag each sound from Assets/_Game/Audio into its slot (the file names match the slot names)
//   3. Drag a transparent unlit material into Burst Material

using System.Collections;
using System.Collections.Generic;
using Game.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;

namespace Game
{
    public class Feedback : MonoBehaviour
    {
        [Header("Teleport")]
        [SerializeField] private AudioClip _teleportDepart;
        [SerializeField] private AudioClip _teleportArrive;
        [SerializeField] private AudioClip _teleportDenied;
        [SerializeField] private AudioClip _spawn;
        [SerializeField] private AudioClip _chargeGained;

        [Header("Combat")]
        [SerializeField] private AudioClip _laserFire;
        [SerializeField] private AudioClip _hitBody;
        [SerializeField] private AudioClip _hitHead;
        [SerializeField] private AudioClip _hurt;
        [SerializeField] private AudioClip _killConfirm;
        [SerializeField] private AudioClip _death;

        [Header("Match")]
        [SerializeField] private AudioClip _countdownBeep;
        [SerializeField] private AudioClip _matchStart;
        [SerializeField] private AudioClip _matchEnd;

        [Header("Death burst")]
        [Tooltip("Transparent unlit material, tinted per player in code")]
        [SerializeField] private Material _burstMaterial;

        // 3D sound settings
        private const float MinDistance = 2f;
        private const float MaxDistance = 25f;
        private const int PoolSize = 12;

        private AudioSource _flatSource;
        private readonly List<AudioSource> _pool = new List<AudioSource>();
        private int _nextPoolIndex;
        private int _lastBeepSecond;

        // Make the flat AudioSource (Spatial Blend 0) and the pool of 3D AudioSources with the settings above
        // TODO(Joe)
        private void Awake()
        {
        }

        // Subscribe to GameEvents: PlayerTeleported, PlayerSpawned, ShotFired, PlayerDamaged, PlayerKilled,
        // ChargeGained, TeleportDenied, MatchPhaseChanged
        // TODO(Joe)
        private void OnEnable()
        {
        }

        // Unsubscribe everything from OnEnable
        // TODO(Joe)
        private void OnDisable()
        {
        }

        // Countdown beeps: while the phase is Countdown, play _countdownBeep once each time
        // MatchManager.SecondsLeft() goes below 3, 2 and 1 (_lastBeepSecond remembers which already played)
        // TODO(Joe)
        private void Update()
        {
        }

        // Listeners
        // Each On... function below is a listener: it runs by itself when its GameEvent is raised on this headset
        // (connected in OnEnable with GameEvents.X += OnX), it is a local function, not an RPC, nothing is sent

        // Listener for GameEvents.PlayerTeleported
        // Your own teleport (clientId is you): PlayFlat(arrive)
        // Anyone else: Play3D(depart, old pad position), Play3D(arrive, new pad position)
        // Pad positions from MapBuilder.Instance.Map.PadWorldPosition
        // TODO(Joe)
        private void OnPlayerTeleported(ulong clientId, GridCoord from, GridCoord to)
        {
        }

        // Listener for GameEvents.PlayerSpawned
        // You spawned: PlayFlat(spawn)
        // Anyone else: Play3D(spawn, pad position)
        // TODO(Joe)
        private void OnPlayerSpawned(ulong clientId, GridCoord pad)
        {
        }

        // Listener for GameEvents.ShotFired
        // Play3D(fire, from) for every shot
        // Your own shot also: Vibrate(right hand, light, short)
        // TODO(Joe)
        private void OnShotFired(ulong shooter, Vector3 from, Vector3 to, bool hitSomething)
        {
        }

        // Listener for GameEvents.PlayerDamaged
        // You are the attacker: PlayFlat(headshot ? hitHead : hitBody)
        // You are the victim: PlayFlat(hurt), Vibrate(both hands, strong)
        // TODO(Joe)
        private void OnPlayerDamaged(ulong victim, ulong attacker, float newHp, bool headshot)
        {
        }

        // Listener for GameEvents.PlayerKilled
        // You are the killer: PlayFlat(killConfirm)
        // Everyone: Play3D(death, victim's head), ShowDeathBurst(victim's head, victim's colour)
        // The victim's head comes from PlayerCombat.Get(victim).Head, their colour from PlayerProfile.Get(victim).Color
        // TODO(Joe)
        private void OnPlayerKilled(ulong victim, ulong killer)
        {
        }

        // Listener for GameEvents.ChargeGained
        // PlayFlat(chargeGained), only ever raised on your own headset
        // TODO(Joe)
        private void OnChargeGained(ulong clientId)
        {
        }

        // Listener for GameEvents.TeleportDenied
        // PlayFlat(denied), TeleportController raises TeleportDenied when the host refused your teleport
        // TODO(Joe)
        private void OnTeleportDenied()
        {
        }

        // Listener for GameEvents.MatchPhaseChanged
        // Playing: PlayFlat(matchStart), Ended: PlayFlat(matchEnd), Countdown: _lastBeepSecond = 4 (ready for 3, 2, 1)
        // TODO(Joe)
        private void OnMatchPhaseChanged(MatchPhase phase)
        {
        }

        // Helpers

        // True if clientId is the player on this headset (NetworkManager.Singleton.LocalClientId)
        // TODO(Joe)
        private static bool IsMe(ulong clientId)
        {
            return false;
        }

        // Straight in your ears: _flatSource.PlayOneShot(clip)
        // We need to make sure a missing clip (null) is skipped, so an empty slot never crashes
        // TODO(Joe)
        private void PlayFlat(AudioClip clip)
        {
        }

        // From a place in the arena: take the next AudioSource in the pool, move it to position, play clip
        // We need to make sure a missing clip (null) is skipped
        // TODO(Joe)
        private void Play3D(AudioClip clip, Vector3 position)
        {
        }

        // Controller vibration: InputDevices.GetDeviceAtXRNode(node).SendHapticImpulse(0, amplitude, seconds)
        // TODO(Joe)
        private static void Vibrate(XRNode node, float amplitude, float seconds)
        {
        }

        // The "tagged out" effect: a sphere at position in color, grows to about 0.8 m and fades out over 0.3 s, then is destroyed
        // Built in code (CreatePrimitive, remove the collider, _burstMaterial tinted with a MaterialPropertyBlock)
        // TODO(Joe)
        private void ShowDeathBurst(Vector3 position, Color color)
        {
        }

        // The grow-and-fade animation for one death burst
        // TODO(Joe)
        private IEnumerator DeathBurstRoutine(Transform burst, Renderer renderer, Color color)
        {
            yield break;
        }
    }
}
