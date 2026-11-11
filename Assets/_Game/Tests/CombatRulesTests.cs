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
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_Damage_HeadAndBody()
    {
        Assert.AreEqual(50f, CombatRules.Damage(true, 25f, 50f));
        Assert.AreEqual(25f, CombatRules.Damage(false, 25f, 50f));
    }

    // ShotCounts(alive, ...) is true
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_AliveShooterShotCounts()
    {
        Assert.IsTrue(CombatRules.ShotCounts(true, double.NegativeInfinity, 10.0, 0.15f));
    }

    // Shooter died at 10.00, shot at 10.14: counts (inside the 0.15 window)
    // Not exactly 10.15: computers don't store 10.15 - 10.00 as exactly 0.15, a test on the edge passes or fails by luck
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_DeadShooterWithinTradeWindowCounts()
    {
        Assert.IsTrue(CombatRules.ShotCounts(false, 10.00, 10.14, 0.15f));
    }

    // Shooter died at 10.00, shot at 10.20: does not count
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_DeadShooterAfterTradeWindowDoesNotCount()
    {
        Assert.IsFalse(CombatRules.ShotCounts(false, 10.00, 10.20, 0.15f));
    }

    // Spawned at 0, not shot: protected at 2.9, not at 3.1
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_SpawnProtectionLastsThreeSeconds()
    {
        Assert.IsTrue(CombatRules.IsSpawnProtected(0.0, 2.9, 3f, false));
        Assert.IsFalse(CombatRules.IsSpawnProtected(0.0, 3.1, 3f, false));
    }

    // Spawned at 0, has acted (shot or teleported): not protected at 1.0
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_SpawnProtectionEndsWhenActing()
    {
        Assert.IsFalse(CombatRules.IsSpawnProtected(0.0, 1.0, 3f, true));
    }

    // Damaged at 0, RegenHp at 3.4: HP unchanged
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_NoRegenBeforeDelay()
    {
        Assert.AreEqual(40f, CombatRules.RegenHp(40f, 100f, 3.4, 0.0, 3.5f, 2.5f, 0.1f));
    }

    // HP 0, damaged at 0, tick from 3.5 to 6.0 in small steps: HP reaches 100
    // Compare with a tolerance, Assert.AreEqual(100f, hp, 0.01f), many small float steps don't add up exactly
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_RegenFullInTwoAndAHalfSeconds()
    {
        float hp = 0f;
        for (int i = 0; i < 250; i++)
        {
            hp = CombatRules.RegenHp(hp, 100f, 3.5 + i * 0.01, 0.0, 3.5f, 2.5f, 0.01f);
        }
        Assert.AreEqual(100f, hp, 0.01f);
    }

    // HP 99, a big tick: HP is 100, never above
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_RegenClampsToMax()
    {
        Assert.AreEqual(100f, CombatRules.RegenHp(99f, 100f, 10.0, 0.0, 3.5f, 2.5f, 1f));
    }

    // Range 40, tolerance 1: 40.5 m passes, 41.5 m is refused
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_HitBeyondRangeIsRefused()
    {
        Assert.IsTrue(CombatRules.IsHitInRange(40.5f, 40f, 1f));
        Assert.IsFalse(CombatRules.IsHitInRange(41.5f, 40f, 1f));
    }

    // Tolerance 1.5: origin 0.8 m from the head passes, 3 m is refused
    // TODO(Moustafa) [X] Completed
    [Test]
    public void Combat_FarShotOriginIsRefused()
    {
        Assert.IsTrue(CombatRules.IsOriginNearShooter(0.8f, 1.5f));
        Assert.IsFalse(CombatRules.IsOriginNearShooter(3f, 1.5f));
    }
}
