// IMoveset.cs
// Owner: Moustafa
// Layer: Core, pure C#, no Unity
//
// The contract every character's movement follows
// An interface only says what a moveset must be able to do, it has no logic itself
// The logic lives in RayMoveset, ClusterMoveset and RingMoveset
//
// IMoveset only answers one question: from this pad, which pads can I reach
// It never moves anyone
//
// Why this matters
// Two different places ask this question
//   TeleportController (on your headset) uses the answer to show the ghost markers
//   PlayerMovement (on the host) uses the answer to approve or reject a teleport
// Both must call it with the same inputs and get the same answer
// If they disagree, you see a marker the host refuses, and you get snapped back
// So any movement rule goes inside the moveset, never added on only one side
//
// Every moveset has two parts
//   Own column: moving straight up or down from where you stand
//   Pattern: where else on the floor you can go
//
//            own column    1 away        2 away              further
//   Cluster  +-1 level     +-1 level     no                  no
//   Ring     +-1 level     no            +-1 level           no
//   Ray      any level     no            lines, same level   lines to the edge, same level
//
// Rules every moveset follows
// Only pads are valid targets (map.IsPad)
// Occupied pads are never valid (isOccupied)
// Walls and floors never block teleports, only the pattern, occupied pads and the map edge do
// Distance is counted in pads, a diagonal step counts as 1 pad, same as a straight step (GridCoord.PadDistance)
// The pad you are standing on can't be in the list as a place to teleport to
// The result must be the same on every headset for the same inputs (no randomness)
//
// Movesets work on cells, not on exact pad positions inside a cell (anchors don't matter here)

using System;
using System.Collections.Generic;

namespace Game.Core
{
    // The three movement patterns
    public enum MovesetKind
    {
        Cluster,  // close range: own column and the 8 pads right next to you, +-1 level
        Ring,     // mid range: own column and every pad exactly 2 away, +-1 level
        Ray       // long range: own column any level, 8 flat lines from 2 pads to the edge
    }

    public interface IMoveset
    {
        // Which pattern this is
        MovesetKind Kind { get; }

        // Every pad this moveset can teleport to from the start pad
        // isOccupied(coord) returns true if another living player stands on that pad
        // (a Func is a function passed in as a value, here: "ask me if a pad is taken")
        List<GridCoord> GetReachable(GridCoord start, MapLayout map, Func<GridCoord, bool> isOccupied);
    }

    // Creates the right moveset for a character
    public static class MovesetFactory
    {
        // Returns new ClusterMoveset(), new RingMoveset() or new RayMoveset() for the kind
        // No settings: the patterns are fixed, only charges live in the character data
        // We need to make sure every MovesetKind returns its matching class, an unknown kind throws an error
        // TODO(Moustafa)
        public static IMoveset Create(MovesetKind kind)
        {
            return null;
        }
    }
}
