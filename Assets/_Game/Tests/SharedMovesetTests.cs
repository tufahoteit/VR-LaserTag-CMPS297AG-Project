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
    // TODO(Joseph) [X] Completed
    [Test]
    public void AllMovesets_ExcludeOccupiedPads()
    {
        GridCoord start = new GridCoord(2, 1, 2);
        foreach (IMoveset moveset in TestMaps.AllMovesets())
        {
            List<GridCoord> open = moveset.GetReachable(start, TestMaps.OpenMap(), TestMaps.Nobody);
            Assert.IsNotEmpty(open, moveset.Kind + " reaches nothing on the open map");
            GridCoord taken = open[0];
            List<GridCoord> list = moveset.GetReachable(start, TestMaps.OpenMap(), coord => coord == taken);
            Assert.IsFalse(list.Contains(taken), moveset.Kind + " still lists an occupied pad");
        }
    }

    // Make a target cell Empty or Solid: it disappears from every moveset's list
    // TODO(Joseph) [X] Completed
    [Test]
    public void AllMovesets_ExcludeNonPadCells()
    {
        GridCoord start = new GridCoord(2, 1, 2);
        foreach (IMoveset moveset in TestMaps.AllMovesets())
        {
            List<GridCoord> open = moveset.GetReachable(start, TestMaps.OpenMap(), TestMaps.Nobody);
            GridCoord emptyTarget = open[0];
            GridCoord solidTarget = open[open.Count - 1];

            MapLayout map = TestMaps.OpenMap();
            map.EditCell(emptyTarget).kind = CellKind.Empty;
            map.EditCell(solidTarget).kind = CellKind.Solid;
            List<GridCoord> list = moveset.GetReachable(start, map, TestMaps.Nobody);
            Assert.IsFalse(list.Contains(emptyTarget), moveset.Kind + " lists an Empty cell");
            Assert.IsFalse(list.Contains(solidTarget), moveset.Kind + " lists a Solid cell");
        }
    }

    // The start pad is never in any moveset's list
    // TODO(Joseph) [X] Completed
    [Test]
    public void AllMovesets_NeverIncludeStartPad()
    {
        GridCoord start = new GridCoord(2, 1, 2);
        foreach (IMoveset moveset in TestMaps.AllMovesets())
        {
            Assert.IsFalse(moveset.GetReachable(start, TestMaps.OpenMap(), TestMaps.Nobody).Contains(start), moveset.Kind + " lists the start pad");
        }
    }

    // Put Solid walls and floors everywhere: every list is exactly the same as on the open map
    // TODO(Joseph) [X] Completed
    [Test]
    public void AllMovesets_IgnoreWallsAndFloors()
    {
        GridCoord start = new GridCoord(2, 1, 2);
        MapLayout walled = TestMaps.OpenMap();
        for (int i = 0; i < walled.CellCount; i++)
        {
            Cell cell = walled.EditCell(walled.CoordOf(i));
            cell.storedNorth = SideKind.Solid;
            cell.storedEast = SideKind.Solid;
            cell.storedFloor = FloorKind.Solid;
        }
        foreach (IMoveset moveset in TestMaps.AllMovesets())
        {
            List<GridCoord> open = moveset.GetReachable(start, TestMaps.OpenMap(), TestMaps.Nobody);
            List<GridCoord> withWalls = moveset.GetReachable(start, walled, TestMaps.Nobody);
            CollectionAssert.AreEquivalent(open, withWalls, moveset.Kind + " is blocked by walls or floors");
        }
    }
}
