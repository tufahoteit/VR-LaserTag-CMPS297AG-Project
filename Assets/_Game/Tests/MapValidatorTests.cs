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
    // TODO(Moustafa)
    [Test]
    public void Validator_FlagsOpenPadFloor()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // A Hatch pad anchored to Center: one error
    // TODO(Moustafa)
    [Test]
    public void Validator_FlagsCenteredHatchPad()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // 7 spawn pads: error, 8: no spawn error
    // TODO(Moustafa)
    [Test]
    public void Validator_FlagsTooFewSpawns()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Ranks 1, 2, 4 ... (3 missing) or 1, 2, 2 ...: error
    // TODO(Moustafa)
    [Test]
    public void Validator_FlagsSpawnRankGapOrRepeat()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // A pad boxed in by Solid walls on all sides and a Solid ceiling: exposure error
    // TODO(Moustafa)
    [Test]
    public void Validator_FlagsPadExposedFromTooFewDirections()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Two pads facing each other: a Solid wall between them blocks sight, a Window lets it through
    // TODO(Moustafa)
    [Test]
    public void LineOfSight_SolidBlocks_WindowPasses()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // A pad no moveset can ever reach (cut off on its own): a reachability error
    // TODO(Moustafa)
    [Test]
    public void Validator_FlagsUnreachablePad()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }
}
