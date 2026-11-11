# VR Laser Tag

A multiplayer VR laser tag game for Meta Quest, built in Unity for CMPS297AG at AUB.
Up to 5 players teleport between pads in a multi-level arena and tag each other with laser guns.

Team: Moustafa (lead), Karim, Joseph, Joe, Ali

## Versions

- Unity 6000.6.3f1 (everyone on the team uses exactly this version)
- VR Multiplayer template 3.0.0
- Netcode for GameObjects 2.13.2, XR Interaction Toolkit 3.6.0, OpenXR 1.18.0, Multiplayer Services 2.3.2

## Getting started

1. Clone the repo and open the folder in Unity Hub with Unity 6000.6.3f1 (with Android Build Support)
2. Ask Moustafa to add you to the Unity Cloud project, it is needed for Relay
3. Open the main scene and press Play, the XR Interaction Simulator lets you test without a headset

## Where things are

- `Assets/_Game/` our game: Core (rules, pure C#), Data (settings assets), Features (networked and local scripts), Shared, Tests, Audio
- `Assets/VRMPAssets/` the template (three small changes, see Docs)
- `Docs/` design documents and setup screenshots
- `Tools/` scripts used to generate some of the sounds
- `Licenses/` licences of third-party assets

The full README (how to build, how to play, architecture) comes at the end of the project.
