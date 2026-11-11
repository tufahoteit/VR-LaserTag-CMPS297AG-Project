// MatchRules.cs
// Owner: Karim
// Layer: Core, pure C#, no Unity
//
// The match rules, as small functions with no state
// Only the host runs these, MatchManager calls them and syncs the results to everyone
//
// Winning
// Free-for-all kill race: the first player to 15 kills wins
// If the time limit runs out, the player with the most kills wins
// Tie when time runs out: sudden death, the match keeps going and the next kill wins
// Trades on match point: the match never ends within TradeWindow (0.15 s) after a kill
//   so a trade shot can still land, if both players end up on 15 it is a tie and sudden death starts
//
// Spawning
// Players respawn on predetermined spawn pads (the brief requires this)
// The pad picked is the free one farthest from every living enemy, on a tie the best spawn rank wins
// A map has at least 8 spawn pads and at most 4 other players are alive, so a free spawn pad always exists
//
// Style exception
// MatchPhase does not end in Kind, "MatchPhaseKind" reads badly for "which phase are we in"
// MatchResult uses public fields, it is a small answer handed back to MatchManager, never saved and never sent

using System;
using System.Collections.Generic;

namespace Game.Core
{
    // The phases of a match, MatchManager moves through them in this order
    public enum MatchPhase : byte
    {
        Lobby,      // everyone in the lobby area picking characters, the host can press Start
        Countdown,  // 5 seconds before everyone spawns
        Playing,
        Ended       // results shown, the host can press Restart
    }

    // Why the match ended
    public enum MatchEndKind : byte
    {
        NotEnded,
        KillLimit,    // someone reached the kill limit
        TimeLimit,    // time ran out with one clear leader
        SuddenDeath   // time ran out on a tie, then someone got the next kill
    }

    // What CheckWin returns
    public struct MatchResult
    {
        public MatchEndKind endKind;
        public ulong winnerClientId;  // only valid when endKind is not NotEnded
    }

    public static class MatchRules
    {
        // Is the match over, and who won
        // kills: clientId -> kill count, for every player in the match
        // lastKillTime: when the host counted the most recent kill
        // Steps:
        //   if now - lastKillTime is at most tradeWindow: not ended yet, a trade may still land
        //   find the highest kill count and how many players have it
        //   more than one player on the highest count: not ended (a tie never ends the match)
        //   the single leader has killsToWin or more: ended, KillLimit
        //   now is at or past endTime: ended, the single leader wins
        //     TimeLimit if lastKillTime is before endTime, SuddenDeath if the deciding kill came after it
        //   otherwise: not ended
        // We need to make sure a tie never ends the match, that rule alone is what makes sudden death work
        // MatchManager calls this every frame while Playing
        // TODO(Karim) [X] Completed
        public static MatchResult CheckWin(IReadOnlyDictionary<ulong, int> kills, int killsToWin,
                                           double now, double endTime, double lastKillTime, float tradeWindow)
        {
            MatchResult notEnded = new MatchResult();
            notEnded.endKind = MatchEndKind.NotEnded;

            if (now - lastKillTime <= tradeWindow)
            {
                return notEnded;
            }
            if (kills == null || kills.Count == 0)
            {
                return notEnded;
            }

            int best = int.MinValue;
            int playersOnBest = 0;
            ulong leader = 0;
            foreach (KeyValuePair<ulong, int> entry in kills)
            {
                if (entry.Value > best)
                {
                    best = entry.Value;
                    playersOnBest = 1;
                    leader = entry.Key;
                }
                else if (entry.Value == best)
                {
                    playersOnBest++;
                }
            }

            if (playersOnBest > 1)
            {
                return notEnded;
            }

            MatchResult result = new MatchResult();
            result.winnerClientId = leader;
            if (best >= killsToWin)
            {
                result.endKind = MatchEndKind.KillLimit;
                return result;
            }
            if (now >= endTime)
            {
                result.endKind = lastKillTime < endTime ? MatchEndKind.TimeLimit : MatchEndKind.SuddenDeath;
                return result;
            }
            return notEnded;
        }
    }

    public static class SpawnSelector
    {
        // Pick the spawn pad for a player who is spawning
        // spawnPads comes from MapLayout.SpawnPads(), already sorted by rank (rank 1 first)
        //   candidates: spawn pads not occupied by a living player
        //   for each candidate, find the distance to its nearest living enemy
        //     distance = the larger of GridCoord.PadDistance and the level difference (a king step in 3D)
        //   pick the candidate whose nearest enemy is farthest away
        //   tie between candidates: the first one in spawnPads, so the best rank wins
        //   no living enemies: the first free spawn pad, which is the best ranked free one
        //   every spawn pad occupied: null, this only happens if something is broken, MatchManager logs an error
        // At match start MatchManager calls this once per player in turn,
        // players already placed count as enemies, so everyone spreads out
        // TODO(Karim) [X] Completed
        public static GridCoord? Pick(IReadOnlyList<GridCoord> spawnPads,
                                      IReadOnlyList<GridCoord> livingEnemyPositions,
                                      Func<GridCoord, bool> isOccupied)
        {
            GridCoord? best = null;
            int bestDistance = -1;

            foreach (GridCoord pad in spawnPads)
            {
                if (isOccupied(pad))
                {
                    continue;
                }
                int nearest = int.MaxValue;
                foreach (GridCoord enemy in livingEnemyPositions)
                {
                    int distance = Math.Max(GridCoord.PadDistance(pad, enemy), Math.Abs(pad.level - enemy.level));
                    nearest = Math.Min(nearest, distance);
                }
                // Strictly greater keeps the first (best ranked) pad on a tie
                if (nearest > bestDistance)
                {
                    bestDistance = nearest;
                    best = pad;
                }
            }

            return best;
        }
    }
}
