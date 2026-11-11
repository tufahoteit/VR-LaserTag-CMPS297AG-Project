// MatchRulesTests.cs
// Owner: Karim
// Layer: Tests, EditMode
//
// What this tests
// Winning (kill limit, time limit, ties, sudden death, trades on match point) and picking spawn pads
//
// How to run: Window, General, Test Runner, EditMode tab, Run All
// Yellow = not written yet, green = works, red = broken (the message says which check failed)
// Test maps, geometry and "nobody there" live in TestMaps.cs, the reasons for testing are written there too

using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

public class MatchRulesTests
{
    private const ulong A = 1;
    private const ulong B = 2;

    // Kills for players A and B
    private static Dictionary<ulong, int> Kills(int a, int b)
    {
        return new Dictionary<ulong, int> { { A, a }, { B, b } };
    }

    // MatchRules (Karim)

    // A on 15, B on 10, last kill 1 s ago: ended, KillLimit, A wins
    // TODO(Karim)
    [Test]
    public void Match_FirstToKillLimitWins()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // A on 15, last kill 0.1 s ago: not ended yet (a trade may still land)
    // TODO(Karim)
    [Test]
    public void Match_NoEndWithinTradeWindow()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // A and B both on 15 (a trade on match point): not ended, sudden death
    // TODO(Karim)
    [Test]
    public void Match_TradeOnMatchPointGoesToSuddenDeath()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // Time is up, A on 9, B on 7: ended, TimeLimit, A wins
    // TODO(Karim)
    [Test]
    public void Match_TimeUp_LeaderWins()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // Time is up, A and B both on 9: not ended
    // TODO(Karim)
    [Test]
    public void Match_TimeUp_TieKeepsPlaying()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // Time is up, then A gets a kill (10 vs 9) after endTime: ended, SuddenDeath, A wins
    // TODO(Karim)
    [Test]
    public void Match_SuddenDeathKillEndsIt()
    {
        Assert.Inconclusive("TODO(Karim)");
    }


    // SpawnSelector (Karim)

    // One enemy in a corner: the pick is the spawn farthest from it
    // TODO(Karim)
    [Test]
    public void Spawn_PicksFarthestFromEnemies()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // Two spawns equally far from the enemy: the one earlier in the list (better rank) wins
    // TODO(Karim)
    [Test]
    public void Spawn_TieGoesToBestRank()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // The farthest spawn is occupied: the next farthest is picked
    // TODO(Karim)
    [Test]
    public void Spawn_SkipsOccupiedSpawns()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // No living enemies: the first free spawn (rank 1) is picked
    // TODO(Karim)
    [Test]
    public void Spawn_NoEnemiesGivesRankOne()
    {
        Assert.Inconclusive("TODO(Karim)");
    }

    // Every spawn occupied: null (only happens if something is broken)
    // TODO(Karim)
    [Test]
    public void Spawn_ReturnsNullWhenAllOccupied()
    {
        Assert.Inconclusive("TODO(Karim)");
    }
}
