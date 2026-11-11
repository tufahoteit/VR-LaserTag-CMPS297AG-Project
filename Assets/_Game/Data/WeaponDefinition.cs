// WeaponDefinition.cs
// Owner: Joseph
// Layer: Data, ScriptableObject
//
// What this is
// A ScriptableObject is a data file you create in the Project window and fill in the Inspector, like a form
// This class is the form, the laser gun is one filled-in copy of it (an asset called LaserGun)
//
// What the weapon asset holds
//   damage: body 25, head 50, so every headshot saves one shot
//     4 body = 0.75 s, 1 head + 2 body = 0.5 s, 2 heads = 0.25 s
//   fire rate: semi-auto, one shot per trigger press, at most one shot every 0.25 s
//   range: 40 m, the whole map is about 27 m corner to corner
//   energy: 30 shots from a full bar, refills after you stop shooting
//
// Who reads it
//   LaserGun on your headset: fire rate, range and energy
//   PlayerCombat on the host: damage, plus the fire rate and range for its safety checks
// The fields are private, other code can only read them, so nothing changes the gun during a match
//
// Editor setup
//   1. In the Project window, open Assets/_Game/Data
//   2. Right-click, Create, Game, Weapon, name the new asset LaserGun
//   3. The default values are already the decided ones, nothing to change
//   4. Click DefaultMatch and drag LaserGun into its Weapon slot

using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Game/Weapon", fileName = "NewWeapon")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Damage")]
        [Tooltip("Damage to the torso: 25")]
        [SerializeField, Range(1f, 100f)] private float _bodyDamage = 25f;

        [Tooltip("Damage to the head: 50, must stay below max HP so a headshot never kills from full")]
        [SerializeField, Range(1f, 100f)] private float _headDamage = 50f;

        [Header("Firing")]
        [Tooltip("Shortest time between two shots, this is the fire rate: 0.25 s (4 shots per second)")]
        [SerializeField, Range(0.05f, 1f)] private float _minShotGap = 0.25f;

        [Tooltip("Longest shot in metres, the whole map is about 27 m corner to corner")]
        [SerializeField, Range(10f, 150f)] private float _shotRange = 40f;

        [Header("Energy")]
        [SerializeField, Range(10f, 200f)] private float _maxEnergy = 100f;

        [Tooltip("Shots from a full bar to empty: 30")]
        [SerializeField, Range(1, 100)] private int _shotsPerFullBar = 30;

        [Tooltip("Energy back per second once it refills")]
        [SerializeField, Range(1f, 200f)] private float _energyRegenPerSecond = 30f;

        [Tooltip("Seconds after the last shot before energy starts coming back")]
        [SerializeField, Range(0f, 3f)] private float _energyRegenDelay = 0.6f;

        public float BodyDamage { get { return _bodyDamage; } }
        public float HeadDamage { get { return _headDamage; } }
        public float MinShotGap { get { return _minShotGap; } }
        public float ShotRange { get { return _shotRange; } }
        public float MaxEnergy { get { return _maxEnergy; } }
        public int ShotsPerFullBar { get { return _shotsPerFullBar; } }
        public float EnergyRegenPerSecond { get { return _energyRegenPerSecond; } }
        public float EnergyRegenDelay { get { return _energyRegenDelay; } }

        // Energy one shot costs, worked out so a full bar gives exactly ShotsPerFullBar shots
        // TODO(Joseph): MaxEnergy / ShotsPerFullBar
        public float CostPerShot
        {
            get { return 0f; }
        }

        // Unity calls this in the editor whenever a value changes, never during the game
        // Warn if headDamage is below bodyDamage, or shotsPerFullBar is 0
        // The "headDamage below max HP" check lives in MatchSettings, it knows both numbers
        // TODO(Joseph)
        private void OnValidate()
        {
        }
    }
}
