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
    // TODO(Joe)
    [Test]
    public void Cluster_ReachesAll8Neighbours()
    {
        Assert.Inconclusive("TODO(Joe)");
    }

    // From (2,L1,2): (3,L0,2) and (3,L2,3) are in the list, neighbours 1 level up and down count
    // TODO(Joe)
    [Test]
    public void Cluster_ReachesNeighbours_OneLevelUpAndDown()
    {
        Assert.Inconclusive("TODO(Joe)");
    }

    // From (2,L0,2): (2,L1,2) is in the list, (2,L2,2) is not, own column is only 1 level
    // TODO(Joe)
    [Test]
    public void Cluster_OwnColumn_OneLevelOnly()
    {
        Assert.Inconclusive("TODO(Joe)");
    }

    // From (2,L1,2): nothing 2 away, e.g. (4,L1,2) is not in the list
    // TODO(Joe)
    [Test]
    public void Cluster_NeverReachesTwoAway()
    {
        Assert.Inconclusive("TODO(Joe)");
    }

    // From (2,L1,2): the list has exactly 26 pads (3 x 3 x 3 cube minus the start)
    // TODO(Joe)
    [Test]
    public void Cluster_OpenMap_Has26Targets()
    {
        Assert.Inconclusive("TODO(Joe)");
    }
}
