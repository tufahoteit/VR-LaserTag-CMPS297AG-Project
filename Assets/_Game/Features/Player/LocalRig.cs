// LocalRig.cs
// Owner: Joe
// Layer: Feature, Player (local only, on your own XR rig, nothing networked)
//
// What this is
// Your own VR body on your own headset: one place for your head, your hands, and moving you
// Other players never see this object, they see your networked avatar
//
// What it does
//   finds your head and both controllers, so other scripts ask LocalRig instead of searching
//   TeleportTo: moves you instantly onto a pad (TeleportController calls it)
//   FaceTowards: turns you toward the arena centre, on spawn only
//   eye height: puts every player's eyes at 1.65 m, so tall and short players see and are seen the same
//   head in a wall: fades your view to black if your head goes inside a wall, so nobody can peek through geometry
//   leash: fades your view when your head drifts more than 0.6 m from your pad centre
//
// Moving you
// There is no walking: the thumbstick never moves you, only teleports do
// We move the XR Origin directly (no XRI teleport provider, it is turned off in the template cleanup)
// Your head ends up above the pad's centre, your real position inside your play area does not change
// A teleport only moves you, it never turns you: you keep facing wherever you were facing when you let go
//
// Your real body
// The headset tracks your real head, so the game can't tell a lean from a step, both are "the head moved"
// And the game can't freeze your head: if the view ignored your real movement, the world would slide with you
// and players get motion sick within seconds, so the camera always follows your real head
// So instead of stopping you, two fades make going too far useless:
//   within 0.6 m of the pad centre: counted as leaning or crouching, works normally (this is how you peek)
//   beyond 0.6 m (you stepped in your room): the view fades, fully black at 0.8 m, step back and it clears
//   head inside a wall: the view goes black, so you never see behind cover
// Walls can't block your head in VR, collision only stops movement the game itself does
//
// Eye height
// Every player's eyes end up at 1.65 m, as if only their legs were made longer or shorter
// Done by moving the Camera Offset, which carries the head and both hands together, nothing is scaled
// From the waist up everything stays real: arm reach, leaning, crouching (crouch 50 cm, your eyes go down 50 cm)
// The avatar has no legs, so nobody sees the difference
// Measured once when you stand up straight (a button in the lobby area), then kept
//
// Template check (done)
//   XR Interaction Setup (MP Variant) is only a container, the XROrigin component is on its child XR Origin (XR Rig)
//   so this script finds it with GetComponentInChildren, and a teleport moves XR Origin (XR Rig), the object that carries
//   the camera, the controllers and the board
//   the controllers: Left Controller and Right Controller under Camera Offset
//   the template's TunnelingVignette is a comfort vignette for moving, not a full black fade, so we use our own quad
//
// Editor setup
//   1. Add this component to the XR Interaction Setup (MP Variant) object
//   2. Create a black quad as a child of the Main Camera, 0.2 m in front, with a transparent black material, drag it into Fade Renderer
//   3. Drag DefaultMatch into Settings, everything else is found automatically

using Unity.XR.CoreUtils;
using UnityEngine;

namespace Game
{
    public class LocalRig : MonoBehaviour
    {
        [SerializeField] private MatchSettings _settings;

        [Header("Fade")]
        [Tooltip("Black quad in front of the camera, its alpha is set here")]
        [SerializeField] private Renderer _fadeRenderer;
        [SerializeField, Range(0.05f, 0.3f)] private float _headRadius = 0.12f;
        [SerializeField, Range(1f, 20f)] private float _fadeSpeed = 6f;
        [Tooltip("The leash fade starts at the leash radius (0.6 m) and is fully black this many metres further (0.8 m)")]
        [SerializeField, Range(0.05f, 0.5f)] private float _leashFadeWidth = 0.2f;

        private XROrigin _origin;
        private Transform _head;
        private Transform _leftController;
        private Transform _rightController;
        private Transform _cameraOffset;
        private Vector3 _padCenter;

        public static LocalRig Instance { get; private set; }

        public Transform Head { get { return _head; } }
        public Transform LeftController { get { return _leftController; } }
        public Transform RightController { get { return _rightController; } }

        // Instance = this
        // Find everything: _origin = GetComponentInChildren<XROrigin>(), head = origin.Camera, cameraOffset = origin.CameraFloorOffsetObject,
        // the two controllers by name under the camera offset ("Left Controller", "Right Controller")
        // We need to make sure a missing object logs an error with its name, so a template rename is easy to spot
        // TODO(Joe)
        private void Awake()
        {
        }

        // Moves you instantly onto a pad
        // Shift _origin.transform (XR Origin (XR Rig)) so your head (on the floor, ignoring height) ends up above feetPosition
        // Remember feetPosition as the new pad centre for the leash
        // Never rotates the rig: you keep facing the same way
        // TODO(Joe)
        public void TeleportTo(Vector3 feetPosition)
        {
        }

        // Spawn only: turns _origin.transform around your head so you face point (on the floor, ignoring height)
        // Never on normal teleports: turning a player mid-fight makes them lose track of enemies
        // A single instant turn when you arrive from the lobby area is comfortable, a turn you watch happen is not
        // TODO(Joe)
        public void FaceTowards(Vector3 point)
        {
        }

        // Stand up straight and press the button in the lobby area
        // Moves the Camera Offset so your eyes are at StandardEyeHeight (1.65 m)
        // offset y = 1.65 - your real eye height above the floor right now
        // TODO(Joe)
        public void CalibrateHeight()
        {
        }

        // True if a small sphere at your head overlaps a wall (Layers.CoverMask)
        // TODO(Joe)
        public bool IsHeadInsideGeometry()
        {
            return false;
        }

        // How far your head is from the pad centre, on the floor (ignoring height)
        // TODO(Joe)
        public float LeashDistance()
        {
            return 0f;
        }

        // Every frame: the fade alpha moves toward the darkest of:
        //   1 if the head is inside a wall
        //   0 inside the leash, rising to 1 between LeashRadius and LeashRadius + leashFadeWidth
        // Smoothly, at fadeSpeed
        // TODO(Joe)
        private void Update()
        {
        }
    }
}
