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
    // TODO(Moustafa)
    [Test]
    public void Layout_SouthSideReadsNeighboursNorth()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // GetSide facing out of the map is Solid on all four edges (north, east, south, west), whatever the edge cell stores
    // TODO(Moustafa)
    [Test]
    public void Layout_OutsideTheMapIsSolid()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Set storedFloor of (2,L1,2) to Hatch: GetCeiling((2,L0,2)) is Hatch
    // TODO(Moustafa)
    [Test]
    public void Layout_CeilingIsTheFloorAbove()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // GetCeiling of any top-level cell is Open
    // TODO(Moustafa)
    [Test]
    public void Layout_TopLevelCeilingIsOpen()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // A pad next to a Solid cell: GetSide toward the Solid cell is Solid, even if the pad cell stores Window there
    // TODO(Moustafa)
    [Test]
    public void Layout_SideTouchingSolidCellIsSolid()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Spawns ranked 3, 1, 2 placed in grid order: SpawnPads() returns them as 1, 2, 3
    // TODO(Moustafa)
    [Test]
    public void Layout_SpawnPadsSortedByRank()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }
}
