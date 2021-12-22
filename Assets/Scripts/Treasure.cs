using System.Collections.Generic;
using UnityEngine;

public static class Treasure
{
    public static readonly List<int[]> _treasureRichnessWeights = new List<int[]>
    {
        new int[] { 40, 25, 20, 10, 5 },
        new int[] { 30, 15, 25, 20, 10 },
        new int[] { 5, 15, 20, 40, 20 }
    };

    private static Dictionary<Vector3, TreasureData> _treasures = new Dictionary<Vector3, TreasureData>();
    private static float _treasureMultiplierMin = 1.0f;
    private static float _treasureMultiplierMax = 4.0f;

    private static float DetermineActualRichness(int richnessIndex)
    {
        float weightIndex = LevelGenerator.GetWeightedRandom(_treasureRichnessWeights[richnessIndex]);
        float rangeFixer = 1.0f / _treasureRichnessWeights[richnessIndex].Length;
        return _treasureMultiplierMin +  (_treasureMultiplierMax - _treasureMultiplierMin) * rangeFixer * weightIndex;
    }

    public static void RewardOrPunish(PlayerCharacter playerCharacter, Vector3 treasurePos, bool success)
    {
        /* TODO */
        if (success)
        {
            TreasureData td;
            if (!_treasures.TryGetValue(treasurePos, out td))
            {
                Debug.LogAssertion("Treasure not found with the given position, this mustn't be possible.");
            }
            td._opened = true;
            _treasures[treasurePos] = td;
            Debug.Log("Reward richness: " + td._richnessIndex + " multiplier: " + td._treasureMultiplier);
            return;
        }
        Debug.Log("Punish");
    }

    public static void AddTreasure(Vector3 treasurePos, int richnessIndex)
    {
        float treasureMultipler = DetermineActualRichness(richnessIndex-2);
        _treasures.Add(treasurePos, new TreasureData(richnessIndex-2, treasureMultipler));
    }

    public static bool IsOpened(Vector3 treasurePos)
    {
        TreasureData td;
        if(!_treasures.TryGetValue(treasurePos, out td))
        {
            Debug.LogAssertion("Treasure not found with the given position, this mustn't be possible.");
        }
        return td._opened;
    }

    public static TreasureData GetTreasureData(Vector3 treasurePos) => _treasures[treasurePos];
}
