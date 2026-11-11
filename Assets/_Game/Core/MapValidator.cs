// MapValidator.cs
// Owner: Moustafa
// Layer: Core, pure C#, no Unity
//
// Checks a map against the map rules and lists every problem it finds
// Used by the Validate button in the editor and when generating maps (generate, validate, fix)
//
// Errors (the map is broken)
//   a pad floor is Open (a pad floor must be Solid or Hatch)
//   a Hatch pad is anchored to Center (the hole is in the middle, the player must stand beside it, not on it)
//   fewer than 8 spawn pads
//   spawn ranks have a gap or a repeat (they must be 1, 2, 3 ...)
//   a pad is exposed from fewer than 2 directions (a safe camping spot)
//   a pad can't be reached by some character
// Warnings (check it, maybe fine)
//   a pad is exposed from more than 5 directions (a death trap)
//   two spawn pads are right next to each other
// Info
//   where pads are stacked in columns (the elevators for Ray, Cluster and Ring)
//
// Exposure
// A direction is where the threat comes from, not how many pads see it
// Two shooters looking through the same window = 1 direction
// Directions are 8 horizontal sectors + "from above" + "from below" = 10 possible
// Measured for standing players (eye height), crouching to hide is not counted
//
// Geometry
// Line of sight needs real sizes (cell size, wall heights, eye height), Core has no Unity
// so MapDefinition passes them in as plain numbers (MapGeometry), all in metres

using System;
using System.Collections.Generic;

namespace Game.Core
{
    // Real sizes the validator needs, filled in by MapDefinition
    public struct MapGeometry
    {
        public float cellSize;           // 3 m
        public float levelHeight;        // 3 m
        public float padAnchorDistance;  // 0.9 m, how far an anchored pad is pushed from the cell centre
        public float eyeHeight;          // 1.65 m, standing eye height above the pad
        public float halfWallHeight;     // 1.1 to 1.3 m
        public float windowBottom;       // height of the bottom of a window opening
        public float windowTop;          // height of the top of a window opening
    }

    // Everything Validate found, as readable lines like "Pad (3,L1,2) exposed from 1 direction (needs 2 to 5)"
    public class ValidationReport
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public List<string> Info { get; } = new List<string>();

        public bool IsValid
        {
            get { return Errors.Count == 0; }
        }
    }

    public static class MapValidator
    {
        public const int MinSpawnPads = 8;
        public const int MinExposure = 2;
        public const int MaxExposure = 5;

        // Runs every check below and collects the messages
        // Order: pad floors, hatch anchors (not Center), spawns, exposure, reachability, stacked columns
        // We need to make sure one bad pad doesn't stop the other checks, collect everything, never stop at the first error
        // TODO(Moustafa)
        public static ValidationReport Validate(MapLayout map, IReadOnlyList<IMoveset> movesets, MapGeometry geometry)
        {
            return new ValidationReport();
        }

        // Spawn checks
        //   error if fewer than MinSpawnPads spawn pads
        //   error if the ranks are not exactly 1, 2, 3 ... N (sort them, each must equal its position + 1)
        //   warning for every two spawn pads with PadDistance 1 on the same level
        // TODO(Moustafa)
        public static void CheckSpawns(MapLayout map, ValidationReport report)
        {
        }

        // World position of a standing player's eyes on this pad, in metres
        //   cell centre: (x + 0.5) * cellSize, level * levelHeight, (z + 0.5) * cellSize
        //   plus MapLayout.AnchorDirection(anchor) * padAnchorDistance on the floor
        //   plus eyeHeight up
        // Returned as (x, y, z) floats, Core has no Vector3
        // We need to make sure this matches MapDefinition.PadWorldPosition, or the validator checks different spots than the game uses
        // TODO(Moustafa)
        public static (float x, float y, float z) EyePosition(MapLayout map, GridCoord pad, MapGeometry geometry)
        {
            return (0f, 0f, 0f);
        }

        // True if a standing player on pad from can see the eyes of a standing player on pad to
        // Walk along the line from EyePosition(from) to EyePosition(to) in small steps (about 0.1 m)
        // Every time the step moves into a new cell, check what was crossed:
        //   into a Solid cell: blocked
        //   a side: Solid blocks, Open passes,
        //     HalfWall blocks only if the line crosses below halfWallHeight,
        //     Window passes only if the line crosses between windowBottom and windowTop
        //   a floor or ceiling: Solid blocks, Hatch and Open pass
        // If one step crosses two sides at once (a corner), check both, blocked if either blocks
        // Players never block sight, only the map does
        // TODO(Moustafa)
        public static bool HasLineOfSight(MapLayout map, GridCoord from, GridCoord to, MapGeometry geometry)
        {
            return false;
        }

        // Number of different directions pad can be shot from (0 to 10)
        // For every other pad otherPad with HasLineOfSight(otherPad, pad):
        //   take the direction from pad to otherPad
        //   if it points more than 45 degrees up: "from above", more than 45 degrees down: "from below"
        //   otherwise one of 8 horizontal sectors of 45 degrees each
        // Count how many different buckets were hit
        // TODO(Moustafa)
        public static int CountExposureDirections(MapLayout map, GridCoord pad, MapGeometry geometry)
        {
            return 0;
        }

        // Every pad this moveset can reach starting from start, any number of jumps
        // Breadth-first search: a queue of pads, for each one add moveset.GetReachable(pad, map, coord => false)
        // isOccupied is always false here, the validator checks the empty map
        // TODO(Moustafa)
        public static HashSet<GridCoord> Reachable(MapLayout map, IMoveset moveset, GridCoord start)
        {
            return new HashSet<GridCoord>();
        }

        // Reachability check, for each moveset:
        //   run Reachable from the rank 1 spawn pad
        //   error for every pad not in the result, e.g. "Ring can't reach (4,L2,1)"
        // One search per moveset is enough: every moveset is symmetric (if A can jump to B, B can jump to A),
        // so if every pad is reachable from one spawn, every pad is reachable from every spawn
        // TODO(Moustafa)
        public static void CheckReachability(MapLayout map, IReadOnlyList<IMoveset> movesets, ValidationReport report)
        {
        }

        // Info line for every column (same x and z) that has pads on 2 or more levels
        // e.g. "Column (2,4): pads on L0, L1, L2"
        // TODO(Moustafa)
        public static void ReportStackedColumns(MapLayout map, ValidationReport report)
        {
        }
    }
}
