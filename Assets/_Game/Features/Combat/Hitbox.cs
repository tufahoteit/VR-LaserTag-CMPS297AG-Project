// Hitbox.cs
// Owner: Joseph
// Layer: Feature, Combat (on the HeadHitbox and Vest objects of the player prefab, on every headset)
//
// What this is
// Marks a collider as part of a player, so a laser hit knows who was hit and where (head or body)
// LaserGun reads it on the shooter's headset
//
// What you can hit is what you see
// The template avatar is a head and two hands, no body, so an invisible torso hitbox would count shots into empty air
// Two targets, both visible:
//   head: the template's head, the sphere is sized to the cartoon head (bigger than a real one)
//   vest: a box under the head in the player's colour, like a laser tag vest, the box collider is exactly the box you see
// Hands and the gun never count
// Your own vest is hidden on your headset, so it never blocks your view when you look down
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
//   the vest: under "Base", the Torso Parent of XR Avatar IK on the same prefab (follows the body, turns with it)
//
// Editor setup
//   1. Open XRI_Network_Player_Avatar, under "Head" create an empty child named HeadHitbox, position 0
//      add a Sphere Collider (Is Trigger on) and this component, tick Is Head
//      size the radius in the Scene view until the green outline matches the visible head (about 0.15 to 0.18 m)
//   2. Under "Base" (Avatar_Head > Armature > Base) create a Cube named Vest, about 0.4 x 0.5 x 0.25 m,
//      just below the head (tuned in Play mode), give it a transparent unlit material
//      keep its Box Collider, tick Is Trigger, add this component (Is Head off)
//   3. No layer and no colour to set by hand, PlayerCombat sets the layer, this script colours the vest

using UnityEngine;

namespace Game
{
    public class Hitbox : MonoBehaviour
    {
        [Tooltip("Ticked on the head (head damage), off on the vest (body damage)")]
        [SerializeField] private bool _isHead;

        public bool IsHead { get { return _isHead; } }

        // The player this hitbox belongs to, found in Awake
        public PlayerCombat Player { get; private set; }

        private Collider _collider;
        private Renderer _renderer;       // the vest's visible box, none on the head
        private PlayerProfile _profile;   // for the vest's colour
        private MaterialPropertyBlock _block;

        // Find Player (GetComponentInParent), the collider and the renderer on this object (the vest has one, the head doesn't),
        // PlayerProfile (GetComponentInParent), make _block
        // We need to make sure both exist, log an error naming this object if one is missing
        // TODO(Joseph) [X] Completed
        private void Awake()
        {
            Player = GetComponentInParent<PlayerCombat>();
            _collider = GetComponent<Collider>();
            _renderer = GetComponent<Renderer>();
            _profile = GetComponentInParent<PlayerProfile>();
            _block = new MaterialPropertyBlock();
            if (Player == null)
            {
                Debug.LogError($"[Hitbox] {name}: no PlayerCombat in its parents", this);
            }
            if (_collider == null)
            {
                Debug.LogError($"[Hitbox] {name}: no collider on this object", this);
            }
        }

        // Puts this hitbox on the right layer for this headset
        //   isLocalPlayer true (your own body): Ignore Raycast, your laser can never hit yourself
        //   isLocalPlayer false (everyone else): Hitbox (LayerMask.NameToLayer(Layers.Hitbox))
        // The vest's renderer: off on your own headset, on for everyone else
        // TODO(Joseph) [X] Completed
        public void ApplyLayer(bool isLocalPlayer)
        {
            int layer = isLocalPlayer ? LayerMask.NameToLayer("Ignore Raycast") : LayerMask.NameToLayer(Layers.Hitbox);
            if (layer < 0)
            {
                Debug.LogError($"[Hitbox] {name}: layer missing, add the Hitbox layer in Project Settings > Tags and Layers", this);
                return;
            }
            gameObject.layer = layer;
            if (_renderer != null)
            {
                _renderer.enabled = !isLocalPlayer;
            }
        }

        // Turn the collider on or off, PlayerCombat calls it when the player spawns or dies
        // TODO(Joseph) [X] Completed
        public void SetEnabled(bool enabled)
        {
            if (_collider != null)
            {
                _collider.enabled = enabled;
            }
        }

        // The vest takes the player's colour every frame (the colour can arrive a moment after spawning)
        // Skip if there is no renderer (the head) or it is hidden (your own vest)
        // _profile.Color with a little transparency (alpha 0.8), set with _block on _BaseColor
        // TODO(Joseph) [X] Completed
        private void LateUpdate()
        {
            if (_renderer == null || !_renderer.enabled || _profile == null)
            {
                return;
            }
            Color color = _profile.Color;
            color.a = 0.8f;
            _renderer.GetPropertyBlock(_block);
            _block.SetColor("_BaseColor", color);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
