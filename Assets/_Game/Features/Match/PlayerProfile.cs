// PlayerProfile.cs
// Owner: Ali
// Layer: Feature, Match (NetworkBehaviour on the player prefab, synced to every headset)
//
// What this is
// Who a player is: name, colour, chosen character and kills
// Every headset can read every player's profile (scoreboard, kill feed, minimap dots, health bar colour)
//
// Who writes what
// The host writes the colour slot, the character and the kills, other headsets only read
// The name is not stored here: the template already syncs it (XRINetworkPlayer.playerName, written by its owner)
// The owner asks for a character change (RequestCharacterRpc), the host decides
// The owner copies its colour into the template (XRINetworkGameManager.LocalPlayerColor), the template then syncs it
// and tints the avatar's shirt and name tag, so nobody picks a colour by hand
//
// Rules
//   name: the template's player name, "P1" to "P5" (colour slot + 1) if it is empty
//   colour: the lowest of the 5 colour slots nobody else is using, freed when a player leaves
//   character: can only change in the lobby area (dead, or before the match starts), it applies at the next spawn
//   kills: start at 0, also for a player who joins mid-match or rejoins after leaving
//
// Template check (done)
//   the name: XRINetworkPlayer.playerName on the same object
//   the colour: XRINetworkGameManager.LocalPlayerColor, the template's own colour picker gets hidden in the cleanup
//
// Editor setup
//   1. Add this component to the template's networked player prefab (XRI_Network_Player_Avatar)
//   2. Drag DefaultMatch into Settings

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer;

namespace Game
{
    public class PlayerProfile : NetworkBehaviour
    {
        // Every player in the match, on every headset
        public static readonly List<PlayerProfile> All = new List<PlayerProfile>();

        // The profile of the player on this headset
        public static PlayerProfile Local { get; private set; }

        [SerializeField] private MatchSettings _settings;

        [Tooltip("One colour per player slot (5), bright so they stand out against the soft-play colours")]
        [SerializeField] private Color[] _palette =
        {
            new Color(0.90f, 0.20f, 0.20f), new Color(0.20f, 0.50f, 1.00f), new Color(0.20f, 0.85f, 0.30f),
            new Color(1.00f, 0.80f, 0.10f), new Color(0.75f, 0.30f, 0.90f),
        };

        // Synced values, only the host writes them
        private readonly NetworkVariable<int> _colorIndex = new NetworkVariable<int>(0);
        private readonly NetworkVariable<int> _characterIndex = new NetworkVariable<int>(0);
        private readonly NetworkVariable<int> _kills = new NetworkVariable<int>(0);

        // Read access for everyone, use .Value to read, .OnValueChanged to react to a change
        public NetworkVariable<int> ColorIndex { get { return _colorIndex; } }
        public NetworkVariable<int> CharacterIndex { get { return _characterIndex; } }
        public NetworkVariable<int> Kills { get { return _kills; } }

        // The template's player script on this object, found in OnNetworkSpawn
        private XRINetworkPlayer _templatePlayer;

        // This player's name, on every headset
        // TODO(Ali): _templatePlayer.playerName, or "P" + (ColorIndex + 1) if it is empty
        public string DisplayName
        {
            get { return ""; }
        }

        // This player's colour
        // TODO(Ali): _palette[ColorIndex], clamped to the palette size
        public Color Color
        {
            get { return Color.white; }
        }

        // This player's chosen character
        // TODO(Ali): _settings.Characters[CharacterIndex], null if the index is out of range
        public CharacterDefinition Character
        {
            get { return null; }
        }

        // Find _templatePlayer (GetComponent<XRINetworkPlayer>), add to All, set Local if IsOwner
        // Host only: ColorIndex = the lowest slot no other player in All is using
        // Owner only: ApplyColorToTemplate now and whenever ColorIndex changes (the host's value may arrive a frame later)
        // TODO(Ali)
        public override void OnNetworkSpawn()
        {
        }

        // Remove from All, clear Local if it was this one, unsubscribe
        // TODO(Ali)
        public override void OnNetworkDespawn()
        {
        }

        // Owner only: XRINetworkGameManager.LocalPlayerColor.Value = Color
        // The template sees the change, writes its own synced colour and tints the shirt and name tag on every headset
        // TODO(Ali)
        private void ApplyColorToTemplate()
        {
        }

        // A player's profile by client id, or null
        // TODO(Ali)
        public static PlayerProfile Get(ulong clientId)
        {
            return null;
        }

        // Host only: +1 kill, called by PlayerCombat when this player gets a kill
        // TODO(Ali)
        public void AddKill()
        {
        }

        // Host only: kills back to 0, called by MatchManager when a new match starts
        // TODO(Ali)
        public void ResetKills()
        {
        }

        // The owner asks for a character, the host decides
        // Accept only if the index is a real character AND the player is in the lobby area
        // (dead, or the match is in Lobby or Countdown), then set CharacterIndex
        // It applies at the next spawn: PlayerMovement reads Character when the player spawns
        // InvokePermission Owner: only this player's own headset can pick this player's character
        // TODO(Ali)
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void RequestCharacterRpc(int index)
        {
        }
    }
}
