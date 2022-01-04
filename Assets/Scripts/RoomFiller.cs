using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomFiller
{
    /* These are probabilities. For easy - medium - hard,
     * determines the range of the random number. 
       Using the difficulty range for both monsters and also for the
       richness degree of the treasures in that level. */
    private static readonly float[] DifficultyRanges = { 0.3f, 0.5f, 0.2f };
    private static readonly float[] TreasureAmountRange = { 0.2f, 0.5f, 0.3f };
    private static readonly float MinMonsterNumDivider = 7.0f;
    private static readonly float MaxMonsterNumDivider = 4.0f;
    private static readonly float MinTreasureNumDivider = 15.0f;
    private static readonly float MaxTreasureNumDivider = 11.0f;
    private static readonly int[] DifficultyWeights = { 30, 55, 15 };
    private static readonly List<int[]> TreasureRichnessLevelWeights = new List<int[]>
    {
        new int[] {50, 35, 15},
        new int[] {30, 50, 20},
        new int[] {20, 40, 40}
    };

    public static int [,] _filledTypes = null;

    public static void FillRoom(Indexes roomEdges, Indexes enteringDoor, List<Indexes>leadingDoors)
    {
        float roomTileCount = roomEdges.j * roomEdges.i;
        /* Set variables for monster generation. */
        int difficultyIndex = LevelGenerator.GetWeightedRandom(DifficultyWeights);
        float minBoundry = roomTileCount / MinMonsterNumDivider,
        maxBoundry = roomTileCount / MaxMonsterNumDivider,
        rangeFixer = 0.0f, difference = maxBoundry - minBoundry;

        /* Determining range fixer for min boundary. */
        rangeFixer = DetermineRangeFixer(DifficultyRanges, difficultyIndex, true);
        minBoundry += difference * rangeFixer;

        /* Determining range fixer for max boundary. */
        rangeFixer = DetermineRangeFixer(DifficultyRanges, difficultyIndex, false);
        maxBoundry -= difference * rangeFixer;

        /* Determine monster count. */
        float monsterCount = LevelGenerator.RandomFloat(minBoundry, maxBoundry);

        /* Set variables for treasure generation. */
        minBoundry = roomTileCount / MinTreasureNumDivider;
        maxBoundry = roomTileCount / MaxTreasureNumDivider;
        difference = maxBoundry - minBoundry;

        rangeFixer = DetermineRangeFixer(TreasureAmountRange, difficultyIndex, true);
        minBoundry += difference * rangeFixer;

        rangeFixer = DetermineRangeFixer(TreasureAmountRange, difficultyIndex, false);
        minBoundry += difference * rangeFixer;

        int treasureCount = Mathf.RoundToInt(LevelGenerator.RandomFloat(minBoundry, maxBoundry));
        /* Using the same treasure boundry with the traps, but making it a little bit deadlier. */
        int trapCount = Mathf.RoundToInt(LevelGenerator.RandomFloat(minBoundry, maxBoundry));
        if (trapCount == 0)
        {
            if (treasureCount > 0)
            {
                /* If zero traps in a room with treasure, give it another chance. */
                trapCount = Mathf.RoundToInt(LevelGenerator.RandomFloat(minBoundry, maxBoundry));
            }
            else
            {
                /* if no traps and treasures in the room, put a trap with %20 chance. */
                trapCount = (LevelGenerator.rand.Next() % 10 > 7) ? 1 : 0; 
            }
        }

        /* Record all existing indexes for the room to pull with rand. */
        monsterCount = Mathf.RoundToInt(monsterCount);
        List<Indexes> allIndexes = new List<Indexes>();
        for (int i = 0; i < roomEdges.i; ++i)
        {
            for (int j = 0; j < roomEdges.j; ++j)
            {
                allIndexes.Add(new Indexes(j, i));
            }
        }

        /* Fill treasures. */
        _filledTypes = new int[roomEdges.i, roomEdges.j];
        int randIndex = 0, filledType = 0, totalCount = treasureCount + trapCount,
            richness = 0;
        bool exitLoop = true, fillTrap = false;
        for (; totalCount > 0 && allIndexes.Count > 0; --totalCount)
        {
            /* Select trap or treasure to fill. */
            if(trapCount == 0)
            {
                fillTrap = false;
            }
            else if(treasureCount == 0)
            {
                fillTrap = true;
            }
            else
            {
                fillTrap = !fillTrap;
            }

            /* Currently, mathematically not possible for a treasure to not find a suitable position inside a room. 
             But better safe than sorry. */
            exitLoop = true;
            for (int failCount = 0; failCount < allIndexes.Count; ++failCount)
            {
                randIndex = LevelGenerator.rand.Next(allIndexes.Count);
                if (!leadingDoors.Contains(allIndexes[randIndex]) &&
                    (allIndexes[randIndex].i != enteringDoor.i || allIndexes[randIndex].j != enteringDoor.j))
                {
                    exitLoop = false;
                    break;
                }
            }
            if (exitLoop)
            {
                break;
            }

            /* Put trap or treasure into the selected index. */
            richness = LevelGenerator.GetWeightedRandom(TreasureRichnessLevelWeights[difficultyIndex]);
            if (fillTrap)
            {
                filledType = (int)FilledType.TrapLow + richness;
                --trapCount;
            }
            else
            {
                filledType = (int)FilledType.TreasureLow + richness;
                --treasureCount;
            }           
            _filledTypes[allIndexes[randIndex].i, allIndexes[randIndex].j] = filledType;
            allIndexes.RemoveAt(randIndex);
        }
        /* Fill goblins. */
        FillWith(allIndexes, FilledType.MonsterGoblin, (int)monsterCount);
    }

    public static float DetermineRangeFixer(float [] difficultyRanges, int difficultyIndex, bool forMinRange)
    {
        float rangeFixer = 0.0f;
        if (forMinRange)
        {
            /* Determining range fixer for min boundary. */
            for (int i = 0; i < difficultyIndex; ++i)
            {
                rangeFixer += difficultyRanges[i];
            }
            return rangeFixer;
        }

        /* Determining range fixer for max boundary. */
        for (int i = difficultyRanges.Length - 1; i > difficultyIndex; --i)
        {
            rangeFixer += difficultyRanges[i];
        }
        return rangeFixer;
    }

    private static void FillWith(List<Indexes> allIndexes, FilledType typeToFill, int amountToFill)
    {
        int intOfType = (int)typeToFill, randIndex = -1;
        for (; amountToFill > 0 && allIndexes.Count > 0; --amountToFill)
        {
            randIndex = LevelGenerator.rand.Next(allIndexes.Count);
            _filledTypes[allIndexes[randIndex].i, allIndexes[randIndex].j] = intOfType;
            allIndexes.RemoveAt(randIndex);
        }
    }
}