// PlayerHUD.cs
// Owner: Ali
// Layer: Feature, UI (local only, on your own XR rig, view only)
//
// What this is
// Everything you read about yourself and the match: the board and the hurt edges
// There is no head-locked HUD, the board lives in the world and the hurt edges are an effect at the edges of your view
// It only reads synced values and events, it never changes the game
//
// Two parts
//   board: press B to open, press B again to close (a toggle, you don't have to hold it)
//     it opens where you are looking, 0.7 m away and a little below eye height, turned toward you like a tablet
//     it is attached to your rig, not your head: turn your head and it stays put, teleport and it comes with you
//     semi-transparent, so movement behind it stays visible, and it has no collider, so it never blocks a shot
//     top: the minimap, one floor plan per level side by side (Minimap.cs)
//     middle: timer, your kills, your character, HP as a bar and a number, teleport charges as lights and "2/3"
//     bottom: every player's kills (scoreboard) and the last kills (kill feed)
//   hurt edges: red at the edges of your view when you get hit, and a steady red while your HP is low
//     this is the HP you see mid-fight without opening the board
//     the only thing drawn on your view, and it is an effect, not information to read
// Elsewhere on purpose: laser energy is the strip on the gun, enemy HP is the bar over their heads
// The template's hand menu (room code, leave, settings) stays as it is, the board is separate
//
// What each part reads
//   timer: MatchManager.Phase, SecondsLeft(), IsSuddenDeath(), Winner
//   kills, character: PlayerProfile.Local
//   HP: PlayerCombat.Local.Hp / MaxHp
//   charges: PlayerMovement.Local.Charges minus TeleportController.PendingRequests,
//     so a light goes off the moment you teleport, the same instant you move
//     the next light fills with (server time - RechargeStartTime) / RechargeSeconds
//   scoreboard: PlayerProfile.All (a player who left is simply no longer in it), kill feed: GameEvents.PlayerKilled
//
// Template check (done)
//   the board goes under XR Origin (XR Rig), the object LocalRig moves on a teleport
//   the B button: the template's input actions have no action for it, so _boardAction carries its own binding
//   TextMeshPro is in the project (it comes with Unity's UI package)
//
// Editor setup
//   1. Under XR Origin (XR Rig) (not under the camera): a World Space Canvas named Board, scale 0.001, size 400 x 500 (40 x 50 cm),
//      add a CanvasGroup and a semi-transparent dark background Image, no Tracked Device Graphic Raycaster (nothing on it is clickable)
//   2. Children of Board, top to bottom:
//      MinimapRoot (an empty object, see Minimap)
//      TimerText, KillsText, CharacterText, HpText, ChargesText (TextMeshPro)
//      HpBar (a background Image and a fill Image, Image Type Filled, Horizontal)
//      ChargeLights (an empty RectTransform with a Horizontal Layout Group)
//      ScoreboardText and KillFeedText (TextMeshPro)
//   3. Create a quad as a child of the Main Camera, 0.15 m in front, with a transparent unlit material, name it HurtEdges
//   4. Add the PlayerHUD component to the XR Interaction Setup (MP Variant) object, drag everything into its slots, DefaultMatch into Settings
//   The sizes are tuned in the headset

using System.Collections.Generic;
using Game.Core;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game
{
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private MatchSettings _settings;

        [Header("Board (press B)")]
        [SerializeField] private CanvasGroup _boardGroup;
        [SerializeField] private Transform _board;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _killsText;
        [SerializeField] private TMP_Text _characterText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private Image _hpFill;
        [SerializeField] private TMP_Text _chargesText;
        [SerializeField] private RectTransform _chargeLights;
        [SerializeField] private TMP_Text _scoreboardText;
        [SerializeField] private TMP_Text _killFeedText;

        [Tooltip("The B button on the right controller, already bound, nothing to drag")]
        [SerializeField] private InputActionProperty _boardAction = new InputActionProperty(
            new InputAction("Board", InputActionType.Button, "<XRController>{RightHand}/secondaryButton"));

        [Header("Hurt edges")]
        [SerializeField] private Renderer _hurtEdges;

        // Where the board opens: in front of you, a little low, see-through
        private const float BoardDistance = 0.7f;
        private const float BoardDrop = 0.15f;
        private const float BoardAlpha = 0.85f;

        private const int KillFeedLines = 5;
        private const float HurtFlashSeconds = 0.4f;

        private readonly List<Image> _lights = new List<Image>();
        private readonly Queue<string> _killFeed = new Queue<string>();
        private MaterialPropertyBlock _block;
        private bool _boardOpen;
        private float _hurtFlash;

        // HideBoard, hide the hurt edges, give the HurtEdges quad its texture (CreateEdgeTexture)
        // TODO(Ali)
        private void Awake()
        {
        }

        // Subscribe to GameEvents.PlayerKilled and PlayerDamaged, enable _boardAction
        // TODO(Ali)
        private void OnEnable()
        {
        }

        // Unsubscribe, disable _boardAction
        // TODO(Ali)
        private void OnDisable()
        {
        }

        // Every frame:
        //   not connected yet (any Local is null): HideBoard and stop here
        //   B pressed this frame (WasPressedThisFrame): ShowBoard if closed, HideBoard if open
        //   board open: UpdateTimer, UpdateStats, UpdateChargeLights, UpdateScoreboard
        //   always: UpdateHurtEdges
        // TODO(Ali)
        private void Update()
        {
        }

        // Board

        // Opens the board where you are looking
        //   position = your head + BoardDistance forward on the floor (ignoring height), BoardDrop below eye height
        //   rotation: turned to face your head, so it tilts up toward you
        //   _boardGroup.alpha = BoardAlpha, _boardOpen = true
        // The board is a child of the rig, so after this it moves with teleports and not with your head
        // TODO(Ali)
        private void ShowBoard()
        {
        }

        // _boardGroup.alpha = 0, _boardOpen = false
        // TODO(Ali)
        private void HideBoard()
        {
        }

        // Lobby: "Lobby", Countdown: "Starting in 3", Playing: "8:42" (minutes:seconds),
        // Playing past the time limit: "Sudden death", Ended: "You won" or "Winner: P3"
        // TODO(Ali)
        private void UpdateTimer()
        {
        }

        // Kills: PlayerProfile.Local.Kills, character: PlayerProfile.Local.Character.DisplayName,
        // HP: _hpText = "HP 74 / 100" (rounded), _hpFill.fillAmount = Hp / MaxHp
        // TODO(Ali)
        private void UpdateStats()
        {
        }

        // One light per charge of your character, BuildChargeLights again if MaxCharges changed (a new character)
        //   available = Charges - TeleportController.Instance.PendingRequests, never below 0
        //   lights below available: lit, the next one: filling with the recharge progress, the rest: dim
        //   progress = (server time - RechargeStartTime) / RechargeSeconds, kept between 0 and 1
        //   _chargesText = "available/MaxCharges", e.g. "2/3"
        // TODO(Ali)
        private void UpdateChargeLights()
        {
        }

        // Make count small Image squares under _chargeLights (the layout group lines them up), remove the old ones
        // TODO(Ali)
        private void BuildChargeLights(int count)
        {
        }

        // One line per player from PlayerProfile.All, most kills first, the name in the player's colour, yours in bold
        // e.g. "<color=#E63333>P1</color>   7"
        // The kill feed: the lines in _killFeed, newest first
        // TODO(Ali)
        private void UpdateScoreboard()
        {
        }

        // Listener for GameEvents.PlayerKilled (runs by itself on each kill, connected in OnEnable, not an RPC)
        // Adds "P3 tagged P1" (names in their colours, PlayerProfile.DisplayName) to the kill feed, keep the last KillFeedLines
        // TODO(Ali)
        private void OnPlayerKilled(ulong victim, ulong killer)
        {
        }

        // Hurt edges

        // Listener for GameEvents.PlayerDamaged (runs by itself on each counted hit, connected in OnEnable)
        // If you are the victim: _hurtFlash = 1
        // TODO(Ali)
        private void OnPlayerDamaged(ulong victim, ulong attacker, float newHp, bool headshot)
        {
        }

        // Edge alpha = the stronger of:
        //   the flash: 0.6 right after a hit, fading to 0 over HurtFlashSeconds
        //   low HP: 0 above half HP, rising to 0.4 at 0 HP
        // Set it on the HurtEdges colour with a MaterialPropertyBlock (_BaseColor)
        // TODO(Ali)
        private void UpdateHurtEdges()
        {
        }

        // A small texture made in code: clear in the middle, red toward the edges (a radial gradient, 64 x 64)
        // So no image file is needed
        // TODO(Ali)
        private static Texture2D CreateEdgeTexture()
        {
            return null;
        }
    }
}
