// RingMovesetTests.cs
// Owner: Karim
// Layer: Tests, EditMode
//
// What this tests
// Ring: every pad exactly 2 away and your own column, 1 level up or down
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class RingMovesetTests
{
    // Ring (Karim)

    // From (2,L1,2): all 16 pads exactly 2 away on L1 are in the list, including (4,L1,3) off the straight lines
    // TODO(Karim) [X] Completed
    [Test]
    public void Ring_ReachesAll16PadsTwoAway()
    {
        GridCoord start = new GridCoord(2, 1, 2);
        List<GridCoord> list = new RingMoveset().GetReachable(start, TestMaps.OpenMap(), TestMaps.Nobody);
        int found = 0;
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dz = -2; dz <= 2; dz++)
            {
                GridCoord target = start.Offset(dx, 0, dz);
                if (GridCoord.PadDistance(start, target) == 2)
                {
                    Assert.Contains(target, list);
                    found++;
                }
            }
        }
        Assert.AreEqual(16, found);
        Assert.Contains(new GridCoord(4, 1, 3), list);
    }

    // From (2,L1,2): nothing 1 away, e.g. (3,L1,2) and (3,L0,3) are not in the list
    // TODO(Karim) [X] Completed
    [Test]
    public void Ring_NeverReachesOneAway()
    {
        List<GridCoord> list = new RingMoveset().GetReachable(new GridCoord(2, 1, 2), TestMaps.OpenMap(), TestMaps.Nobody);
        Assert.IsFalse(list.Contains(new GridCoord(3, 1, 2)));
        Assert.IsFalse(list.Contains(new GridCoord(3, 0, 3)));
    }

    // From (2,L0,2): (4,L1,2) is in the list, (4,L2,2) is not, the ring changes level by at most 1
    // TODO(Karim) [X] Completed
    [Test]
    public void Ring_LevelChangeAtMostOne()
    {
        List<GridCoord> list = new RingMoveset().GetReachable(new GridCoord(2, 0, 2), TestMaps.OpenMap(), TestMaps.Nobody);
        Assert.Contains(new GridCoord(4, 1, 2), list);
        Assert.IsFalse(list.Contains(new GridCoord(4, 2, 2)));
    }

    // From (2,L0,2): (2,L1,2) is in the list, (2,L2,2) is not, own column is only 1 level
    // TODO(Karim) [X] Completed
    [Test]
    public void Ring_OwnColumn_OneLevelOnly()
    {
        List<GridCoord> list = new RingMoveset().GetReachable(new GridCoord(2, 0, 2), TestMaps.OpenMap(), TestMaps.Nobody);
        Assert.Contains(new GridCoord(2, 1, 2), list);
        Assert.IsFalse(list.Contains(new GridCoord(2, 2, 2)));
    }
}
