using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomFiller
{
    /* These are probabilities. For easy - medium - hard,
     * determines the range of the random number. 
       Using the difficulty range for both monsters and also for the
       richness degree of the treasures in that level. */
    private static float[] _difficultyRanges = { 0.3f, 0.5f, 0.2f };
    private static float[] _treasureAmountRange = { 0.2f, 0.5f, 0.3f };
    private static float _minMonsterNumDivider = 7.0f;
    private static float _maxMonsterNumDivider = 4.0f;
    private static float _minTreasureNumDivider = 15.0f;
    private static float _maxTreasureNumDivider = 11.0f;
    private static int[] _difficultyWeights = { 30, 55, 15 };
    private static List<int[]> _treasureRichnessLevelWeights = new List<int[]>
    {
        new int[] {50, 35, 15},
        new int[] {30, 50, 20},
        new int[] {20, 40, 40}
    };

    public static int [,] _filledTypes = null;

    public static void FillRoom(Indexes roomEdges)
    {
        float roomTileCount = roomEdges.j * roomEdges.i;
        /* Set variables for monster generation. */
        int difficultyIndex = LevelGenerator.GetWeightedRandom(_difficultyWeights);
        float minBoundry = roomTileCount / _minMonsterNumDivider,
        maxBoundry = roomTileCount / _maxMonsterNumDivider,
        rangeFixer = 0.0f, difference = maxBoundry - minBoundry;

        /* Determining range fixer for min boundary. */
        rangeFixer = DetermineRangeFixer(_difficultyRanges, difficultyIndex, true);
        minBoundry += difference * rangeFixer;

        /* Determining range fixer for max boundary. */
        rangeFixer = DetermineRangeFixer(_difficultyRanges, difficultyIndex, false);
        maxBoundry -= difference * rangeFixer;

        /* Determine monster count. */
        float monsterCount = LevelGenerator.RandomFloat(minBoundry, maxBoundry);

        /* Set variables for treasure generation. */
        minBoundry = roomTileCount / _minTreasureNumDivider;
        maxBoundry = roomTileCount / _maxTreasureNumDivider;
        difference = maxBoundry - minBoundry;

        rangeFixer = DetermineRangeFixer(_treasureAmountRange, difficultyIndex, true);
        minBoundry += difference * rangeFixer;

        rangeFixer = DetermineRangeFixer(_treasureAmountRange, difficultyIndex, false);
        minBoundry += difference * rangeFixer;

        int treasureCount = Mathf.RoundToInt(LevelGenerator.RandomFloat(minBoundry, maxBoundry));

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
        for (; treasureCount > 0 && allIndexes.Count > 0; --treasureCount)
        {
            int randIndex = LevelGenerator.rand.Next(allIndexes.Count);
            int treasureType = (int)FilledType.TreasureLow + LevelGenerator.GetWeightedRandom(_treasureRichnessLevelWeights[difficultyIndex]);
            _filledTypes[allIndexes[randIndex].i, allIndexes[randIndex].j] = treasureType;
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