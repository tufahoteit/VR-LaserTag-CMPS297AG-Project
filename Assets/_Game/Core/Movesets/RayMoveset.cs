// RayMoveset.cs
// Owner: Moustafa
// Layer: Core, pure C#, no Unity
//
// Ray: long range, lines in every direction, 3 charges (set in Ray.asset)
// Best reach on the map, worst instant combat: every sideways move takes you at least 2 pads away
//
// Own column (vertical line)
// Straight up or down to any level in your own column
//
// Pattern (flat lines)
// 8 straight lines on the floor: north, north-east, east, south-east, south, south-west, west, north-west
// Each line starts 2 pads away and goes all the way to the edge of the map
// Lines are flat: same level as the start only
// Never 1 pad away, so Ray can't shuffle next to you, that is Cluster's job
// On a 6x6 map a line is at most 5 pads long, so Ray can cross the whole board in one jump
//
// So a Ray jump is either far and flat, or straight up or down, never both
// To get high somewhere else: jump along a line, then go up in that column (two jumps)
//
// What does not block a line
// Walls and floors never block (teleports ignore them)
// Other players don't block either: an occupied pad is skipped, but the line continues past it
//
// The start pad is never in the list

using System;
using System.Collections.Generic;

namespace Game.Core
{
    public class RayMoveset : IMoveset
    {
        // Flat lines start this many pads away
        public const int MinLineDistance = 2;

        // The 8 flat line directions as steps on the floor (dx = east, dz = north)
        private static readonly (int dx, int dz)[] LineDirections =
        {
            (0, 1), (1, 1), (1, 0), (1, -1),
            (0, -1), (-1, -1), (-1, 0), (-1, 1)
        };

        public MovesetKind Kind
        {
            get { return MovesetKind.Ray; }
        }

        // Own column:
        //   for every level from 0 to levels - 1, except start.level:
        //     target = (start.x, level, start.z)
        //     add target if map.IsPad(target) and not isOccupied(target)
        // Flat lines, for each of the 8 directions:
        //   for k = MinLineDistance and up:
        //     target = (start.x + dx * k, start.level, start.z + dz * k)
        //     if target is outside the map, this line is finished
        //     add target if map.IsPad(target) and not isOccupied(target)
        // We need to make sure an occupied or non-pad cell does not end the line, only the map edge does
        // TODO(Moustafa)
        public List<GridCoord> GetReachable(GridCoord start, MapLayout map, Func<GridCoord, bool> isOccupied)
        {
            return new List<GridCoord>();
        }
    }
}
