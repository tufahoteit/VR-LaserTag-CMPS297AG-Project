// MovesetTests.cs
// Owners: Moustafa (Ray), Joe (Cluster), Ali (Ring), shared rules: anyone
// Layer: Tests, EditMode
//
// Why we test
// The movesets decide where you can teleport, and two places use them:
// your headset (to show the ghost markers) and the host (to approve the teleport)
// If a moveset has a bug, you see a marker the host refuses and get snapped back,
// and in VR that is slow to find: five people on headsets guessing which rule is wrong
// A test finds it in one second, without a headset, and says exactly which rule broke
// It is also our safety net in the demo: change a rule live, press Run, see what still works
// And the results go in the report (the brief asks for testing results)
//
// What a test is
// A small function that sets something up, calls the code, and checks the answer
//   set up: make a map and pick a start pad
//   call: moveset.GetReachable(start, map, NobodyThere)
//   check: Assert.Contains / Assert.IsFalse(list.Contains(...)) / Assert.AreEqual(expected count, list.Count)
// Example:
//   var map = OpenMap();
//   var list = new RingMoveset().GetReachable(new GridCoord(2, 1, 2), map, NobodyThere);
//   Assert.IsFalse(list.Contains(new GridCoord(3, 1, 2)));   // Ring never reaches 1 away
//
// Colours in the Test Runner
//   yellow (Inconclusive): not written yet, replace Assert.Inconclusive with the real checks
//   green: the rule works
//   red: the rule is broken, the message says which check failed
//
// How to run
//   Window, General, Test Runner, EditMode tab, Run All
//
// Your task
// Each test has a comment above it with the exact start pad and which pads must or must not be in the list
// Write those checks, run, make it green, commit with your file

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class MovesetTests
{
    // Test-only map, never used in the game: 6 x 6 x 3, every cell a pad, every floor Solid, no walls
    // Real maps have Empty and Solid cells too, this one doesn't on purpose:
    // with a pad everywhere, a missing target can only mean the moveset pattern is wrong
    // Tests that check non-pad cells, walls or occupied pads change just that one thing on top of this map
    // TODO(any): MapLayout.CreateEmpty(6, 6, 3), then EditCell every cell to CellKind.Pad with a Solid floor
    private static MapLayout OpenMap()
    {
        return null;
    }

    // Nobody is standing anywhere
    private static bool NobodyThere(GridCoord coord)
    {
        return false;
    }

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

    // Ring (Ali)

    // From (2,L1,2): all 16 pads exactly 2 away on L1 are in the list, including (4,L1,3) off the straight lines
    // TODO(Ali)
    [Test]
    public void Ring_ReachesAll16PadsTwoAway()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // From (2,L1,2): nothing 1 away, e.g. (3,L1,2) and (3,L0,3) are not in the list
    // TODO(Ali)
    [Test]
    public void Ring_NeverReachesOneAway()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // From (2,L0,2): (4,L1,2) is in the list, (4,L2,2) is not, the ring changes level by at most 1
    // TODO(Ali)
    [Test]
    public void Ring_LevelChangeAtMostOne()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // From (2,L0,2): (2,L1,2) is in the list, (2,L2,2) is not, own column is only 1 level
    // TODO(Ali)
    [Test]
    public void Ring_OwnColumn_OneLevelOnly()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Shared rules (anyone), each test runs all three movesets

    // Mark a target pad occupied: it disappears from every moveset's list
    // TODO(any)
    [Test]
    public void AllMovesets_ExcludeOccupiedPads()
    {
        Assert.Inconclusive("TODO(any)");
    }

    // Make a target cell Empty or Solid: it disappears from every moveset's list
    // TODO(any)
    [Test]
    public void AllMovesets_ExcludeNonPadCells()
    {
        Assert.Inconclusive("TODO(any)");
    }

    // The start pad is never in any moveset's list
    // TODO(any)
    [Test]
    public void AllMovesets_NeverIncludeStartPad()
    {
        Assert.Inconclusive("TODO(any)");
    }

    // Put Solid walls and floors everywhere: every list is exactly the same as on the open map
    // TODO(any)
    [Test]
    public void AllMovesets_IgnoreWallsAndFloors()
    {
        Assert.Inconclusive("TODO(any)");
    }

    // MovesetFactory.Create(kind).Kind == kind for Cluster, Ring and Ray
    // TODO(Moustafa)
    [Test]
    public void MovesetFactory_CreatesMatchingKind()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }
}
