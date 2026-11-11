// LaserGun.cs
// Owner: Joseph
// Layer: Feature, Combat (NetworkBehaviour on the gun, under the right hand of the player prefab, one per player)
//
// What this is
// The gun in your right hand: the trigger, the energy, the laser raycast and the beam
// Your headset decides what your laser hit, the host decides the damage (PlayerCombat), this file never changes HP
//
// Why the shooter decides the hit
// Hitscan, "favour the shooter": if the enemy was under your laser on your screen, it's a hit
// The host sees everyone slightly in the past (Wi-Fi delay), checking hits there would miss shots that looked perfect
//
// One shot, on your headset
//   1. Trigger pressed (semi-auto: one shot per press, holding does nothing) and you are alive
//      a press up to 0.1 s before the fire gap ends is remembered and fires as soon as the gap allows (press buffer),
//      so pressing slightly too fast never eats a shot
//   2. Wall check: a line from your eyes to the muzzle, if a wall is in between your gun is inside a wall,
//      the gun clicks empty and nothing is fired (a gun pushed through a wall can never shoot from the other side)
//      being close to a wall or leaning on it is fine, only a muzzle past a wall surface is blocked
//      the same check also stops blind fire (no extra code): a gun held around a corner your eyes are behind
//      has a wall between your eyes and the muzzle, so you must lean out far enough to see before you can shoot
//      on purpose: in real laser tag players agree blind fire isn't allowed, here the check enforces it
//   3. Energy: LaserEnergyCharging.TryFire, not enough energy: empty click
//   4. Raycast from the muzzle forward, ShotRange, ShotMask: the first thing hit counts (walls stop the laser)
//      a Hitbox: that player, head or body, anything else: a miss that ends where the laser stopped
//   5. Right away on your headset: RaiseShotFired (your beam and sound, no waiting), then PlayerCombat.ReportShotRpc
//   The host then shows the shot to everyone else and applies the damage
//
// Who sees what
//   Energy: every headset, the strip on your gun is visible to everyone like real laser tag gun lights
//     only your headset writes it (owner-write NetworkVariable), the host never touches it
//   Beams: every LaserGun listens to ShotFired and draws its own player's shots
//     a beam lasts 0.07 s and shots are at least 0.25 s apart, so a laser always shows as separate pulses
//
// Time
// The gun uses your headset's own clock (Time.timeAsDouble), it never compares times with another headset
//
// When it fires
// Firing happens in LateUpdate, after the template has moved the avatar's hand to your controller this frame
// ([DefaultExecutionOrder(100)] makes this LateUpdate run after the template's), so the laser leaves from where your hand really is
//
// Template check (done)
//   the gun goes under "Right Hand" of XRI_Network_Player_Avatar, not under "Right Hand Networked"
//     "Right Hand" stays active and follows your controller on your headset, synced to the others
//     "Right Hand Networked" (the hand model) is turned off by the template on your own headset, a gun there would stop working
//   the right trigger: XRI Right Interaction/Activate in the template's XRI Default Input Actions
//
// Editor setup
//   1. Import the Kenney Blaster Kit into Assets/_Game/Art/Blaster, pick one blaster model
//   2. Open XRI_Network_Player_Avatar, drag the model under "Right Hand", turn it so the barrel points forward
//   3. Add this component to the model's root object, remove every collider from the model (the gun must never block a shot)
//   4. Add an empty child at the barrel tip named Muzzle, its blue arrow (forward) along the barrel, drag it into Muzzle
//   5. Add a thin cube child under the gun named EnergyStrip, remove its collider, drag it into Energy Strip
//   6. Add a LineRenderer child named Beam (width about 0.01 m, unlit material), drag it into Beam
//   7. Add an AudioSource to the gun (Spatial Blend 1), drag the empty click sound into Empty Click
//   8. Drag DefaultMatch into Settings and XRI Right Interaction/Activate into Fire Action

using Game.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    [DefaultExecutionOrder(100)]
    public class LaserGun : NetworkBehaviour
    {
        [SerializeField] private MatchSettings _settings;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _energyStrip;
        [SerializeField] private LineRenderer _beam;
        [SerializeField] private AudioClip _emptyClick;

        [Tooltip("Right trigger: XRI Right Interaction/Activate")]
        [SerializeField] private InputActionReference _fireAction;

        // How long a beam stays visible, shorter than the 0.25 s fire gap so one LineRenderer is enough
        private const float BeamSeconds = 0.07f;

        // How early a press may come before the fire gap ends and still fire
        private const float PressBufferSeconds = 0.1f;

        // Synced value, only the owner writes it, everyone reads it
        private readonly NetworkVariable<float> _energy = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<float> Energy { get { return _energy; } }

        // Every headset
        private PlayerCombat _combat;
        private AudioSource _audio;
        private double _beamHideTime;
        private Vector3 _stripFullScale;
        private MaterialPropertyBlock _block;

        // Owner only
        private LaserEnergyCharging _gun;
        private double _pressTime = double.NegativeInfinity;  // last trigger press not fired yet, minus infinity = none

        // Everyone: find PlayerCombat (GetComponentInParent) and the AudioSource, subscribe to GameEvents.ShotFired
        // Owner only: create LaserEnergyCharging from _settings.Weapon (MaxEnergy, CostPerShot, EnergyRegenPerSecond,
        //   EnergyRegenDelay, MinShotGap), enable _fireAction,
        //   subscribe to _combat.IsAlive changes: false to true is a respawn, Reset the energy and write Energy
        // TODO(Joseph) [X] Completed
        public override void OnNetworkSpawn()
        {
            _combat = GetComponentInParent<PlayerCombat>();
            _audio = GetComponent<AudioSource>();
            _block = new MaterialPropertyBlock();
            if (_energyStrip != null)
            {
                _stripFullScale = _energyStrip.localScale;
            }
            if (_beam != null)
            {
                _beam.useWorldSpace = true;
                _beam.enabled = false;
            }
            GameEvents.ShotFired += OnShotFired;

            if (IsOwner)
            {
                WeaponDefinition weapon = _settings.Weapon;
                _gun = new LaserEnergyCharging(weapon.MaxEnergy, weapon.CostPerShot, weapon.EnergyRegenPerSecond,
                                               weapon.EnergyRegenDelay, weapon.MinShotGap);
                _gun.Reset();
                _energy.Value = _gun.Energy;
                if (_fireAction != null)
                {
                    _fireAction.action.Enable();
                }
                if (_combat != null)
                {
                    _combat.IsAlive.OnValueChanged += OnAliveChanged;
                }
            }
        }

        // Unsubscribe everything, disable _fireAction on the owner
        // TODO(Joseph) [X] Completed
        public override void OnNetworkDespawn()
        {
            GameEvents.ShotFired -= OnShotFired;
            if (IsOwner)
            {
                if (_fireAction != null)
                {
                    _fireAction.action.Disable();
                }
                if (_combat != null)
                {
                    _combat.IsAlive.OnValueChanged -= OnAliveChanged;
                }
            }
        }

        // Owner only:
        //   _gun.Tick, if it changed write Energy
        //   trigger pressed this frame (WasPressedThisFrame): _pressTime = now
        //   _combat.IsAlive and now - _pressTime is at most PressBufferSeconds: TryFire
        // Everyone: UpdateEnergyStrip, hide the beam once _beamHideTime has passed
        // LateUpdate, not Update: the hand has already moved this frame (see "When it fires")
        // TODO(Joseph) [X] Completed
        private void LateUpdate()
        {
            double now = Time.timeAsDouble;
            if (IsSpawned && IsOwner && _gun != null)
            {
                if (_gun.Tick(now, Time.deltaTime))
                {
                    _energy.Value = _gun.Energy;
                }
                if (_fireAction != null && _fireAction.action.WasPressedThisFrame())
                {
                    _pressTime = now;
                }
                if (_combat != null && _combat.IsAlive.Value && now - _pressTime <= PressBufferSeconds)
                {
                    TryFire();
                }
            }

            UpdateEnergyStrip();
            if (_beam != null && _beam.enabled && now >= _beamHideTime)
            {
                _beam.enabled = false;
            }
        }

        // Steps 2 to 5 from the header, in this order
        //   the fire gap hasn't passed yet (now - _gun.LastShotTime < MinShotGap): do nothing, keep _pressTime, the buffer fires it later
        //   IsMuzzleInWall: empty click, log it in the editor, clear _pressTime
        //   _gun.TryFire(now) refused (not enough energy): empty click, clear _pressTime
        //   otherwise: write Energy, FindHit, RaiseShotFired, ReportShotRpc, clear _pressTime
        // TODO(Joseph) [X] Completed
        private void TryFire()
        {
            double now = Time.timeAsDouble;
            if (now - _gun.LastShotTime < _gun.MinShotGap)
            {
                return;
            }
            if (IsMuzzleInWall())
            {
                PlayEmptyClick();
            #if UNITY_EDITOR
                Debug.Log("[LaserGun] Shot blocked: a wall is between your eyes and the muzzle");
            #endif
                _pressTime = double.NegativeInfinity;
                return;
            }
            if (!_gun.TryFire(now))
            {
                PlayEmptyClick();
                _pressTime = double.NegativeInfinity;
                return;
            }

            _energy.Value = _gun.Energy;
            Vector3 origin = _muzzle.position;
            FindHit(out Vector3 hitPoint, out ulong victimClientId, out bool headshot);
            bool hitSomething = victimClientId != PlayerCombat.NoVictim
                                || Vector3.Distance(origin, hitPoint) < _settings.Weapon.ShotRange - 0.01f;
            GameEvents.RaiseShotFired(OwnerClientId, origin, hitPoint, hitSomething);
            _combat.ReportShotRpc(origin, hitPoint, victimClientId, headshot);
            _pressTime = double.NegativeInfinity;
        }

        // True if a wall is between your eyes and the muzzle, or your head is inside a wall
        //   Physics.Linecast(LocalRig.Instance.Head.position, _muzzle.position, Layers.CoverMask)
        //   or LocalRig.Instance.IsHeadInsideGeometry()
        // We need to make sure both are checked: a Linecast that starts inside a wall doesn't see that wall,
        // so with your head in a wall and the gun on the other side, the line alone would let you fire blind through it
        // TODO(Joseph) [X] Completed
        private bool IsMuzzleInWall()
        {
            if (LocalRig.Instance == null || LocalRig.Instance.Head == null)
            {
                return false;
            }
            bool wallBetween = Physics.Linecast(LocalRig.Instance.Head.position, _muzzle.position, Layers.CoverMask,
                                                QueryTriggerInteraction.Ignore);
            return wallBetween || LocalRig.Instance.IsHeadInsideGeometry();
        }

        // Step 4: Physics.Raycast(muzzle position, muzzle forward, ShotRange, Layers.ShotMask, QueryTriggerInteraction.Collide)
        //   Collide, because the hitboxes are triggers
        //   hit a Hitbox: victimClientId = its Player.OwnerClientId, headshot = its IsHead
        //   hit a wall: victimClientId = PlayerCombat.NoVictim
        //   hit nothing: hitPoint = muzzle + forward * ShotRange, victimClientId = PlayerCombat.NoVictim
        // TODO(Joseph) [X] Completed
        private void FindHit(out Vector3 hitPoint, out ulong victimClientId, out bool headshot)
        {
            Vector3 origin = _muzzle.position;
            Vector3 forward = _muzzle.forward;
            float range = _settings.Weapon.ShotRange;
            victimClientId = PlayerCombat.NoVictim;
            headshot = false;

            RaycastHit hit;
            if (Physics.Raycast(origin, forward, out hit, range, Layers.ShotMask, QueryTriggerInteraction.Collide))
            {
                hitPoint = hit.point;
                Hitbox hitbox = hit.collider.GetComponent<Hitbox>();
                if (hitbox != null && hitbox.Player != null)
                {
                    victimClientId = hitbox.Player.OwnerClientId;
                    headshot = hitbox.IsHead;
                }
            }
            else
            {
                hitPoint = origin + forward * range;
            }
        }

        // Listener for GameEvents.ShotFired (runs by itself on every shot, on every headset, connected in OnNetworkSpawn)
        // Only for this gun's own player (shooter == OwnerClientId): beam from -> to in the player's colour
        // (PlayerProfile.Color), _beamHideTime = now + BeamSeconds
        // TODO(Joseph) [X] Completed
        private void OnShotFired(ulong shooter, Vector3 from, Vector3 to, bool hitSomething)
        {
            if (shooter != OwnerClientId || _beam == null)
            {
                return;
            }
            PlayerProfile profile = PlayerProfile.Get(OwnerClientId);
            Color color = profile != null ? profile.Color : Color.white;
            _beam.positionCount = 2;
            _beam.SetPosition(0, from);
            _beam.SetPosition(1, to);
            _beam.startColor = color;
            _beam.endColor = color;
            _beam.enabled = true;
            _beamHideTime = Time.timeAsDouble + BeamSeconds;
        }

        // Owner only, listener for IsAlive changes: false to true is a respawn, the energy refills
        private void OnAliveChanged(bool previous, bool current)
        {
            if (!previous && current && _gun != null)
            {
                _gun.Reset();
                _energy.Value = _gun.Energy;
                _pressTime = double.NegativeInfinity;
            }
        }

        private void PlayEmptyClick()
        {
            if (_audio != null && _emptyClick != null)
            {
                _audio.PlayOneShot(_emptyClick);
            }
        }

        // Scale the strip's length by Energy / MaxEnergy, colour from green (full) to red (empty)
        // TODO(Joseph) [X] Completed
        private void UpdateEnergyStrip()
        {
            if (!IsSpawned || _energyStrip == null || _block == null || _settings == null || _settings.Weapon == null)
            {
                return;
            }
            float maxEnergy = _settings.Weapon.MaxEnergy;
            float fraction = maxEnergy > 0f ? Mathf.Clamp01(_energy.Value / maxEnergy) : 0f;
            _energyStrip.localScale = new Vector3(_stripFullScale.x, _stripFullScale.y, _stripFullScale.z * fraction);

            Renderer strip = _energyStrip.GetComponent<Renderer>();
            if (strip != null)
            {
                strip.GetPropertyBlock(_block);
                _block.SetColor("_BaseColor", Color.Lerp(Color.red, Color.green, fraction));
                strip.SetPropertyBlock(_block);
            }
        }
    }
}
