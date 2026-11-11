// Hitbox.cs
// Owner: Joseph
// Layer: Feature, Combat (on the HeadHitbox and TorsoHitbox objects of the player prefab, on every headset)
//
// What this is
// Marks a collider as part of a player, so a laser hit knows who was hit and where (head or body)
// LaserGun reads it on the shooter's headset, it does nothing on its own
//
// Layers
// PlayerCombat calls ApplyLayer when the player spawns
//   on your own headset: your hitboxes go to Ignore Raycast, your laser can never hit yourself
//   on every other headset: they go to Hitbox, so others can hit you
// PlayerCombat turns the colliders off while the player is dead, shots pass through players in the lobby area
//
// Why separate objects
// The colliders go on new child objects of our own, not on the template's objects
// ApplyLayer changes the layer of this object only, so the template's Head keeps its own layer and scripts
//
// Template check (done)
//   the head: the "Head" object of XRI_Network_Player_Avatar (follows the networked head)
//   the torso: the Torso Parent of the XR Avatar IK component on the same prefab (follows the body, turns with it)
//
// Editor setup
//   1. Open XRI_Network_Player_Avatar, under "Head" create an empty child named HeadHitbox, position 0
//      add a Sphere Collider (radius about 0.12 m, Is Trigger on) and this component, tick Is Head
//   2. On the root, click the Torso Parent field of XR Avatar IK, Unity highlights that object in the Hierarchy
//      under it create an empty child named TorsoHitbox, about 0.35 m below the head (tuned in Play mode)
//      add a Capsule Collider (radius about 0.18 m, height about 0.6 m, Is Trigger on) and this component
//   3. No layer to set by hand, PlayerCombat sets it when the player spawns

using UnityEngine;

namespace Game
{
    public class Hitbox : MonoBehaviour
    {
        [Tooltip("Ticked on the head collider (head damage), off on the torso (body damage)")]
        [SerializeField] private bool _isHead;

        public bool IsHead { get { return _isHead; } }

        // The player this hitbox belongs to, found in Awake
        public PlayerCombat Player { get; private set; }

        private Collider _collider;

        // Find Player (GetComponentInParent) and the collider on this object
        // We need to make sure both exist, log an error naming this object if one is missing
        // TODO(Joseph)
        private void Awake()
        {
        }

        // Puts this hitbox on the right layer for this headset
        //   isLocalPlayer true (your own body): Ignore Raycast, your laser can never hit yourself
        //   isLocalPlayer false (everyone else): Hitbox (LayerMask.NameToLayer(Layers.Hitbox))
        // TODO(Joseph)
        public void ApplyLayer(bool isLocalPlayer)
        {
        }

        // Turn the collider on or off, PlayerCombat calls it when the player spawns or dies
        // TODO(Joseph)
        public void SetEnabled(bool enabled)
        {
        }
    }
}
