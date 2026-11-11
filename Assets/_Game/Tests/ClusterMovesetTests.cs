// ClusterMovesetTests.cs
// Owner: Joe
// Layer: Tests, EditMode
//
// What this tests
// Cluster: the 8 pads next to you and your own column, 1 level up or down
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class ClusterMovesetTests
{
    // Cluster (Joe)

    // From (2,L1,2): all 8 pads 1 away on L1 are in the list
    // TODO(Joe) [X] Completed
    [Test]
    public void Cluster_ReachesAll8Neighbours()
    {
        GridCoord start = new GridCoord(2, 1, 2);
        List<GridCoord> list = new ClusterMoveset().GetReachable(start, TestMaps.OpenMap(), TestMaps.Nobody);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx != 0 || dz != 0)
                {
                    Assert.Contains(start.Offset(dx, 0, dz), list);
                }
            }
        }
    }

    // From (2,L1,2): (3,L0,2) and (3,L2,3) are in the list, neighbours 1 level up and down count
    // TODO(Joe) [X] Completed
    [Test]
    public void Cluster_ReachesNeighbours_OneLevelUpAndDown()
    {
        List<GridCoord> list = new ClusterMoveset().GetReachable(new GridCoord(2, 1, 2), TestMaps.OpenMap(), TestMaps.Nobody);
        Assert.Contains(new GridCoord(3, 0, 2), list);
        Assert.Contains(new GridCoord(3, 2, 3), list);
    }

    // From (2,L0,2): (2,L1,2) is in the list, (2,L2,2) is not, own column is only 1 level
    // TODO(Joe) [X] Completed
    [Test]
    public void Cluster_OwnColumn_OneLevelOnly()
    {
        List<GridCoord> list = new ClusterMoveset().GetReachable(new GridCoord(2, 0, 2), TestMaps.OpenMap(), TestMaps.Nobody);
        Assert.Contains(new GridCoord(2, 1, 2), list);
        Assert.IsFalse(list.Contains(new GridCoord(2, 2, 2)));
    }

    // From (2,L1,2): nothing 2 away, e.g. (4,L1,2) is not in the list
    // TODO(Joe) [X] Completed
    [Test]
    public void Cluster_NeverReachesTwoAway()
    {
        List<GridCoord> list = new ClusterMoveset().GetReachable(new GridCoord(2, 1, 2), TestMaps.OpenMap(), TestMaps.Nobody);
        Assert.IsFalse(list.Contains(new GridCoord(4, 1, 2)));
    }

    // From (2,L1,2): the list has exactly 26 pads (3 x 3 x 3 cube minus the start)
    // TODO(Joe) [X] Completed
    [Test]
    public void Cluster_OpenMap_Has26Targets()
    {
        List<GridCoord> list = new ClusterMoveset().GetReachable(new GridCoord(2, 1, 2), TestMaps.OpenMap(), TestMaps.Nobody);
        Assert.AreEqual(26, list.Count);
    }
}
