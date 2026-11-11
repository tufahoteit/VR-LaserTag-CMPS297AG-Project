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
    // TODO(Karim)
    [Test]
    public void Ring_ReachesAll16PadsTwoAway()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // From (2,L1,2): nothing 1 away, e.g. (3,L1,2) and (3,L0,3) are not in the list
    // TODO(Karim)
    [Test]
    public void Ring_NeverReachesOneAway()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // From (2,L0,2): (4,L1,2) is in the list, (4,L2,2) is not, the ring changes level by at most 1
    // TODO(Karim)
    [Test]
    public void Ring_LevelChangeAtMostOne()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // From (2,L0,2): (2,L1,2) is in the list, (2,L2,2) is not, own column is only 1 level
    // TODO(Karim)
    [Test]
    public void Ring_OwnColumn_OneLevelOnly()
    {
        Assert.Inconclusive("TODO(Karim)");
    }
}
