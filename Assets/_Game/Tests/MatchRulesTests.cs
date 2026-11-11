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
    // TODO(Karim) [X] Completed
    [Test]
    public void Match_FirstToKillLimitWins()
    {
        MatchResult result = MatchRules.CheckWin(Kills(15, 10), 15, 100.0, 540.0, 99.0, 0.15f);
        Assert.AreEqual(MatchEndKind.KillLimit, result.endKind);
        Assert.AreEqual(A, result.winnerClientId);
    }

    // A on 15, last kill 0.1 s ago: not ended yet (a trade may still land)
    // TODO(Karim) [X] Completed
    [Test]
    public void Match_NoEndWithinTradeWindow()
    {
        MatchResult result = MatchRules.CheckWin(Kills(15, 10), 15, 100.0, 540.0, 99.9, 0.15f);
        Assert.AreEqual(MatchEndKind.NotEnded, result.endKind);
    }

    // A and B both on 15 (a trade on match point): not ended, sudden death
    // TODO(Karim) [X] Completed
    [Test]
    public void Match_TradeOnMatchPointGoesToSuddenDeath()
    {
        MatchResult result = MatchRules.CheckWin(Kills(15, 15), 15, 100.0, 540.0, 90.0, 0.15f);
        Assert.AreEqual(MatchEndKind.NotEnded, result.endKind);
    }

    // Time is up, A on 9, B on 7: ended, TimeLimit, A wins
    // TODO(Karim) [X] Completed
    [Test]
    public void Match_TimeUp_LeaderWins()
    {
        MatchResult result = MatchRules.CheckWin(Kills(9, 7), 15, 541.0, 540.0, 500.0, 0.15f);
        Assert.AreEqual(MatchEndKind.TimeLimit, result.endKind);
        Assert.AreEqual(A, result.winnerClientId);
    }

    // Time is up, A and B both on 9: not ended
    // TODO(Karim) [X] Completed
    [Test]
    public void Match_TimeUp_TieKeepsPlaying()
    {
        MatchResult result = MatchRules.CheckWin(Kills(9, 9), 15, 541.0, 540.0, 500.0, 0.15f);
        Assert.AreEqual(MatchEndKind.NotEnded, result.endKind);
    }

    // Time is up, then A gets a kill (10 vs 9) after endTime: ended, SuddenDeath, A wins
    // TODO(Karim) [X] Completed
    [Test]
    public void Match_SuddenDeathKillEndsIt()
    {
        MatchResult result = MatchRules.CheckWin(Kills(10, 9), 15, 560.0, 540.0, 550.0, 0.15f);
        Assert.AreEqual(MatchEndKind.SuddenDeath, result.endKind);
        Assert.AreEqual(A, result.winnerClientId);
    }


    // SpawnSelector (Karim)

    // One enemy in a corner: the pick is the spawn farthest from it
    // TODO(Karim) [X] Completed
    [Test]
    public void Spawn_PicksFarthestFromEnemies()
    {
        List<GridCoord> spawns = new List<GridCoord> { new GridCoord(0, 0, 0), new GridCoord(2, 0, 2), new GridCoord(5, 0, 5) };
        List<GridCoord> enemies = new List<GridCoord> { new GridCoord(0, 0, 0) };
        Assert.AreEqual(new GridCoord(5, 0, 5), SpawnSelector.Pick(spawns, enemies, TestMaps.Nobody));
    }

    // Two spawns equally far from the enemy: the one earlier in the list (better rank) wins
    // TODO(Karim) [X] Completed
    [Test]
    public void Spawn_TieGoesToBestRank()
    {
        List<GridCoord> spawns = new List<GridCoord> { new GridCoord(5, 0, 0), new GridCoord(0, 0, 5) };
        List<GridCoord> enemies = new List<GridCoord> { new GridCoord(0, 0, 0) };
        Assert.AreEqual(new GridCoord(5, 0, 0), SpawnSelector.Pick(spawns, enemies, TestMaps.Nobody));
    }

    // The farthest spawn is occupied: the next farthest is picked
    // TODO(Karim) [X] Completed
    [Test]
    public void Spawn_SkipsOccupiedSpawns()
    {
        List<GridCoord> spawns = new List<GridCoord> { new GridCoord(0, 0, 0), new GridCoord(2, 0, 2), new GridCoord(5, 0, 5) };
        List<GridCoord> enemies = new List<GridCoord> { new GridCoord(0, 0, 0) };
        GridCoord taken = new GridCoord(5, 0, 5);
        Assert.AreEqual(new GridCoord(2, 0, 2), SpawnSelector.Pick(spawns, enemies, coord => coord == taken));
    }

    // No living enemies: the first free spawn (rank 1) is picked
    // TODO(Karim) [X] Completed
    [Test]
    public void Spawn_NoEnemiesGivesRankOne()
    {
        List<GridCoord> spawns = new List<GridCoord> { new GridCoord(3, 0, 3), new GridCoord(1, 0, 1) };
        Assert.AreEqual(new GridCoord(3, 0, 3), SpawnSelector.Pick(spawns, new List<GridCoord>(), TestMaps.Nobody));
    }

    // Every spawn occupied: null (only happens if something is broken)
    // TODO(Karim) [X] Completed
    [Test]
    public void Spawn_ReturnsNullWhenAllOccupied()
    {
        List<GridCoord> spawns = new List<GridCoord> { new GridCoord(3, 0, 3), new GridCoord(1, 0, 1) };
        Assert.IsNull(SpawnSelector.Pick(spawns, new List<GridCoord>(), coord => true));
    }
}
