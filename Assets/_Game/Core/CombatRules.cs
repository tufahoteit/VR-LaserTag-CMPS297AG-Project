// CombatRules.cs
// Owner: Moustafa
// Layer: Core, pure C#, no Unity
//
// The combat maths, as small functions with no state
// Every combat rule lives here so it can be tested without Unity
//
// Who runs this
// Only the host runs these functions, the host is the match server (it is also one of the players' headsets)
// PlayerCombat on the host calls them and stores the results (HP, deaths, kills), then syncs them to everyone
// Other headsets never call this file, they only show the numbers the host sends
//   shooter's headset: raycasts its laser and reports "I hit B in the head"
//   host: uses this file to decide if the shot counts, the damage, the new HP, death and the kill
// This file itself stores nothing and never touches the network, it only calculates and returns answers
//
// The rules (numbers live in WeaponDefinition and MatchSettings)
// HP: 100
// Damage: body 25, head 50, so every headshot saves one shot
//   4 body = 0.75 s, 1 head + 2 body = 0.5 s, 2 heads = 0.25 s (semi-auto, one shot every 0.25 s)
//   a headshot never kills from full HP in one shot (WeaponDefinition checks that headDamage is below maxHp)
// Health regen like Call of Duty: after 3.5 seconds without damage, HP refills from 0 to full in 2.5 seconds
//   any damage restarts the delay
// Trades: a shot from a player who died at most TradeWindow seconds ago (0.15) still counts
//   so two players who shoot each other at the same moment both die
// Kill credit: the last hit gets the kill (the attacker of the hit that takes HP to 0)
//   a hit on a player who is already dead is ignored, so nobody gets a second kill
// Spawn protection: 3 seconds of no damage after a respawn or the match start
//   it ends early the moment the protected player acts: their first shot or their first teleport
//
// HP is a float everywhere, regen adds a little every frame and whole numbers would round it away

using System;

namespace Game.Core
{
    public static class CombatRules
    {
        // Damage for one hit: headDamage if headshot, otherwise bodyDamage
        // TODO(Moustafa) [X] Completed
        public static float Damage(bool headshot, float bodyDamage, float headDamage)
        {
            return headshot ? headDamage : bodyDamage;
        }

        // Should the host accept a shot from this shooter
        //   shooter alive: yes
        //   shooter dead: only if now - shooterDeathTime is at most tradeWindow, this is the trade rule
        // now is the time the host received the shot, both shots of a real trade arrive within a few milliseconds
        // TODO(Moustafa) [X] Completed
        public static bool ShotCounts(bool shooterAlive, double shooterDeathTime, double now, float tradeWindow)
        {
            if (shooterAlive)
            {
                return true;
            }
            return now - shooterDeathTime <= tradeWindow;
        }

        // True while the player is still spawn protected, damage to them is ignored
        // Protection lasts protectSeconds (3) after a respawn or the match start, a teleport never gives it
        // It ends early the moment the player acts (shoots or teleports), so a protected player can't fight or reposition for free
        // TODO(Moustafa) [X] Completed
        public static bool IsSpawnProtected(double spawnTime, double now, float protectSeconds, bool hasActedSinceSpawn)
        {
            if (hasActedSinceSpawn)
            {
                return false;
            }
            return now - spawnTime < protectSeconds;
        }

        // New HP after this frame's regen
        //   if now - lastDamageTime is less than regenDelay: unchanged
        //   otherwise: hp + (maxHp / timeToFull) * deltaSeconds, never above maxHp
        // TODO(Moustafa) [X] Completed
        public static float RegenHp(float hp, float maxHp, double now, double lastDamageTime,
                                    float regenDelay, float timeToFull, float deltaSeconds)
        {
            if (now - lastDamageTime < regenDelay)
            {
                return hp;
            }
            if (timeToFull <= 0f)
            {
                return maxHp;
            }
            float regen = hp + (maxHp / timeToFull) * deltaSeconds;
            return Math.Min(maxHp, regen);
        }

        // Safety checks for a hit the shooter reported
        // They mainly catch our own bugs (wrong gun position, late report after a teleport), they are not real anti-cheat
        // They run in every build, PlayerCombat logs a warning whenever one refuses a shot so we notice the bug

        // False if the hit point is farther from the shot origin than weaponRange + tolerance
        // TODO(Moustafa) [X] Completed
        public static bool IsHitInRange(float originToHitDistance, float weaponRange, float tolerance)
        {
            return originToHitDistance <= weaponRange + tolerance;
        }

        // False if the shot started farther than tolerance (about 1.5 m) from where the host last saw the shooter's head
        // The gun is in your hand, so a real shot always starts within arm's reach of your head
        // TODO(Moustafa) [X] Completed
        public static bool IsOriginNearShooter(float originToShooterHeadDistance, float tolerance)
        {
            return originToShooterHeadDistance <= tolerance;
        }
    }
}
