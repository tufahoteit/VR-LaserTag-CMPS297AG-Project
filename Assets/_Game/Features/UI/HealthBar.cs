// HealthBar.cs
// Owner: Karim
// Layer: Feature, UI (on the player prefab, one per player, on every headset, visuals only)
//
// What this is
// A small bar above a player's head, only while they are hurt, like Call of Duty
// Every headset sees it, except on yourself (your own HP is on your board and in the red hurt edges)
// Like real laser tag, where a hit vest lights up for everyone to see
//
// When it shows
//   HP below max: visible
//   HP back at max (regen filled it): fades out
//   dead: hidden
// It never reacts to the hit itself, only to the HP number the host already syncs, so there is no networking here
// The host works out the HP (PlayerCombat), this file only draws it
//
// How it's built
// Three flat cubes made in code, no canvas and no prefab:
//   background: dark, full width
//   fill: its width follows HP / MaxHp, shrinking from the right, colour from green (full) to yellow to red (low)
//   trail: white, behind the fill, after a hit it waits a moment then shrinks to the fill,
//     so you see the chunk of damage you just did
// The bar sits above the player's head and turns to face your camera every frame
// It is blocked by walls like any normal object (never drawn through walls)
//
// Template check
//   none, the head comes from PlayerCombat.Head (the head hitbox)
//
// Editor setup
//   1. Add this component to the template's networked player prefab, next to PlayerCombat
//   2. Drag DefaultMatch into Settings
//   3. Drag a transparent unlit material into Bar Material (one material for every bar, each bar sets its own colours in code)

using UnityEngine;

namespace Game
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private MatchSettings _settings;

        [Tooltip("Transparent unlit material, every bar shares it and sets its own colours in code")]
        [SerializeField] private Material _barMaterial;

        [Header("Size and place (m)")]
        [SerializeField, Range(0.2f, 0.8f)] private float _width = 0.4f;
        [SerializeField, Range(0.02f, 0.1f)] private float _height = 0.05f;
        [SerializeField, Range(0.1f, 0.6f)] private float _heightAboveHead = 0.3f;

        [Header("Timing (s)")]
        [SerializeField, Range(0.1f, 2f)] private float _fadeSeconds = 0.5f;
        [Tooltip("How long the white trail waits after a hit before it shrinks to the fill")]
        [SerializeField, Range(0f, 1f)] private float _trailDelay = 0.4f;
        [Tooltip("How fast the trail shrinks, in bar widths per second")]
        [SerializeField, Range(0.5f, 5f)] private float _trailSpeed = 2f;

        private PlayerCombat _combat;
        private Transform _root;
        private Transform _background;
        private Transform _fill;
        private Transform _trail;
        private MaterialPropertyBlock _block;

        private float _alpha;
        private float _trailFraction = 1f;
        private float _lastFraction = 1f;
        private float _trailWaitUntil;

        // Find PlayerCombat on this object, BuildBar, start hidden
        // TODO(Karim)
        private void Awake()
        {
        }

        // Make _root (an empty child) and the three cubes under it, remove their colliders, give them _barMaterial
        // Order front to back: fill in front of trail in front of background, a few millimetres apart so they never flicker
        // TODO(Karim)
        private void BuildBar()
        {
        }

        // Unity runs LateUpdate every frame after every Update, so the head and the camera have already moved this frame
        // Placing the bar here means it never lags a frame behind the head
        //   hidden (alpha 0) if this is your own player (_combat.IsOwner), the player is dead, or Head is not found yet
        //   otherwise the target alpha is 1 while Hp is below MaxHp, 0 at full HP, move _alpha toward it over _fadeSeconds
        //   _root above Head by _heightAboveHead, turned to face LocalRig.Instance.Head
        //   fraction = Hp / MaxHp: SetWidth(_fill, fraction), fill colour from FillColor(fraction)
        //   trail: if fraction dropped since last frame, _trailWaitUntil = now + _trailDelay,
        //     after that, _trailFraction moves down toward fraction at _trailSpeed, SetWidth(_trail, _trailFraction)
        //   apply _alpha to all three colours
        // TODO(Karim)
        private void LateUpdate()
        {
        }

        // Scale a cube's width to fraction of _width and shift it so its left edge stays in place
        // TODO(Karim)
        private void SetWidth(Transform bar, float fraction)
        {
        }

        // Green at full, yellow at half, red near empty
        // TODO(Karim)
        private static Color FillColor(float fraction)
        {
            return Color.green;
        }

        // Colour one cube without making a new material: _block.SetColor("_BaseColor", color), renderer.SetPropertyBlock(_block)
        // TODO(Karim)
        private void SetColor(Transform bar, Color color)
        {
        }
    }
}
