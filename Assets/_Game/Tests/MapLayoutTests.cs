// MapLayoutTests.cs
// Owner: Moustafa
// Layer: Tests, EditMode
//
// What this tests
// Reading the map: shared walls, the outer edge, ceilings, Solid cells, spawn order
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class MapLayoutTests
{
    // MapLayout (Moustafa)

    // Set storedNorth of (2,L0,2) to Window: GetSide((2,L0,3), South) is Window
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Layout_SouthSideReadsNeighboursNorth()
    {
        MapLayout map = TestMaps.EmptyMap();
        map.EditCell(new GridCoord(2, 0, 2)).storedNorth = SideKind.Window;
        Assert.AreEqual(SideKind.Window, map.GetSide(new GridCoord(2, 0, 3), Direction.South));
    }

    // GetSide facing out of the map is Solid on all four edges (north, east, south, west), whatever the edge cell stores
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Layout_OutsideTheMapIsSolid()
    {
        MapLayout map = TestMaps.EmptyMap();
        Assert.AreEqual(SideKind.Solid, map.GetSide(new GridCoord(2, 0, 5), Direction.North));
        Assert.AreEqual(SideKind.Solid, map.GetSide(new GridCoord(5, 0, 2), Direction.East));
        Assert.AreEqual(SideKind.Solid, map.GetSide(new GridCoord(2, 0, 0), Direction.South));
        Assert.AreEqual(SideKind.Solid, map.GetSide(new GridCoord(0, 0, 2), Direction.West));
    }

    // Set storedFloor of (2,L1,2) to Hatch: GetCeiling((2,L0,2)) is Hatch
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Layout_CeilingIsTheFloorAbove()
    {
        MapLayout map = TestMaps.EmptyMap();
        map.EditCell(new GridCoord(2, 1, 2)).storedFloor = FloorKind.Hatch;
        Assert.AreEqual(FloorKind.Hatch, map.GetCeiling(new GridCoord(2, 0, 2)));
    }

    // GetCeiling of any top-level cell is Open
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Layout_TopLevelCeilingIsOpen()
    {
        MapLayout map = TestMaps.EmptyMap();
        Assert.AreEqual(FloorKind.Open, map.GetCeiling(new GridCoord(1, 2, 1)));
    }

    // A pad next to a Solid cell: GetSide toward the Solid cell is Solid, even if the pad cell stores Window there
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Layout_SideTouchingSolidCellIsSolid()
    {
        MapLayout map = TestMaps.EmptyMap();
        Cell pad = map.EditCell(new GridCoord(2, 0, 2));
        pad.kind = CellKind.Pad;
        pad.storedNorth = SideKind.Window;
        map.EditCell(new GridCoord(2, 0, 3)).kind = CellKind.Solid;
        Assert.AreEqual(SideKind.Solid, map.GetSide(new GridCoord(2, 0, 2), Direction.North));
    }

    // Spawns ranked 3, 1, 2 placed in grid order: SpawnPads() returns them as 1, 2, 3
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Layout_SpawnPadsSortedByRank()
    {
        MapLayout map = TestMaps.EmptyMap();
        TestMaps.AddPad(map, new GridCoord(0, 0, 0), 3);
        TestMaps.AddPad(map, new GridCoord(1, 0, 0), 1);
        TestMaps.AddPad(map, new GridCoord(2, 0, 0), 2);
        List<GridCoord> spawns = map.SpawnPads();
        Assert.AreEqual(new GridCoord(1, 0, 0), spawns[0]);
        Assert.AreEqual(new GridCoord(2, 0, 0), spawns[1]);
        Assert.AreEqual(new GridCoord(0, 0, 0), spawns[2]);
    }
}
