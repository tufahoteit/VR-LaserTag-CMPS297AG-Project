// ChargingTests.cs
// Owner: Ali
// Layer: Tests, EditMode
//
// What this tests
// Teleport charges (spend, gap, recharge, kill bonus) and laser energy (cost, 30 shots, fire gap, regen)
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class ChargingTests
{
    // 3 charges, 4.5 s recharge, 0.3 s gap, full at time 0
    private static TeleportCharging NewCharging()
    {
        TeleportCharging charging = new TeleportCharging(3, 4.5f, 0.3f);
        charging.Reset(0.0);
        return charging;
    }

    // Energy 100, 30 shots per bar, regen 30 per second after 0.6 s, 0.25 s between shots, full
    private static LaserEnergyCharging NewGun()
    {
        LaserEnergyCharging gun = new LaserEnergyCharging(100f, 100f / 30f, 30f, 0.6f, 0.25f);
        gun.Reset();
        return gun;
    }

    // TeleportCharging (Ali)

    // After Reset(0): Charges == MaxCharges
    // TODO(Ali)
    [Test]
    public void Charging_StartsFullAfterReset()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // TrySpend: true, Charges goes from 3 to 2
    // TODO(Ali)
    [Test]
    public void Charging_SpendRemovesOne()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Spend all 3 (0.3 s apart), the 4th TrySpend is false
    // TODO(Ali)
    [Test]
    public void Charging_CannotSpendAtZero()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // TrySpend at 1.0 and again at 1.1: the second is false (gap is 0.3 s), at 1.3 it is true
    // TODO(Ali)
    [Test]
    public void Charging_MinGapBlocksBackToBackTeleports()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Spend 1 at time 0, Tick(4.4): still 2, Tick(4.5): back to 3
    // TODO(Ali)
    [Test]
    public void Charging_RechargesOneEvery4_5Seconds()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Spend all 3, Tick(9.0) once: 2 charges back in one tick
    // TODO(Ali)
    [Test]
    public void Charging_RechargesSeveralAfterLongPause()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Spend 1, Tick(100): Charges is 3, never more
    // TODO(Ali)
    [Test]
    public void Charging_NeverExceedsMax()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Spend 1, AddCharge: true, back to 3
    // TODO(Ali)
    [Test]
    public void Charging_KillBonusAddsOne()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // At full charges, AddCharge: false, still 3
    // TODO(Ali)
    [Test]
    public void Charging_KillBonusLostWhenFull()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Spend 2 at time 0, AddCharge at 2.0: the recharge still finishes at 4.5, not 6.5
    // TODO(Ali)
    [Test]
    public void Charging_KillBonusKeepsRechargeProgress()
    {
        Assert.Inconclusive("TODO(Ali)");
    }


    // LaserEnergyCharging (Ali)

    // TryFire: true, Energy goes down by CostPerShot
    // TODO(Ali)
    [Test]
    public void Energy_FireCostsEnergy()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Fire 30 times 0.25 s apart: all true, the 31st is false, the bar is empty (this is the test that catches the rounding problem)
    // TODO(Ali)
    [Test]
    public void Energy_ThirtyShotsPerFullBar()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // TryFire at 1.0 and again at 1.1: the second is false (gap is 0.25 s)
    // TODO(Ali)
    [Test]
    public void Energy_MinShotGapBlocksFastShots()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Fire at 0, Tick at 0.5: Energy unchanged (regen delay is 0.6 s)
    // TODO(Ali)
    [Test]
    public void Energy_NoRegenDuringDelay()
    {
        Assert.Inconclusive("TODO(Ali)");
    }

    // Fire at 0, Tick for 10 s: Energy is back to MaxEnergy, never above
    // TODO(Ali)
    [Test]
    public void Energy_RegensAfterDelayToMax()
    {
        Assert.Inconclusive("TODO(Ali)");
    }
}
