// SharedMovesetTests.cs
// Owner: Joseph
// Layer: Tests, EditMode
//
// What this tests
// Rules every moveset follows: no occupied pads, no Empty or Solid cells, never the start pad, walls and floors never block a teleport
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class SharedMovesetTests
{
    // Shared rules (Joseph), each test runs all three movesets

    // Mark a target pad occupied: it disappears from every moveset's list
    // TODO(Joseph)
    [Test]
    public void AllMovesets_ExcludeOccupiedPads()
    {
        Assert.Inconclusive("TODO(Joseph)");
    }

    // Make a target cell Empty or Solid: it disappears from every moveset's list
    // TODO(Joseph)
    [Test]
    public void AllMovesets_ExcludeNonPadCells()
    {
        Assert.Inconclusive("TODO(Joseph)");
    }

    // The start pad is never in any moveset's list
    // TODO(Joseph)
    [Test]
    public void AllMovesets_NeverIncludeStartPad()
    {
        Assert.Inconclusive("TODO(Joseph)");
    }

    // Put Solid walls and floors everywhere: every list is exactly the same as on the open map
    // TODO(Joseph)
    [Test]
    public void AllMovesets_IgnoreWallsAndFloors()
    {
        Assert.Inconclusive("TODO(Joseph)");
    }
}
