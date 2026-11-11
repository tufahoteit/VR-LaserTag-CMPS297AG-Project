// CharacterDefinition.cs
// Owner: Joseph
// Layer: Data, ScriptableObject
//
// What this is
// A ScriptableObject is a data file you create in the Project window and fill in the Inspector, like a form
// This class is the form, each character is one filled-in copy of it (an asset)
// We have three: Ray, Cluster, Ring
//
// What a character asset holds
//   its name and description (for the character picker)
//   which moveset it uses (Ray, Cluster or Ring), the movement rules themselves are in the moveset classes
//   how many teleport charges it has when full
//   how many seconds it takes to get one charge back
//
// Who reads it
//   PlayerMovement: moveset, charges and recharge time
//   LobbyExtras and PlayerHUD: name and description
// The fields are private, other code can only read them, so nothing changes a character during a match
//
// Editor setup
//   1. In the Project window, open Assets/_Game/Data and create a folder called Characters
//   2. Right-click inside it, Create, Game, Character, name the new asset Ray
//   3. Click Ray and fill it in the Inspector: Display Name Ray, Moveset Ray, Max Charges 3, Recharge Seconds 4.5
//   4. Repeat for Cluster: Display Name Cluster, Moveset Cluster, Max Charges 6, Recharge Seconds 4.5
//   5. Repeat for Ring: Display Name Ring, Moveset Ring, Max Charges 4, Recharge Seconds 4.5
//   6. Click DefaultMatch and drag Ray, Cluster, Ring into its Characters list, in that order
//      (the order is the order in the character picker)

using Game.Core;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Game/Character", fileName = "NewCharacter")]
    public class CharacterDefinition : ScriptableObject
    {
        [Tooltip("Name shown in the character picker and on the HUD")]
        [SerializeField] private string _displayName = "Ray";

        [Tooltip("Which movement pattern this character uses")]
        [SerializeField] private MovesetKind _moveset = MovesetKind.Ray;

        [Tooltip("Teleport charges when full: Ray 3, Cluster 6, Ring 4")]
        [SerializeField, Range(1, 10)] private int _maxCharges = 3;

        [Tooltip("Seconds to get one charge back: 4.5 for every character")]
        [SerializeField, Range(1f, 20f)] private float _rechargeSeconds = 4.5f;

        [Tooltip("Short description for the character picker")]
        [SerializeField, TextArea] private string _description;

        public string DisplayName { get { return _displayName; } }
        public MovesetKind Moveset { get { return _moveset; } }
        public int MaxCharges { get { return _maxCharges; } }
        public float RechargeSeconds { get { return _rechargeSeconds; } }
        public string Description { get { return _description; } }

        // This character's moveset, PlayerMovement asks for it once and keeps it
        // TODO(Joseph) [X] Completed
        public IMoveset CreateMoveset()
        {
            return MovesetFactory.Create(_moveset);
        }

        // Unity calls this in the editor whenever a value changes, never during the game
        // Warn about bad data, e.g. "[Character] Ray: display name is empty"
        // TODO(Joseph) [X] Completed
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                Debug.LogWarning($"[Character] {name}: display name is empty", this);
            }
            if (_maxCharges < 1)
            {
                Debug.LogWarning($"[Character] {name}: max charges must be at least 1", this);
            }
        }
    }
}
