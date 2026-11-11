// Charging.cs
// Owner: Ali
// Layer: Core, pure C#, no Unity
//
// Two resources a player spends and gets back over time
//   TeleportCharging: teleport charges, 1 per teleport, +1 every 4.5 seconds, +1 per kill
//   LaserEnergyCharging: laser energy, drains per shot, refills after you stop shooting
//
// Who uses them
// These are plain classes, not components, nothing is selected in the editor
// The numbers they start with come from the data assets (CharacterDefinition, WeaponDefinition, MatchSettings)
//   TeleportCharging belongs to the host: PlayerMovement creates it in code when the player spawns on the host
//   the host decides teleports, because two players may want the same pad and only the host can say who was first
//   it syncs Charges and RechargeStartTime to the owner only (your board needs both to draw the charge lights filling)
//   nobody else uses another player's charges, the brief asks to sync charge "as relevant to each player"
//   LaserEnergyCharging belongs to the shooter's own headset: the gun must react instantly, no waiting for the host
//   it syncs Energy to every headset: the energy strip on your gun is visible to everyone, like real laser tag gun lights
//
// Network timing
// Wi-Fi does not deliver messages at perfect intervals, two legal actions can arrive closer together than they were sent
// So the strict gaps are checked where the action starts, and the host only refuses what our own code could never produce:
//   teleports: your headset enforces the real 0.3 s gap, the host's copy is created with a smaller gap (0.2 s)
//   shots: your headset enforces the real 0.25 s gap, the host only refuses shots closer than half of it
// Legal actions are never refused
//
// Time
// Neither class reads a clock, the caller passes the current time in (now, in seconds)
// That keeps them deterministic and testable: a test can jump time forward without waiting
//   TeleportCharging: the host passes the network server time, so your board fills in step with the host
//   LaserEnergyCharging: the shooter's headset passes its own clock, the gun never compares times with another headset

using System;

namespace Game.Core
{
    // Teleport charges
    // Max charges are set per character (Ray 3, Cluster 6, Ring 4)
    public class TeleportCharging
    {
        public int MaxCharges { get; }
        public float RechargeSeconds { get; }  // one charge comes back every this many seconds (4.5)
        public float MinGapSeconds { get; }    // shortest time allowed between two teleports (0.3)

        public int Charges { get; private set; }

        // When the current recharge started, the HUD uses it for the "next charge" bar
        public double RechargeStartTime { get; private set; }

        // Time of the last teleport
        // Starts at minus infinity so the first teleport is never blocked by the gap
        public double LastTeleportTime { get; private set; } = double.NegativeInfinity;

        public TeleportCharging(int maxCharges, float rechargeSeconds, float minGapSeconds)
        {
            MaxCharges = maxCharges;
            RechargeSeconds = rechargeSeconds;
            MinGapSeconds = minGapSeconds;
        }

        // On spawn: full charges, the recharge timer starts at now
        // TODO(Ali) [X] Completed
        public void Reset(double now)
        {
            Charges = MaxCharges;
            RechargeStartTime = now;
        }

        // Called every frame on the host
        // While Charges is below MaxCharges:
        //   every full RechargeSeconds since RechargeStartTime adds 1 charge
        //   and moves RechargeStartTime forward by RechargeSeconds
        // At MaxCharges: RechargeStartTime = now, so the next recharge starts fresh after a teleport
        // Returns true if Charges changed, so the host knows to sync it
        // We need to make sure a long pause gives several charges at once (a loop, not a single if), but never more than MaxCharges
        // TODO(Ali) [X] Completed
        public bool Tick(double now)
        {
            bool changed = false;
            while (Charges < MaxCharges && RechargeSeconds > 0f && now - RechargeStartTime >= RechargeSeconds)
            {
                Charges++;
                RechargeStartTime += RechargeSeconds;
                changed = true;
            }
            if (Charges >= MaxCharges)
            {
                RechargeStartTime = now;
            }
            return changed;
        }

        // True if there is at least 1 charge and MinGapSeconds have passed since the last teleport
        // TODO(Ali) [X] Completed
        public bool CanSpend(double now)
        {
            return Charges >= 1 && now - LastTeleportTime >= MinGapSeconds;
        }

        // If CanSpend: Charges - 1, LastTeleportTime = now
        // If we were at MaxCharges before spending, the recharge starts now (RechargeStartTime = now)
        // Returns true if the charge was spent
        // TODO(Ali) [X] Completed
        public bool TrySpend(double now)
        {
            if (!CanSpend(now))
            {
                return false;
            }
            bool wasFull = Charges >= MaxCharges;
            Charges--;
            LastTeleportTime = now;
            if (wasFull)
            {
                RechargeStartTime = now;
            }
            return true;
        }

        // Kill bonus: +1 charge, never above MaxCharges
        // The recharge progress keeps going, a kill doesn't reset it
        // Returns true if a charge was added, false if already full (the bonus is lost)
        // TODO(Ali) [X] Completed
        public bool AddCharge()
        {
            if (Charges >= MaxCharges)
            {
                return false;
            }
            Charges++;
            return true;
        }

        // 0 to 1 progress toward the next charge, 1 when full, the HUD lights use it
        // TODO(Ali) [X] Completed
        public float NextChargeProgress(double now)
        {
            if (Charges >= MaxCharges || RechargeSeconds <= 0f)
            {
                return 1f;
            }
            double progress = (now - RechargeStartTime) / RechargeSeconds;
            return (float)Math.Max(0.0, Math.Min(1.0, progress));
        }
    }

    // Laser energy
    // Max energy, cost per shot, regen and shot gap are set in the weapon data
    public class LaserEnergyCharging
    {
        public float MaxEnergy { get; }
        public float CostPerShot { get; }
        public float RegenPerSecond { get; }
        public float RegenDelay { get; }   // seconds without shooting before energy starts coming back
        public float MinShotGap { get; }   // shortest time allowed between two shots, this is the fire rate

        public float Energy { get; private set; }

        // Time of the last shot, starts at minus infinity so the first shot and regen are never blocked
        public double LastShotTime { get; private set; } = double.NegativeInfinity;

        public LaserEnergyCharging(float maxEnergy, float costPerShot, float regenPerSecond, float regenDelay, float minShotGap)
        {
            MaxEnergy = maxEnergy;
            CostPerShot = costPerShot;
            RegenPerSecond = regenPerSecond;
            RegenDelay = regenDelay;
            MinShotGap = minShotGap;
        }

        // On spawn: full energy
        // TODO(Ali) [X] Completed
        public void Reset()
        {
            Energy = MaxEnergy;
        }

        // Called every frame on the shooter's own headset
        // If RegenDelay seconds have passed since the last shot: Energy + RegenPerSecond * deltaSeconds, never above MaxEnergy
        // Returns true if Energy changed
        // We need to make sure a full bar returns false, otherwise the host would sync it every frame for nothing
        // TODO(Ali) [X] Completed
        public bool Tick(double now, float deltaSeconds)
        {
            if (Energy >= MaxEnergy)
            {
                return false;
            }
            if (now - LastShotTime < RegenDelay)
            {
                return false;
            }
            Energy = Math.Min(MaxEnergy, Energy + RegenPerSecond * deltaSeconds);
            return true;
        }

        // True if Energy is at least CostPerShot and MinShotGap has passed since the last shot
        // We need to make sure the energy check allows a tiny margin (Energy >= CostPerShot - 0.001)
        // 100 / 30 is not an exact decimal, after 29 shots the bar can be a hair below the cost and the 30th shot would fail
        // TODO(Ali) [X] Completed
        public bool CanFire(double now)
        {
            return Energy >= CostPerShot - 0.001f && now - LastShotTime >= MinShotGap;
        }

        // If CanFire: Energy - CostPerShot, LastShotTime = now, return true
        // Otherwise nothing changes and it returns false (the gun clicks empty)
        // TODO(Ali) [X] Completed
        public bool TryFire(double now)
        {
            if (!CanFire(now))
            {
                return false;
            }
            Energy = Math.Max(0f, Energy - CostPerShot);
            LastShotTime = now;
            return true;
        }

        // 0 to 1 for the energy bar on the gun
        // TODO(Ali) [X] Completed
        public float Fraction
        {
            get
            {
                if (MaxEnergy <= 0f)
                {
                    return 0f;
                }
                return Energy / MaxEnergy;
            }
        }
    }
}
