using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomFiller
{
    /* These are probabilities. For easy - medium - hard,
     * determines the range of the random number. */
    private static float[] _difficultyRanges = { 0.3f, 0.5f, 0.2f };
    private static float _minMonsterNumDivider = 5.0f;
    private static float _maxMonsterNumDivider = 4.0f;
    private static int[] _difficultyWeights = { 30, 55, 15 };

    private static List<List<Indexes>> _filledIndexes = new List<List<Indexes>>();
    private static List<List<FilledType>> _filledTypes = new List<List<FilledType>>();
    private static List<int> _roomIds = new List<int>();

    public static void InitData()
    {
        _filledIndexes = new List<List<Indexes>>();
        _filledTypes = new List<List<FilledType>>();
        _roomIds = new List<int>();
    }

    public static void FillRoom(int roomId, Indexes roomEdges)
    {
        _roomIds.Add(roomId);
        float roomTileCount = roomEdges.j * roomEdges.i;
        int difficultyIndex = LevelGenerator.GetWeightedRandom(_difficultyWeights);
        float minBoundry = roomTileCount / _minMonsterNumDivider,
        maxBoundry = roomTileCount / _maxMonsterNumDivider,
        rangeFixer = 0.0f, difference = maxBoundry - minBoundry;

        /* Determining range fixer for min boundry. */
        for (int i = 0; i < difficultyIndex; ++i)
        {
            rangeFixer += _difficultyRanges[i];
        }
        minBoundry += difference * rangeFixer;

        /* Determining range fixer for max boundry. */
        rangeFixer = 0.0f;
        for (int i = _difficultyRanges.Length-1; i > difficultyIndex; --i)
        {
            rangeFixer += _difficultyRanges[i];
        }
        maxBoundry -= difference * rangeFixer;

        float monsterCount = LevelGenerator.rand.Next((int)minBoundry, (int)maxBoundry+1);
        monsterCount += (float)LevelGenerator.rand.NextDouble();

        if (monsterCount < minBoundry)
        {
            monsterCount = minBoundry;
        }
        else if(monsterCount > maxBoundry){
            monsterCount = maxBoundry;
        }
        /* Adding a constant on top according to the tile count for flavour. */
        monsterCount += Mathf.RoundToInt(roomTileCount/(_maxMonsterNumDivider*_maxMonsterNumDivider));

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

        List<Indexes> selectedIndexes = new List<Indexes>();
        List<FilledType> selectedTypes = new List<FilledType>();
        for(; monsterCount > 0 && allIndexes.Count > 0; --monsterCount)
        {
            int randIndex = LevelGenerator.rand.Next(allIndexes.Count);
            selectedIndexes.Add(allIndexes[randIndex]);
            selectedTypes.Add(FilledType.Monster_Goblin);
            allIndexes.RemoveAt(randIndex);
        }

        /* Add generated lists to the outer lists. */
        _filledIndexes.Add(selectedIndexes);
        _filledTypes.Add(selectedTypes);
    }
}