// RingMoveset.cs
// Owner: Karim
// Layer: Core, pure C#, no Unity
//
// Ring: mid range, charges set in Ring.asset (around 4)
// Flanker: always pops up at a new angle, but can't make small moves next to you
//
// Own column
// Straight up or down by 1 level
//
// Pattern
// Every pad exactly 2 pads away, in every direction (a square ring of 16 pads around you)
// At your level, 1 level up or 1 level down
// Never 1 pad away, that is Cluster's job
// 8 of the 16 ring pads are off the straight lines, only Ring can reach those
//
// The start pad is never in the list

using System;
using System.Collections.Generic;

namespace Game.Core
{
    public class RingMoveset : IMoveset
    {
        // The ring is exactly this many pads away
        public const int RingDistance = 2;

        // How many levels up or down Ring can go in one jump
        public const int MaxLevelChange = 1;

        public MovesetKind Kind
        {
            get { return MovesetKind.Ring; }
        }

        // Own column:
        //   for level start.level - 1 and start.level + 1, kept inside 0 and levels - 1:
        //     target = (start.x, level, start.z)
        //     add target if map.IsPad(target) and not isOccupied(target)
        // Ring:
        //   for dx from -2 to 2 and dz from -2 to 2:
        //     skip unless GridCoord.PadDistance is exactly 2 (the outer edge of the 5x5 square)
        //     for every level from start.level - 1 to start.level + 1, kept inside 0 and levels - 1:
        //       target = (start.x + dx, level, start.z + dz)
        //       add target if map.IsPad(target) and not isOccupied(target)
        // We need to make sure the 8 pads 1 away are never added, only the outer edge of the square counts
        // TODO(Karim)
        public List<GridCoord> GetReachable(GridCoord start, MapLayout map, Func<GridCoord, bool> isOccupied)
        {
            return new List<GridCoord>();
        }
    }
}
