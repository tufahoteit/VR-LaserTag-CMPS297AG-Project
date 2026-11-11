// ClusterMoveset.cs
// Owner: Joe
// Layer: Core, pure C#, no Unity
//
// Cluster: close range, 6 charges (set in Cluster.asset)
// Best instant combat: someone is behind the wall next to you, one jump left or one level up and you have an angle
// Worst at crossing the map: one pad at a time
//
// Own column
// Straight up or down by 1 level
//
// Pattern
// The 8 pads right next to you (1 pad away, including diagonals)
// At your level, 1 level up or 1 level down
//
// The start pad is never in the list

using System;
using System.Collections.Generic;

namespace Game.Core
{
    public class ClusterMoveset : IMoveset
    {
        // How many levels up or down Cluster can go in one jump
        public const int MaxLevelChange = 1;

        public MovesetKind Kind
        {
            get { return MovesetKind.Cluster; }
        }

        // For dx from -1 to 1 and dz from -1 to 1 (this includes your own column when both are 0):
        //   for every level from start.level - 1 to start.level + 1, kept inside 0 and levels - 1:
        //     target = (start.x + dx, level, start.z + dz)
        //     skip target if it is the start pad
        //     add target if map.IsPad(target) and not isOccupied(target)
        // We need to make sure the start pad is skipped, the loop passes over it when dx, dz and the level change are all 0
        // TODO(Joe)
        public List<GridCoord> GetReachable(GridCoord start, MapLayout map, Func<GridCoord, bool> isOccupied)
        {
            return new List<GridCoord>();
        }
    }
}
