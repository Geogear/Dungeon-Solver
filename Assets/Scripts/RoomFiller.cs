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
    private static float _minMonsterNumDivider = 7.0f;
    private static float _maxMonsterNumDivider = 4.0f;
    private static float _minTreasureNumDivider = 11.0f;
    private static float _maxTreasureNumDivider = 7.0f;
    private static int[] _difficultyWeights = { 30, 55, 15 };

    public static int [,] _filledTypes = null;

    public static void FillRoom(Indexes roomEdges)
    {
        float roomTileCount = roomEdges.j * roomEdges.i;
        int difficultyIndex = LevelGenerator.GetWeightedRandom(_difficultyWeights);
        float minBoundry = roomTileCount / _minMonsterNumDivider,
        maxBoundry = roomTileCount / _maxMonsterNumDivider,
        rangeFixer = 0.0f, difference = maxBoundry - minBoundry;

        /* Determining range fixer for min boundry. */
        rangeFixer = DetermineRangeFixer(_difficultyRanges, difficultyIndex, true);
        minBoundry += difference * rangeFixer;

        /* Determining range fixer for max boundry. */
        rangeFixer = DetermineRangeFixer(_difficultyRanges, difficultyIndex, false);
        maxBoundry -= difference * rangeFixer;

        /* Determine monster count. */
        float monsterCount = LevelGenerator.RandomFloat(minBoundry, maxBoundry);

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

        _filledTypes = new int[roomEdges.i, roomEdges.j];
        for(; monsterCount > 0 && allIndexes.Count > 0; --monsterCount)
        {
            int randIndex = LevelGenerator.rand.Next(allIndexes.Count);
            _filledTypes[allIndexes[randIndex].i, allIndexes[randIndex].j] = (int)FilledType.MonsterGoblin;
            allIndexes.RemoveAt(randIndex);
        }
    }

    private static float DetermineRangeFixer(float [] difficultyRanges, int difficultyIndex, bool forMinRange)
    {
        float rangeFixer = 0.0f;
        if (forMinRange)
        {
            /* Determining range fixer for min boundry. */
            for (int i = 0; i < difficultyIndex; ++i)
            {
                rangeFixer += difficultyRanges[i];
            }
            return rangeFixer;
        }

        /* Determining range fixer for max boundry. */
        for (int i = difficultyRanges.Length - 1; i > difficultyIndex; --i)
        {
            rangeFixer += difficultyRanges[i];
        }
        return rangeFixer;
    }

}