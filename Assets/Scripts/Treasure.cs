using System.Collections.Generic;
using UnityEngine;

public static class Treasure
{
    private static Dictionary<Vector3, TreasureData> _treasures = new Dictionary<Vector3, TreasureData>();
    private static List<int[]> _treasureRichnessWeights = new List<int[]>
    {
        new int[] { 40, 25, 20, 10, 5 },
        new int[] { 30, 15, 25, 20, 10 },
        new int[] { 5, 15, 20, 40, 20 }
    };
    private static float _treasureMultiplierMin = 1.0f;
    private static float _treasureMultiplierMax = 4.0f;

    private static float DetermineActualRichness(int richnessIndex)
    {
        float weightIndex = LevelGenerator.GetWeightedRandom(_treasureRichnessWeights[richnessIndex]);
        float rangeFixer = 1.0f / _treasureRichnessWeights[richnessIndex].Length;
        return _treasureMultiplierMin +  (_treasureMultiplierMax - _treasureMultiplierMin) * rangeFixer * weightIndex;
    }

    public static void Punish(PlayerCharacter playerCharacter, Vector3 treasurePos)
    {
        /* TODO */
        Debug.Log("Punish");
    }

    public static void Reward(PlayerCharacter playerCharacter, Vector3 treasurePos)
    {
        /* TODO */
        Debug.Log("Reward");
    }

    public static void AddTreasure(Vector3 treasurePos, int richnessIndex)
    {
        float treasureMultipler = DetermineActualRichness(richnessIndex);
        _treasures.Add(treasurePos, new TreasureData(richnessIndex, treasureMultipler));
    }
}
