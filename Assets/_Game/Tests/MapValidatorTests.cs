// MapValidatorTests.cs
// Owner: Moustafa
// Layer: Tests, EditMode
//
// What this tests
// The map checks: floors, hatch pads, spawns, exposure, line of sight, reachability
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class MapValidatorTests
{
    // MapValidator (Moustafa)

    // A pad with an Open floor: one error
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Validator_FlagsOpenPadFloor()
    {
        MapLayout map = TestMaps.EmptyMap();
        TestMaps.AddPad(map, new GridCoord(2, 1, 2), 0);
        map.EditCell(new GridCoord(2, 1, 2)).storedFloor = FloorKind.Open;
        ValidationReport report = MapValidator.Validate(map, TestMaps.AllMovesets(), TestMaps.Geometry());
        Assert.AreEqual(1, report.Errors.FindAll(error => error.Contains("Open floor")).Count);
    }

    // A Hatch pad anchored to Center: one error
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Validator_FlagsCenteredHatchPad()
    {
        MapLayout map = TestMaps.EmptyMap();
        TestMaps.AddPad(map, new GridCoord(2, 0, 2), 0);
        map.EditCell(new GridCoord(2, 0, 2)).storedFloor = FloorKind.Hatch;
        ValidationReport report = MapValidator.Validate(map, TestMaps.AllMovesets(), TestMaps.Geometry());
        Assert.AreEqual(1, report.Errors.FindAll(error => error.Contains("Hatch floor")).Count);
    }

    // 7 spawn pads: error, 8: no spawn error
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Validator_FlagsTooFewSpawns()
    {
        MapLayout seven = TestMaps.EmptyMap();
        for (int i = 0; i < 7; i++)
        {
            TestMaps.AddPad(seven, new GridCoord(i % 6, 0, i / 6 * 3), i + 1);
        }
        ValidationReport report = new ValidationReport();
        MapValidator.CheckSpawns(seven, report);
        Assert.IsTrue(report.Errors.Exists(error => error.Contains("spawn pads")));

        MapLayout eight = TestMaps.EmptyMap();
        for (int i = 0; i < 8; i++)
        {
            TestMaps.AddPad(eight, new GridCoord(i % 6, 0, i / 6 * 3), i + 1);
        }
        report = new ValidationReport();
        MapValidator.CheckSpawns(eight, report);
        Assert.IsFalse(report.Errors.Exists(error => error.Contains("spawn pads")));
    }

    // Ranks 1, 2, 4 ... (3 missing) or 1, 2, 2 ...: error
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Validator_FlagsSpawnRankGapOrRepeat()
    {
        int[] gap = { 1, 2, 4, 5, 6, 7, 8, 9 };
        int[] repeat = { 1, 2, 2, 3, 4, 5, 6, 7 };
        foreach (int[] ranks in new[] { gap, repeat })
        {
            MapLayout map = TestMaps.EmptyMap();
            for (int i = 0; i < ranks.Length; i++)
            {
                TestMaps.AddPad(map, new GridCoord(i % 6, 0, i / 6 * 3), ranks[i]);
            }
            ValidationReport report = new ValidationReport();
            MapValidator.CheckSpawns(map, report);
            Assert.IsTrue(report.Errors.Exists(error => error.Contains("gap or a repeat")));
        }
    }

    // A pad boxed in by Solid walls on all sides and a Solid ceiling: exposure error
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Validator_FlagsPadExposedFromTooFewDirections()
    {
        // Every ground cell a pad, then box one in: four Solid sides and a Solid ceiling
        MapLayout map = TestMaps.EmptyMap();
        for (int x = 0; x < 6; x++)
        {
            for (int z = 0; z < 6; z++)
            {
                TestMaps.AddPad(map, new GridCoord(x, 0, z), 0);
            }
        }
        GridCoord boxed = new GridCoord(2, 0, 2);
        map.EditCell(boxed).storedNorth = SideKind.Solid;
        map.EditCell(boxed).storedEast = SideKind.Solid;
        map.EditCell(new GridCoord(2, 0, 1)).storedNorth = SideKind.Solid;
        map.EditCell(new GridCoord(1, 0, 2)).storedEast = SideKind.Solid;
        map.EditCell(new GridCoord(2, 1, 2)).storedFloor = FloorKind.Solid;
        Assert.Less(MapValidator.CountExposureDirections(map, boxed, TestMaps.Geometry()), MapValidator.MinExposure);
    }

    // Two pads facing each other: a Solid wall between them blocks sight, a Window lets it through
    // TODO(Moustafa) [X] Completed
    [Test]
    public void LineOfSight_SolidBlocks_WindowPasses()
    {
        MapLayout map = TestMaps.EmptyMap();
        GridCoord a = new GridCoord(1, 0, 2);
        GridCoord b = new GridCoord(3, 0, 2);
        TestMaps.AddPad(map, a, 0);
        TestMaps.AddPad(map, b, 0);
        map.EditCell(a).storedEast = SideKind.Solid;
        Assert.IsFalse(MapValidator.HasLineOfSight(map, a, b, TestMaps.Geometry()));
        map.EditCell(a).storedEast = SideKind.Window;
        Assert.IsTrue(MapValidator.HasLineOfSight(map, a, b, TestMaps.Geometry()));
    }

    // A pad no moveset can ever reach (cut off on its own): a reachability error
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Validator_FlagsUnreachablePad()
    {
        // Ground level full of pads, plus one pad high up on L2 with nothing on L1: Cluster and Ring can never climb there
        MapLayout map = TestMaps.EmptyMap();
        for (int x = 0; x < 6; x++)
        {
            for (int z = 0; z < 6; z++)
            {
                TestMaps.AddPad(map, new GridCoord(x, 0, z), 0);
            }
        }
        GridCoord cutOff = new GridCoord(5, 2, 5);
        TestMaps.AddPad(map, cutOff, 0);
        ValidationReport report = new ValidationReport();
        MapValidator.CheckReachability(map, TestMaps.AllMovesets(), report);
        Assert.IsTrue(report.Errors.Exists(error => error.Contains("can't reach " + cutOff)));
    }
}
