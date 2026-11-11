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
        private Texture2D _edgeTexture;

        private static readonly Color LitColor = new Color(0.3f, 0.9f, 1f, 1f);
        private static readonly Color DimColor = new Color(1f, 1f, 1f, 0.15f);

        // HideBoard, hide the hurt edges, give the HurtEdges quad its texture (CreateEdgeTexture)
        // TODO(Ali) [X] Completed
        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            HideBoard();
            if (_hurtEdges != null)
            {
                _edgeTexture = CreateEdgeTexture();
                _hurtEdges.GetPropertyBlock(_block);
                _block.SetTexture("_BaseMap", _edgeTexture);
                _block.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0f));
                _hurtEdges.SetPropertyBlock(_block);
                _hurtEdges.enabled = false;
            }
        }

        // Subscribe to GameEvents.PlayerKilled and PlayerDamaged, enable _boardAction
        // TODO(Ali) [X] Completed
        private void OnEnable()
        {
            GameEvents.PlayerKilled += OnPlayerKilled;
            GameEvents.PlayerDamaged += OnPlayerDamaged;
            if (_boardAction.action != null)
            {
                _boardAction.action.Enable();
            }
        }

        // Unsubscribe, disable _boardAction
        // TODO(Ali) [X] Completed
        private void OnDisable()
        {
            GameEvents.PlayerKilled -= OnPlayerKilled;
            GameEvents.PlayerDamaged -= OnPlayerDamaged;
            if (_boardAction.action != null)
            {
                _boardAction.action.Disable();
            }
        }

        // Every frame:
        //   not connected yet (any Local is null): HideBoard and stop here
        //   B pressed this frame (WasPressedThisFrame): ShowBoard if closed, HideBoard if open
        //   board open: UpdateTimer, UpdateStats, UpdateChargeLights, UpdateScoreboard
        //   always: UpdateHurtEdges
        // TODO(Ali) [X] Completed
        private void Update()
        {
            if (PlayerProfile.Local == null || PlayerCombat.Local == null || PlayerMovement.Local == null || LocalRig.Instance == null)
            {
                if (_boardOpen)
                {
                    HideBoard();
                }
                return;
            }

            if (_boardAction.action != null && _boardAction.action.WasPressedThisFrame())
            {
                if (_boardOpen)
                {
                    HideBoard();
                }
                else
                {
                    ShowBoard();
                }
            }

            if (_boardOpen)
            {
                UpdateTimer();
                UpdateStats();
                UpdateChargeLights();
                UpdateScoreboard();
            }
            UpdateHurtEdges();
        }

        // Board

        // Opens the board where you are looking
        //   position = your head + BoardDistance forward on the floor (ignoring height), BoardDrop below eye height
        //   rotation: turned to face your head, so it tilts up toward you
        //   _boardGroup.alpha = BoardAlpha, _boardOpen = true
        // The board is a child of the rig, so after this it moves with teleports and not with your head
        // TODO(Ali) [X] Completed
        private void ShowBoard()
        {
            Transform head = LocalRig.Instance.Head;
            if (head == null || _board == null)
            {
                return;
            }
            Vector3 forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            forward.Normalize();

            _board.position = head.position + forward * BoardDistance + Vector3.down * BoardDrop;
            // A world space canvas reads correctly when its forward points away from the viewer
            _board.rotation = Quaternion.LookRotation(_board.position - head.position);
            _boardGroup.alpha = BoardAlpha;
            _boardOpen = true;
        }

        // _boardGroup.alpha = 0, _boardOpen = false
        // TODO(Ali) [X] Completed
        private void HideBoard()
        {
            if (_boardGroup != null)
            {
                _boardGroup.alpha = 0f;
            }
            _boardOpen = false;
        }

        // Lobby: "Lobby", Countdown: "Starting in 3", Playing: "8:42" (minutes:seconds),
        // Playing past the time limit: "Sudden death", Ended: "You won" or "Winner: P3"
        // TODO(Ali) [X] Completed
        private void UpdateTimer()
        {
            if (_timerText == null)
            {
                return;
            }
            MatchManager match = MatchManager.Instance;
            if (match == null)
            {
                _timerText.text = "";
                return;
            }

            switch (match.Phase.Value)
            {
                case MatchPhase.Lobby:
                    _timerText.text = "Lobby";
                    break;
                case MatchPhase.Countdown:
                    _timerText.text = "Starting in " + Mathf.CeilToInt(match.SecondsLeft());
                    break;
                case MatchPhase.Playing:
                    if (match.IsSuddenDeath())
                    {
                        _timerText.text = "Sudden death";
                    }
                    else
                    {
                        int seconds = Mathf.CeilToInt(match.SecondsLeft());
                        _timerText.text = $"{seconds / 60}:{seconds % 60:00}";
                    }
                    break;
                case MatchPhase.Ended:
                    ulong winner = match.Winner.Value;
                    if (winner == PlayerProfile.Local.OwnerClientId)
                    {
                        _timerText.text = "You won";
                    }
                    else
                    {
                        PlayerProfile winnerProfile = PlayerProfile.Get(winner);
                        _timerText.text = "Winner: " + (winnerProfile != null ? winnerProfile.DisplayName : "?");
                    }
                    break;
            }
        }

        // Kills: PlayerProfile.Local.Kills, character: PlayerProfile.Local.Character.DisplayName,
        // HP: _hpText = "HP 74 / 100" (rounded), _hpFill.fillAmount = Hp / MaxHp
        // TODO(Ali) [X] Completed
        private void UpdateStats()
        {
            PlayerProfile profile = PlayerProfile.Local;
            PlayerCombat combat = PlayerCombat.Local;
            if (_killsText != null)
            {
                _killsText.text = "Kills " + profile.Kills.Value;
            }
            if (_characterText != null)
            {
                _characterText.text = profile.Character != null ? profile.Character.DisplayName : "";
            }
            float maxHp = _settings.MaxHp;
            float hp = combat.Hp.Value;
            if (_hpText != null)
            {
                _hpText.text = $"HP {Mathf.RoundToInt(hp)} / {Mathf.RoundToInt(maxHp)}";
            }
            if (_hpFill != null)
            {
                _hpFill.fillAmount = maxHp > 0f ? hp / maxHp : 0f;
            }
        }

        // One light per charge of your character, BuildChargeLights again if MaxCharges changed (a new character)
        //   available = Charges - TeleportController.Instance.PendingRequests, never below 0
        //   lights below available: lit, the next one: filling with the recharge progress, the rest: dim
        //   progress = (server time - RechargeStartTime) / RechargeSeconds, kept between 0 and 1
        //   _chargesText = "available/MaxCharges", e.g. "2/3"
        // TODO(Ali) [X] Completed
        private void UpdateChargeLights()
        {
            CharacterDefinition character = PlayerProfile.Local.Character;
            int max = character != null ? character.MaxCharges : 0;
            if (_lights.Count != max)
            {
                BuildChargeLights(max);
            }

            PlayerMovement player = PlayerMovement.Local;
            int pending = TeleportController.Instance != null ? TeleportController.Instance.PendingRequests : 0;
            int available = Mathf.Max(0, player.Charges.Value - pending);

            float progress = 1f;
            if (character != null && character.RechargeSeconds > 0f && NetworkManager.Singleton != null)
            {
                double elapsed = NetworkManager.Singleton.ServerTime.Time - player.RechargeStartTime.Value;
                progress = Mathf.Clamp01((float)(elapsed / character.RechargeSeconds));
            }

            for (int i = 0; i < _lights.Count; i++)
            {
                if (i < available)
                {
                    _lights[i].color = LitColor;
                }
                else if (i == available)
                {
                    _lights[i].color = Color.Lerp(DimColor, LitColor, progress * 0.6f);
                }
                else
                {
                    _lights[i].color = DimColor;
                }
            }
            if (_chargesText != null)
            {
                _chargesText.text = $"{available}/{max}";
            }
        }

        // Make count small Image squares under _chargeLights (the layout group lines them up), remove the old ones
        // TODO(Ali) [X] Completed
        private void BuildChargeLights(int count)
        {
            foreach (Image light in _lights)
            {
                if (light != null)
                {
                    Destroy(light.gameObject);
                }
            }
            _lights.Clear();
            if (_chargeLights == null)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                GameObject light = new GameObject("Charge " + (i + 1), typeof(RectTransform), typeof(Image));
                light.transform.SetParent(_chargeLights, false);
                ((RectTransform)light.transform).sizeDelta = new Vector2(24f, 24f);
                _lights.Add(light.GetComponent<Image>());
            }
        }

        // One line per player from PlayerProfile.All, most kills first, the name in the player's colour, yours in bold
        // e.g. "<color=#E63333>P1</color>   7"
        // The kill feed: the lines in _killFeed, newest first
        // TODO(Ali) [X] Completed
        private void UpdateScoreboard()
        {
            if (_scoreboardText != null)
            {
                List<PlayerProfile> players = new List<PlayerProfile>(PlayerProfile.All);
                players.Sort((a, b) => b.Kills.Value.CompareTo(a.Kills.Value));
                System.Text.StringBuilder board = new System.Text.StringBuilder();
                foreach (PlayerProfile player in players)
                {
                    string line = $"{Colored(player)}   {player.Kills.Value}";
                    if (player == PlayerProfile.Local)
                    {
                        line = "<b>" + line + "</b>";
                    }
                    board.AppendLine(line);
                }
                _scoreboardText.text = board.ToString();
            }

            if (_killFeedText != null)
            {
                List<string> lines = new List<string>(_killFeed);
                lines.Reverse();
                _killFeedText.text = string.Join("\n", lines);
            }
        }

        // A player's name in their colour, for the scoreboard and the kill feed
        private static string Colored(PlayerProfile player)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(player.Color)}>{player.DisplayName}</color>";
        }

        // Listener for GameEvents.PlayerKilled (runs by itself on each kill, connected in OnEnable, not an RPC)
        // Adds "P3 tagged P1" (names in their colours, PlayerProfile.DisplayName) to the kill feed, keep the last KillFeedLines
        // TODO(Ali) [X] Completed
        private void OnPlayerKilled(ulong victim, ulong killer)
        {
            PlayerProfile victimProfile = PlayerProfile.Get(victim);
            PlayerProfile killerProfile = PlayerProfile.Get(killer);
            if (victimProfile == null)
            {
                return;
            }
            string line = killerProfile != null
                ? $"{Colored(killerProfile)} tagged {Colored(victimProfile)}"
                : $"{Colored(victimProfile)} was tagged";
            _killFeed.Enqueue(line);
            while (_killFeed.Count > KillFeedLines)
            {
                _killFeed.Dequeue();
            }
        }

        // Hurt edges

        // Listener for GameEvents.PlayerDamaged (runs by itself on each counted hit, connected in OnEnable)
        // If you are the victim: _hurtFlash = 1
        // TODO(Ali) [X] Completed
        private void OnPlayerDamaged(ulong victim, ulong attacker, float newHp, bool headshot)
        {
            if (PlayerCombat.Local != null && victim == PlayerCombat.Local.OwnerClientId)
            {
                _hurtFlash = 1f;
            }
        }

        // Edge alpha = the stronger of:
        //   the flash: 0.6 right after a hit, fading to 0 over HurtFlashSeconds
        //   low HP: 0 above half HP, rising to 0.4 at 0 HP
        // Set it on the HurtEdges colour with a MaterialPropertyBlock (_BaseColor)
        // TODO(Ali) [X] Completed
        private void UpdateHurtEdges()
        {
            if (_hurtEdges == null)
            {
                return;
            }
            float flash = 0.6f * _hurtFlash;
            _hurtFlash = Mathf.Max(0f, _hurtFlash - Time.deltaTime / HurtFlashSeconds);

            float lowHp = 0f;
            PlayerCombat combat = PlayerCombat.Local;
            if (combat != null && combat.IsAlive.Value && _settings.MaxHp > 0f)
            {
                float fraction = combat.Hp.Value / _settings.MaxHp;
                if (fraction < 0.5f)
                {
                    lowHp = (0.5f - fraction) / 0.5f * 0.4f;
                }
            }

            float alpha = Mathf.Max(flash, lowHp);
            _hurtEdges.enabled = alpha > 0.001f;
            _hurtEdges.GetPropertyBlock(_block);
            _block.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));
            _hurtEdges.SetPropertyBlock(_block);
        }

        // A small texture made in code: clear in the middle, red toward the edges (a radial gradient, 64 x 64)
        // So no image file is needed
        // TODO(Ali) [X] Completed
        private static Texture2D CreateEdgeTexture()
        {
            const int Size = 64;
            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float dx = (x + 0.5f) / Size * 2f - 1f;
                    float dy = (y + 0.5f) / Size * 2f - 1f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((distance - 0.55f) / 0.45f);
                    texture.SetPixel(x, y, new Color(1f, 0f, 0f, alpha));
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
