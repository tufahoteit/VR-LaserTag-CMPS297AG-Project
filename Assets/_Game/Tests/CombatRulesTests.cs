// CombatRulesTests.cs
// Owner: Moustafa
// Layer: Tests, EditMode
//
// What this tests
// Damage, trades, spawn protection, HP regen, the host safety checks
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class CombatRulesTests
{
    // CombatRules (Moustafa)

    // Damage(true, 25, 50) == 50, Damage(false, 25, 50) == 25
    // TODO(Moustafa)
    [Test]
    public void Combat_Damage_HeadAndBody()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // ShotCounts(alive, ...) is true
    // TODO(Moustafa)
    [Test]
    public void Combat_AliveShooterShotCounts()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Shooter died at 10.00, shot at 10.14: counts (inside the 0.15 window)
    // Not exactly 10.15: computers don't store 10.15 - 10.00 as exactly 0.15, a test on the edge passes or fails by luck
    // TODO(Moustafa)
    [Test]
    public void Combat_DeadShooterWithinTradeWindowCounts()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Shooter died at 10.00, shot at 10.20: does not count
    // TODO(Moustafa)
    [Test]
    public void Combat_DeadShooterAfterTradeWindowDoesNotCount()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Spawned at 0, not shot: protected at 2.9, not at 3.1
    // TODO(Moustafa)
    [Test]
    public void Combat_SpawnProtectionLastsThreeSeconds()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Spawned at 0, has acted (shot or teleported): not protected at 1.0
    // TODO(Moustafa)
    [Test]
    public void Combat_SpawnProtectionEndsWhenActing()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Damaged at 0, RegenHp at 3.4: HP unchanged
    // TODO(Moustafa)
    [Test]
    public void Combat_NoRegenBeforeDelay()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // HP 0, damaged at 0, tick from 3.5 to 6.0 in small steps: HP reaches 100
    // Compare with a tolerance, Assert.AreEqual(100f, hp, 0.01f), many small float steps don't add up exactly
    // TODO(Moustafa)
    [Test]
    public void Combat_RegenFullInTwoAndAHalfSeconds()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // HP 99, a big tick: HP is 100, never above
    // TODO(Moustafa)
    [Test]
    public void Combat_RegenClampsToMax()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Range 40, tolerance 1: 40.5 m passes, 41.5 m is refused
    // TODO(Moustafa)
    [Test]
    public void Combat_HitBeyondRangeIsRefused()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }

    // Tolerance 1.5: origin 0.8 m from the head passes, 3 m is refused
    // TODO(Moustafa)
    [Test]
    public void Combat_FarShotOriginIsRefused()
    {
        Assert.Inconclusive("TODO(Moustafa)");
    }
}
