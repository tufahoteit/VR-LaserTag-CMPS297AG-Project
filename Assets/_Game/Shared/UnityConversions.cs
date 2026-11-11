// UnityConversions.cs
// Owner: Moustafa
// Layer: Shared
//
// Core is pure C# with no Unity types, so a GridCoord can't be sent over the network
// Netcode can send Vector3Int, so we convert at the border between the two
// Rule: network code (NetworkVariables, RPCs) stores and sends Vector3Int, game logic uses GridCoord
// x = x, y = level, z = z
//
// Not to be confused with MapDefinition
//   UnityConversions: GridCoord <-> Vector3Int, only for sending a cell over the network, no metres
//   MapDefinition: cell <-> position in metres (where things are in the world)

using Game.Core;
using UnityEngine;

namespace Game
{
    public static class UnityConversions
    {
        // "Not on any pad": dead or waiting in the lobby area
        // Negative, so it can never be a real cell (IsInBounds rejects it)
        public static readonly Vector3Int NoPad = new Vector3Int(-1, -1, -1);

        // GridCoord to Vector3Int, used like coord.ToVector3Int()
        // TODO(Moustafa)
        public static Vector3Int ToVector3Int(this GridCoord coord)
        {
            return Vector3Int.zero;
        }

        // Vector3Int to GridCoord, used like value.ToGrid()
        // TODO(Moustafa)
        public static GridCoord ToGrid(this Vector3Int value)
        {
            return default;
        }
    }
}
