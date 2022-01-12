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
    private static readonly FilledType[] MonstersToSpawn =
        { FilledType.MonsterGoblin, FilledType.MonsterOrc, FilledType.MonsterOgre,
        FilledType.MonsterGolem1, FilledType.MonsterGolem2, FilledType.MonsterGolem3};
    private static readonly int[] MonsterSpawnLevelCaps = { 1, 5, 9, 13, 15, 17};
    private static readonly int SpawnRateChanger = 8;

    private static int[] _monsterSpawnRates = { SpawnRateChanger * LevelGenerator.MaxLevel, 0, 0, 0, 0, 0};
    private static int _currentSRC = SpawnRateChanger;

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
            if(treasureCount == 0)
            {
                fillTrap = true;
            }

            /* Currently, mathematically not possible for a treasure to not find a suitable position inside a room. 
             But better safe than sorry. */
            exitLoop = true;
            for (int failCount = 0; failCount < allIndexes.Count; ++failCount)
            {
                randIndex = LevelGenerator.rand.Next(allIndexes.Count);

                /* If treasure blocks the road, skip. Don't put if blocks horizontal or vertical or diagonal or butterfly. */
                Indexes tmp = new Indexes(allIndexes[randIndex].j, allIndexes[randIndex].i);
                if (!fillTrap &&
                    (
                    /* It has wall on one side, so it shouldn't have anything on the other side. */
                    (tmp.i == 0 && !allIndexes.Contains(new Indexes(tmp.j, tmp.i + 1)))
                    || (tmp.i == roomEdges.i-1 && !allIndexes.Contains(new Indexes(tmp.j, tmp.i - 1)))
                    /* It shouldn't have anything on both sides. */
                    || (tmp.i > 0 && tmp.i < roomEdges.i-1 && !allIndexes.Contains(new Indexes(tmp.j, tmp.i - 1)) && !allIndexes.Contains(new Indexes(tmp.j, tmp.i + 1)))

                    /* It has wall on one side, so it shouldn't have anything on the other side. */
                    || (tmp.j == 0 && !allIndexes.Contains(new Indexes(tmp.j +1, tmp.i)))
                    || (tmp.j == roomEdges.j - 1 && !allIndexes.Contains(new Indexes(tmp.j -1, tmp.i)))
                    /* It shouldn't have anything on both sides. */
                    || (tmp.j > 0 && tmp.j < roomEdges.j - 1 && !allIndexes.Contains(new Indexes(tmp.j - 1, tmp.i)) && !allIndexes.Contains(new Indexes(tmp.j + 1, tmp.i)))

                    /* If not on the right most side, check the right butterfly. */
                    || (tmp.j < roomEdges.j - 1 && tmp.i > 0 && tmp.i < roomEdges.i -1 &&
                        (!allIndexes.Contains(new Indexes(tmp.j+1, tmp.i - 1)) || !allIndexes.Contains(new Indexes(tmp.j+1, tmp.i + 1))))

                    /* If not on the left most side, check the left butterfly. */
                    || (tmp.j != 0 && tmp.i > 0 && tmp.i < roomEdges.i - 1 &&
                        (!allIndexes.Contains(new Indexes(tmp.j - 1, tmp.i - 1)) || !allIndexes.Contains(new Indexes(tmp.j - 1, tmp.i + 1))))

                    /* If not on the upper most side, check the upper butterfly. */
                    || (tmp.i != 0 && tmp.j > 0 && tmp.i < roomEdges.j - 1 &&
                        (!allIndexes.Contains(new Indexes(tmp.j - 1, tmp.i - 1)) || !allIndexes.Contains(new Indexes(tmp.j + 1, tmp.i - 1))))

                    /* If not on the bottom most side, check the bottom butterfly. */
                    || (tmp.i < roomEdges.i - 1 && tmp.j > 0 && tmp.i < roomEdges.j - 1 &&
                        (!allIndexes.Contains(new Indexes(tmp.j - 1, tmp.i + 1)) || !allIndexes.Contains(new Indexes(tmp.j + 1, tmp.i + 1))))

                    /* If not on the room edges, check diagonals. */
                    || (tmp.j > 0 && tmp.i > 0 && tmp.i < roomEdges.i-1 && 
                        !allIndexes.Contains(new Indexes(tmp.j - 1, tmp.i - 1)) && !allIndexes.Contains(new Indexes(tmp.j - 1, tmp.i + 1)))
                    || (tmp.j < roomEdges.j-1 && tmp.i > 0 && tmp.i < roomEdges.i-1 && 
                        !allIndexes.Contains(new Indexes(tmp.j + 1, tmp.i - 1)) && !allIndexes.Contains(new Indexes(tmp.j + 1, tmp.i + 1)))
                    )
                   )
                {
                    continue;
                }

                /* Since door indexes are not inside room indexes, have to check them separately. */
                if (!leadingDoors.Contains(allIndexes[randIndex]) && (allIndexes[randIndex].j != enteringDoor.j))
                {
                    exitLoop = false;
                    break;
                }
            }
            if (exitLoop)
            {
                if(!fillTrap)
                {
                    fillTrap = true;
                }
                else
                {
                    break;
                }             
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

        /* Decide how many to spawn, each monster. */
        int[] monsterCountByType = new int[MonstersToSpawn.Length];
        for(; monsterCount > 0; --monsterCount)
        {
            ++monsterCountByType[LevelGenerator.GetWeightedRandom(_monsterSpawnRates)];
        }

        /* Fill with monsters. */
        for(int i = 0; i < monsterCountByType.Length; ++i)
        {
            FillWith(allIndexes, MonstersToSpawn[i], monsterCountByType[i]);
        }
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

    public static void SetMonsterSpawnRates(int currentLevel)
    {
        int changeAmount = 0;
        /* Update current spawn rate changer if needed. Which is when a new cap level has been achieved. */
        for(int i = 1; i < MonsterSpawnLevelCaps.Length; ++i)
        {
            if(MonsterSpawnLevelCaps[i] == currentLevel)
            {
                _currentSRC += SpawnRateChanger;
                break;
            }
        }
        /* Loop until the last element, or until the first element that spawn level cap exceeds the cuerrent level. */
        for(int i = 0; i < MonsterSpawnLevelCaps.Length-1 && MonsterSpawnLevelCaps[i] <= currentLevel; ++i)
        {
            /* If less or equal to current spawn rate changer, skip it. */
            if(_monsterSpawnRates[i] <= _currentSRC)
            {
                continue;
            }
            /* Set the change amount in a decreased manner with the ith power of 2. First decrease yourself. */
            changeAmount = _currentSRC / ((i == 0) ? 1 : 2*i);
            _monsterSpawnRates[i] -= changeAmount;
            /* Then increase the next one. */
            _monsterSpawnRates[i + 1] += changeAmount;
        }
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

    private static bool BlocksDoor(Indexes door, Indexes index)
    {
        return (door.j == index.j && door.i == index.i) ||
            (door.j == index.j && (door.i == index.i+1 || door.i == index.i-1)) ||
            (door.i == index.i && (door.j == index.j + 1 || door.j == index.j - 1));
    }
}