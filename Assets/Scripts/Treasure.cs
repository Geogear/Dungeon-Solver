using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    public int _richnessIndex = -1;

    private static List<int[]> _treasureRichnessWeights = new List<int[]>
    {
        new int[] { 40, 25, 20, 10, 5 },
        new int[] { 30, 15, 25, 20, 10 },
        new int[] { 5, 15, 20, 40, 20 }
    };
    private static float _treasureMultiplierMin = 1.0f;
    private static float _treasureMultiplierMax = 4.0f;

    private float _treasureMultiplier = _treasureMultiplierMin;

    // Start is called before the first frame update
    void Start()
    {
        DetermineActualRichness();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DetermineActualRichness()
    {
        float weightIndex = LevelGenerator.GetWeightedRandom(_treasureRichnessWeights[_richnessIndex]);
        float rangeFixer = 1.0f / _treasureRichnessWeights[0].Length;
        _treasureMultiplier += (_treasureMultiplierMax - _treasureMultiplierMin) * rangeFixer * weightIndex;
    }
}
