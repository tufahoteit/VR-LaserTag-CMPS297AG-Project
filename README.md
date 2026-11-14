# VR Laser Tag: Pad Blink

A multiplayer VR laser tag game for Meta Quest, built in Unity for CMPS297AG at AUB.
Up to 5 players teleport between pads in a three-level arena and tag each other with laser guns. First to 15 tags wins.

Team: Moustafa Hoteit, Joseph Abi Saad, Joe Abi Rizk, Ali M. Sleiman, Karim Ramadan

## Versions

- Unity **6000.6.3f1** with Android Build Support (OpenJDK, Android SDK and NDK)
- Unity VR Multiplayer template **3.0.0**
- Netcode for GameObjects 2.13.2
- Multiplayer Services 2.3.2, Authentication 3.8.0 (Relay)
- XR Interaction Toolkit 3.6.0, XR Hands 1.9.0, XR Core Utils 2.6.0, XR Plug-in Management 4.7.0
- OpenXR 1.18.0
- Universal Render Pipeline 17.6.0, Shader Graph 17.6.0
- Input System 1.20.0
- Multiplayer Play Mode 3.0.0

## Install on a Quest

Download `VRLaserTag_PadBlink.apk` from the [latest release](https://github.com/tufahoteit/VR-LaserTag-CMPS297AG-Project/releases/latest).

Connect the headset to the PC with a USB-C cable, then either:

1. **Build And Run** from Unity (File, Build Profiles, Android)
2. **SideQuest:** sideload the APK, then open it from Library, Unknown Sources

The headset needs internet: rooms run through Unity Relay.

## Host and join

- **Host:** in the lobby menu, create a room. The room code shows on the sign to your right
- **Join:** join by code on the lobby menu, or Quick Join to enter any open room
- Pick a character (Ray, Cluster or Ring), press **Set height** once, and the host presses **Start**

## How to play

| Action | Control |
|---|---|
| Aim a teleport | Hold the left thumbstick forward, markers appear on every pad you can reach |
| Teleport | Release the stick |
| Cancel a teleport | Left grip |
| Up or down a level | While aiming, point at the floating arrows in front of you |
| Fire | Right trigger (one shot per press) |
| Open or close the board | B (map, timer, HP, charges, scoreboard, kill feed) |
| Crouch, lean, dodge | Your real body |

**Characters** (picked in the lobby, applied at the next spawn, 3 teleport charges each, +1 every 4.5 s and +1 per tag):

| Character | Reach |
|---|---|
| Ray | 8 straight and diagonal lines, 2 pads away up to the map edge, same level |
| Cluster | The 8 pads right next to you, one level up or down |
| Ring | Every pad exactly 2 away (16 pads), one level up or down |

Body hits deal 25, headshots 50, out of 100 HP. Health regenerates after 3.5 s without damage.
A match lasts up to 9 minutes; a tie at the end goes to sudden death.

## Build from source

1. Clone the repo and open the folder in Unity Hub with Unity 6000.6.3f1
2. Open `Assets/Scenes/Main.unity`
3. Press Play to test in the editor, the XR Interaction Simulator stands in for a headset
4. To build for Quest: File, Build Profiles, Android, Switch Platform, then Build

Rooms use Unity Relay through our Unity Cloud project. The APK needs nothing. To run from the editor, ask us to add your Unity account to the project, or link the project to your own Unity Cloud project (Edit, Project Settings, Services) and enable Relay. A build from your own project only sees its own rooms.

## Tests

71 EditMode tests cover the rules that run without Unity: movesets, charging, combat, match rules, the map layout and the map validator.
Run them from Window, General, Test Runner, EditMode, Run All.

## Where things are

- `Assets/_Game/Core/` the game rules in plain C# (movesets, charging, combat, match rules, map layout and validator)
- `Assets/_Game/Data/` settings assets: characters, laser gun, match settings, maps
- `Assets/_Game/Features/` the scripts in the scene: arena, movement, combat, match, player, UI, feedback
- `Assets/_Game/Shared/` events, layers, conversions
- `Assets/_Game/Tests/` the EditMode tests
- `Assets/_Game/Audio/`, `Art/`, `Materials/`, `Prefabs/`, `Shaders/` assets
- `Assets/VRMPAssets/` the VR Multiplayer template
- `Tools/` scripts used to generate some of the sounds
- `Licenses/` licences of third-party assets

## Changes to the template

- Networking: client-host over Relay instead of Distributed Authority (`SessionManager`), at most 5 players (`XRINetworkGameManager`, lobby room size), Connection Approval on
- `PlayerOptions`: the host or client panel is decided by the NetworkManager, because a joining player's avatar spawns only after the host approves the join
- Voice chat, demo content and the template's own locomotion turned off (snap turn kept), teleport input slots cleared on the left controller, controller hint callouts off
- Lobby menus moved next to the player's start spot
- OpenXR: empty Android feature entries removed from the package settings

## Credits

- Unity VR Multiplayer template
- Blaster model: Kenney Blaster Kit (CC0)
- Sounds: Kenney Sci-Fi Sounds and Kenney Interface Sounds (CC0), plus sounds generated for this project (`Tools/`)
