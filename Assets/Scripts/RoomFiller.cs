using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomFiller
{
    /* These are probabilities. For easy - medium - hard */
    private static float[] _difficultyRanges = { 0.3f, 0.5f, 0.2f };
    private static float _minMonsterNumDivider = 5.0f;
    private static float _maxMonsterNumDivider = 4.0f;

    private static List<List<Indexes>> _monsterIndexes = new List<List<Indexes>>();
    private static List<int> _roomIds = new List<int>();

    public static void InitData()
    {
        _monsterIndexes = new List<List<Indexes>>();
        _roomIds = new List<int>();
    }
}
