// MatchSettings.cs
// Owner: Karim
// Layer: Data, ScriptableObject
//
// What this is
// A ScriptableObject is a data file you create in the Project window and fill in the Inspector, like a form
// This class is the form, our match rules are one filled-in copy of it (an asset called DefaultMatch)
//
// What the match asset holds
//   the roster: which characters can be picked, and the weapon
//   winning: 15 kills or 9 minutes, countdown, end screen
//   respawning: 4 s delay, 3 s spawn protection
//   health: 100 HP, regen after 3.5 s, full in 2.5 s
//   trades: 0.15 s window
//   safety check tolerances for reported hits
//   movement: teleport gap, leash, standard eye height
//
// Who reads it
//   MatchManager: winning, respawning
//   PlayerProfile: the characters (the roster)
//   LaserGun: the weapon
//   PlayerCombat: health, spawn protection, trades, safety checks
//   PlayerMovement and TeleportController: teleport gap
//   LocalRig: leash and eye height
// The fields are private, other code can only read them, so nothing changes the rules during a match
//
// Editor setup
//   1. In the Project window, open Assets/_Game/Data
//   2. Right-click, Create, Game, Match Settings, name the new asset DefaultMatch
//   3. The default numbers are already the decided ones
//   4. Drag Ray, Cluster, Ring into Characters (in that order) and LaserGun into Weapon

using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Game/Match Settings", fileName = "DefaultMatch")]
    public class MatchSettings : ScriptableObject
    {
        [Header("Roster")]
        [Tooltip("Characters players can pick, in picker order, the index is what PlayerProfile syncs")]
        [SerializeField] private CharacterDefinition[] _characters;
        [SerializeField] private WeaponDefinition _weapon;

        [Header("Players")]
        [SerializeField, Range(2, 5)] private int _maxPlayers = 5;

        [Header("Win")]
        [SerializeField, Range(1, 50)] private int _killsToWin = 15;
        [Tooltip("Seconds, most kills wins when it runs out, a tie goes to sudden death: 540 (9 minutes)")]
        [SerializeField, Range(60, 1800)] private int _timeLimitSeconds = 540;
        [SerializeField, Range(0f, 10f)] private float _countdownSeconds = 5f;
        [Tooltip("Seconds the results stay on screen before everyone goes back to the lobby area")]
        [SerializeField, Range(0f, 20f)] private float _endScreenSeconds = 5f;

        [Header("Respawn")]
        [SerializeField, Range(0f, 15f)] private float _respawnDelaySeconds = 4f;
        [Tooltip("Seconds of no damage after a respawn or the match start, ends early when you shoot or teleport: 3")]
        [SerializeField, Range(0f, 5f)] private float _spawnProtectionSeconds = 3f;

        [Header("Health")]
        [SerializeField, Range(10f, 500f)] private float _maxHp = 100f;
        [Tooltip("Seconds without damage before HP comes back: 3.5")]
        [SerializeField, Range(0f, 15f)] private float _regenDelaySeconds = 3.5f;
        [Tooltip("Seconds to refill from 0 to full once it starts: 2.5")]
        [SerializeField, Range(0.5f, 15f)] private float _regenTimeToFull = 2.5f;

        [Header("Trades")]
        [Tooltip("A shot from a player who died at most this long ago still counts, so both die: 0.15")]
        [SerializeField, Range(0f, 0.5f)] private float _tradeWindowSeconds = 0.15f;

        [Header("Safety checks")]
        [Tooltip("Extra metres allowed past the weapon range before a hit is refused")]
        [SerializeField, Range(0f, 5f)] private float _hitRangeTolerance = 1f;
        [Tooltip("How far a shot may start from the shooter's head before it is refused")]
        [SerializeField, Range(0.5f, 3f)] private float _originTolerance = 1.5f;

        [Header("Movement")]
        [Tooltip("Shortest time between two teleports: 0.3")]
        [SerializeField, Range(0f, 2f)] private float _minTeleportGap = 0.3f;
        [Tooltip("How far (m) your head may drift from the pad centre before the view starts to fade: 0.6")]
        [SerializeField, Range(0.2f, 1.5f)] private float _leashRadius = 0.6f;
        [Tooltip("Every player's eyes are moved to this height (m) above the pad, so tall and short players are equal: 1.65")]
        [SerializeField, Range(1.2f, 2f)] private float _standardEyeHeight = 1.65f;

        public CharacterDefinition[] Characters { get { return _characters; } }
        public WeaponDefinition Weapon { get { return _weapon; } }
        public int MaxPlayers { get { return _maxPlayers; } }
        public int KillsToWin { get { return _killsToWin; } }
        public int TimeLimitSeconds { get { return _timeLimitSeconds; } }
        public float CountdownSeconds { get { return _countdownSeconds; } }
        public float EndScreenSeconds { get { return _endScreenSeconds; } }
        public float RespawnDelaySeconds { get { return _respawnDelaySeconds; } }
        public float SpawnProtectionSeconds { get { return _spawnProtectionSeconds; } }
        public float MaxHp { get { return _maxHp; } }
        public float RegenDelaySeconds { get { return _regenDelaySeconds; } }
        public float RegenTimeToFull { get { return _regenTimeToFull; } }
        public float TradeWindowSeconds { get { return _tradeWindowSeconds; } }
        public float HitRangeTolerance { get { return _hitRangeTolerance; } }
        public float OriginTolerance { get { return _originTolerance; } }
        public float MinTeleportGap { get { return _minTeleportGap; } }
        public float LeashRadius { get { return _leashRadius; } }
        public float StandardEyeHeight { get { return _standardEyeHeight; } }

        // Unity calls this in the editor whenever a value changes, never during the game
        // Warn if: the roster is empty or has an empty slot, the weapon is missing,
        // the weapon's headDamage is at or above maxHp (a headshot would kill from full)
        // TODO(Karim) [X] Completed
        private void OnValidate()
        {
            if (_characters == null || _characters.Length == 0)
            {
                Debug.LogWarning($"[MatchSettings] {name}: the character roster is empty", this);
            }
            else
            {
                for (int i = 0; i < _characters.Length; i++)
                {
                    if (_characters[i] == null)
                    {
                        Debug.LogWarning($"[MatchSettings] {name}: character slot {i} is empty", this);
                    }
                }
            }
            if (_weapon == null)
            {
                Debug.LogWarning($"[MatchSettings] {name}: no weapon assigned", this);
            }
            else if (_weapon.HeadDamage >= _maxHp)
            {
                Debug.LogWarning($"[MatchSettings] {name}: head damage ({_weapon.HeadDamage}) would kill from full HP ({_maxHp})", this);
            }
        }
    }
}
