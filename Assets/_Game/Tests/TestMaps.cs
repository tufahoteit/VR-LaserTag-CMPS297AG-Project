// TestMaps.cs
// Owner: Moustafa
// Layer: Tests, EditMode (shared helpers, no tests of its own)
//
// What this is
// The small maps and values every test file builds on, so each test file only holds its own tests
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
// For the rules tests (charging, combat, match):
// These are the numbers the demo shows: charges, energy, damage, HP, deaths, kills, the winner
// The brief says they must behave the same on every headset, and they all come from these rules
// Checking them in VR means playing until the exact situation happens (a trade on match point, an empty energy bar)
// A test creates that situation in one line and checks the answer in one second
// It is also our safety net in the demo: change a number live, press Run, see what still works
// And the results go in the report (the brief asks for testing results)
//
// What a test is
// A small function that sets something up, calls the code, and checks the answer
// Time is passed in as a number (now), so a test jumps forward in time without waiting
// Example:
//   var charging = new TeleportCharging(3, 4.5f, 0.3f);
//   charging.Reset(0);
//   charging.TrySpend(0);
//   charging.Tick(4.4);
//   Assert.AreEqual(2, charging.Charges);   // not back yet
//   charging.Tick(4.5);
//   Assert.AreEqual(3, charging.Charges);   // back after 4.5 s
//
// Numbers used in these tests are the decided ones:
//   charges 3, recharge 4.5 s, teleport gap 0.3 s
//   energy 100, 30 shots per bar, shot gap 0.25 s, regen delay 0.6 s
//   HP 100, body 25, head 50, regen after 3.5 s, full in 2.5 s, trade window 0.15 s, spawn protection 3 s
//
// Decimal numbers are never exact in a computer, so:
//   test clearly inside and clearly outside a limit, never exactly on it (10.14 and 10.20, not 10.15)
//   compare decimal results with a tolerance: Assert.AreEqual(expected, actual, 0.01f)

using Game.Core;

public static class TestMaps
{
    // Test-only map, never used in the game: 6 x 6 x 3, every cell a pad, every floor Solid, no walls
    // Real maps have Empty and Solid cells too, this one doesn't on purpose:
    // with a pad everywhere, a missing target can only mean the moveset pattern is wrong
    public static MapLayout OpenMap()
    {
        MapLayout map = new MapLayout();
        map.CreateEmpty(6, 6, 3);
        for (int i = 0; i < map.CellCount; i++)
        {
            Cell cell = map.EditCell(map.CoordOf(i));
            cell.kind = CellKind.Pad;
            cell.storedFloor = FloorKind.Solid;
        }
        return map;
    }

    // A 6 x 6 x 3 map of Empty cells, tests add pads on top of it
    public static MapLayout EmptyMap()
    {
        MapLayout map = new MapLayout();
        map.CreateEmpty(6, 6, 3);
        return map;
    }

    // Make coord a pad with a Solid floor, spawnRank 0 = not a spawn
    public static void AddPad(MapLayout map, GridCoord coord, int spawnRank)
    {
        Cell cell = map.EditCell(coord);
        cell.kind = CellKind.Pad;
        cell.storedFloor = FloorKind.Solid;
        cell.spawnRank = spawnRank;
    }

    // Nobody is standing anywhere
    public static bool Nobody(GridCoord coord)
    {
        return false;
    }

    // One of each moveset, for the shared rules and the validator
    public static IMoveset[] AllMovesets()
    {
        return new IMoveset[] { new ClusterMoveset(), new RingMoveset(), new RayMoveset() };
    }

    // The decided sizes, the same as MapDefinition and MatchSettings
    public static MapGeometry Geometry()
    {
        MapGeometry geometry = new MapGeometry();
        geometry.cellSize = 3f;
        geometry.levelHeight = 3f;
        geometry.padAnchorDistance = 0.9f;
        geometry.eyeHeight = 1.65f;
        geometry.halfWallHeight = 1.2f;
        geometry.windowBottom = 1.2f;
        geometry.windowTop = 1.8f;
        return geometry;
    }
}
