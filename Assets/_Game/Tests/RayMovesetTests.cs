// RayMovesetTests.cs
// Owner: Moustafa
// Layer: Tests, EditMode
//
// What this tests
// Ray: own column any level, 8 flat lines from 2 pads away to the edge
// MovesetFactory makes the right moveset for each kind
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class RayMovesetTests
{
    // Ray (Moustafa)

    // From (2,L1,2): (2,L0,2) and (2,L2,2) are both in the list
    // TODO(Moustafa)
    [Test]
    public void Ray_OwnColumn_ReachesEveryLevel()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // From (2,L1,2): (4,L1,2) east, (4,L1,4) north-east, (0,L1,0) south-west are in the list
    // TODO(Moustafa)
    [Test]
    public void Ray_FlatLines_StartTwoPadsAway()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // From (0,L0,0): (5,L0,0) and (5,L0,5) are in the list, the line runs to the edge
    // TODO(Moustafa)
    [Test]
    public void Ray_FlatLines_GoToTheMapEdge()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // From (2,L1,2): nothing 1 pad away on the floor, e.g. (3,L1,2) and (3,L1,3) are not in the list
    // TODO(Moustafa)
    [Test]
    public void Ray_NeverReachesOneAway()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // From (2,L1,2): (4,L0,2) and (4,L2,2) are not in the list, lines never change level
    // TODO(Moustafa)
    [Test]
    public void Ray_FlatLines_StayOnTheSameLevel()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // From (2,L1,2): (4,L1,3) is not in the list, it is 2 away but not on a straight line
    // TODO(Moustafa)
    [Test]
    public void Ray_NeverReachesOffLineCells()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // From (0,L0,0) with (2,L0,0) occupied: (2,L0,0) is not in the list, (3,L0,0) still is
    // TODO(Moustafa)
    [Test]
    public void Ray_OccupiedPad_IsSkipped_LineContinues()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }


    // MovesetFactory.Create(kind).Kind == kind for Cluster, Ring and Ray
    // TODO(Moustafa)
    [Test]
    public void MovesetFactory_CreatesMatchingKind()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }
}
