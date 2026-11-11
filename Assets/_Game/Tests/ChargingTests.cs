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
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_StartsFullAfterReset()
    {
        TeleportCharging charging = NewCharging();
        Assert.AreEqual(charging.MaxCharges, charging.Charges);
    }

    // TrySpend: true, Charges goes from 3 to 2
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_SpendRemovesOne()
    {
        TeleportCharging charging = NewCharging();
        Assert.IsTrue(charging.TrySpend(1.0));
        Assert.AreEqual(2, charging.Charges);
    }

    // Spend all 3 (0.3 s apart), the 4th TrySpend is false
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_CannotSpendAtZero()
    {
        // 0.5 s apart, clearly more than the 0.3 s gap
        TeleportCharging charging = NewCharging();
        Assert.IsTrue(charging.TrySpend(0.0));
        Assert.IsTrue(charging.TrySpend(0.5));
        Assert.IsTrue(charging.TrySpend(1.0));
        Assert.IsFalse(charging.TrySpend(1.5));
        Assert.AreEqual(0, charging.Charges);
    }

    // TrySpend at 1.0 and again at 1.1: the second is false (gap is 0.3 s), at 1.3 it is true
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_MinGapBlocksBackToBackTeleports()
    {
        // 1.31 instead of 1.3: 1.3 - 1.0 is a hair below 0.3 in a computer, the test would fail by luck
        TeleportCharging charging = NewCharging();
        Assert.IsTrue(charging.TrySpend(1.0));
        Assert.IsFalse(charging.TrySpend(1.1));
        Assert.IsTrue(charging.TrySpend(1.31));
    }

    // Spend 1 at time 0, Tick(4.4): still 2, Tick(4.5): back to 3
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_RechargesOneEvery4_5Seconds()
    {
        TeleportCharging charging = NewCharging();
        charging.TrySpend(0.0);
        charging.Tick(4.4);
        Assert.AreEqual(2, charging.Charges);
        charging.Tick(4.5);
        Assert.AreEqual(3, charging.Charges);
    }

    // Spend all 3, Tick(9.0) once: 2 charges back in one tick
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_RechargesSeveralAfterLongPause()
    {
        TeleportCharging charging = NewCharging();
        charging.TrySpend(0.0);
        charging.TrySpend(0.5);
        charging.TrySpend(1.0);
        charging.Tick(9.0);
        Assert.AreEqual(2, charging.Charges);
    }

    // Spend 1, Tick(100): Charges is 3, never more
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_NeverExceedsMax()
    {
        TeleportCharging charging = NewCharging();
        charging.TrySpend(0.0);
        charging.Tick(100.0);
        Assert.AreEqual(3, charging.Charges);
    }

    // Spend 1, AddCharge: true, back to 3
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_KillBonusAddsOne()
    {
        TeleportCharging charging = NewCharging();
        charging.TrySpend(0.0);
        Assert.IsTrue(charging.AddCharge());
        Assert.AreEqual(3, charging.Charges);
    }

    // At full charges, AddCharge: false, still 3
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_KillBonusLostWhenFull()
    {
        TeleportCharging charging = NewCharging();
        Assert.IsFalse(charging.AddCharge());
        Assert.AreEqual(3, charging.Charges);
    }

    // Spend 2 at time 0, AddCharge at 2.0: the recharge still finishes at 4.5, not 6.5
    // TODO(Ali) [X] Completed
    [Test]
    public void Charging_KillBonusKeepsRechargeProgress()
    {
        // The second spend is 0.5 s later because of the teleport gap, the recharge started with the first one at 0
        TeleportCharging charging = NewCharging();
        charging.TrySpend(0.0);
        charging.TrySpend(0.5);
        charging.AddCharge();
        Assert.AreEqual(2, charging.Charges);
        charging.Tick(4.4);
        Assert.AreEqual(2, charging.Charges);
        charging.Tick(4.5);
        Assert.AreEqual(3, charging.Charges);
    }


    // LaserEnergyCharging (Ali)

    // TryFire: true, Energy goes down by CostPerShot
    // TODO(Ali) [X] Completed
    [Test]
    public void Energy_FireCostsEnergy()
    {
        LaserEnergyCharging gun = NewGun();
        float before = gun.Energy;
        Assert.IsTrue(gun.TryFire(1.0));
        Assert.AreEqual(before - gun.CostPerShot, gun.Energy, 0.001f);
    }

    // Fire 30 times 0.25 s apart: all true, the 31st is false, the bar is empty (this is the test that catches the rounding problem)
    // TODO(Ali) [X] Completed
    [Test]
    public void Energy_ThirtyShotsPerFullBar()
    {
        LaserEnergyCharging gun = NewGun();
        for (int i = 0; i < 30; i++)
        {
            Assert.IsTrue(gun.TryFire(i * 0.25), "shot " + (i + 1) + " refused");
        }
        Assert.IsFalse(gun.TryFire(30 * 0.25));
        Assert.AreEqual(0f, gun.Energy, 0.01f);
    }

    // TryFire at 1.0 and again at 1.1: the second is false (gap is 0.25 s)
    // TODO(Ali) [X] Completed
    [Test]
    public void Energy_MinShotGapBlocksFastShots()
    {
        LaserEnergyCharging gun = NewGun();
        Assert.IsTrue(gun.TryFire(1.0));
        Assert.IsFalse(gun.TryFire(1.1));
    }

    // Fire at 0, Tick at 0.5: Energy unchanged (regen delay is 0.6 s)
    // TODO(Ali) [X] Completed
    [Test]
    public void Energy_NoRegenDuringDelay()
    {
        LaserEnergyCharging gun = NewGun();
        gun.TryFire(0.0);
        float afterShot = gun.Energy;
        Assert.IsFalse(gun.Tick(0.5, 0.5f));
        Assert.AreEqual(afterShot, gun.Energy, 0.0001f);
    }

    // Fire at 0, Tick for 10 s: Energy is back to MaxEnergy, never above
    // TODO(Ali) [X] Completed
    [Test]
    public void Energy_RegensAfterDelayToMax()
    {
        LaserEnergyCharging gun = NewGun();
        gun.TryFire(0.0);
        for (int i = 1; i <= 100; i++)
        {
            gun.Tick(i * 0.1, 0.1f);
        }
        Assert.AreEqual(gun.MaxEnergy, gun.Energy, 0.001f);
        Assert.LessOrEqual(gun.Energy, gun.MaxEnergy);
    }
}
