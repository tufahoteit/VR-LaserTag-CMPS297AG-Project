// Layers.cs
// Owner: Moustafa
// Layer: Shared
//
// Layer names as constants, so nobody types the strings by hand
// A typo in a string compiles fine and silently breaks shot blocking, a typo in Layers.Cover doesn't compile
//
// Who uses it
//   MapBuilder puts every wall and floor piece on Cover
//   Hitbox puts the head and torso colliders on Hitbox (PlayerCombat calls Hitbox.ApplyLayer when the player spawns)
//   LaserGun raycasts with ShotMask, and checks for a wall between the eyes and the muzzle with CoverMask
//   LocalRig checks with CoverMask whether your head is inside a wall
//
// Your own body
// Only the shooter's headset raycasts, so on your own headset Hitbox.ApplyLayer moves your own head and torso colliders
// to Unity's built-in Ignore Raycast layer: your laser can never hit yourself
// On every other headset your colliders stay on Hitbox, so others can hit you
// The template's rig, hands and controllers stay on Default, which no mask includes, so they never block a shot
//
// Editor setup
//   Project Settings, Tags and Layers: add the layers Cover and Hitbox, with exactly these names

using UnityEngine;

namespace Game
{
    public static class Layers
    {
        public const string Cover = "Cover";    // map walls and floors, they block shots
        public const string Hitbox = "Hitbox";  // player head and torso colliders

        // Layer mask for laser raycasts: Cover + Hitbox only, everything else is ignored (Default too)
        // TODO(Moustafa): LayerMask.GetMask(Cover, Hitbox)
        public static int ShotMask
        {
            get { return 0; }
        }

        // Layer mask for walls only: the check that a gun pushed through a wall can't shoot from the other side
        // TODO(Moustafa): LayerMask.GetMask(Cover)
        public static int CoverMask
        {
            get { return 0; }
        }
    }
}
