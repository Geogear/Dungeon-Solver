using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    public int _richnessIndex = -1;

    private static List<int[]> _treasureRichnessWeights = new List<int[]>
    {
        new int[] { 40, 20, 20, 15, 5 },
        new int[] { 30, 15, 30, 15, 10 },
        new int[] { 5, 15, 20, 25, 35 }
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
        int weightIndex = LevelGenerator.GetWeightedRandom(_treasureRichnessWeights[_richnessIndex]);
        float rangeFixer = 0.0f;
        /* TODO, this is wrong? */
        for(int i = 0; i < weightIndex; ++i)
        {
            rangeFixer += _treasureRichnessWeights[_richnessIndex][i];
        }
        rangeFixer /= 100.0f;
        _treasureMultiplier += (_treasureMultiplierMax - _treasureMultiplierMin) * rangeFixer;
    }
}
