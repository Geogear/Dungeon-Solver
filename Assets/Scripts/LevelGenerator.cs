using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelGenerator : MonoBehaviour
{
    /* 1 for tile, 2 for door. The room is a rectangle and can rotate 90 degrees. */
    private readonly int[] EntranceRoom =
        {1, 2, 1,
         1, 1, 1};
    private readonly int[] ExitRoom =
        {1, 2, 1,
         1, 1, 1};

    private const int DungeonFirstMaxEdge = 5;
    private const int DungeonFirstMinEdge = 3;

    private int _currentMaxEdge = 5;
    private int _currentMinEdge = 3;

    private readonly int [] EdgeIncreaseAmount =
        {1, 2, 3, 4};
    private readonly int[] EdgeIncreaseAmountWeights =
        {20, 50, 20, 10};

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void GenerateLevel()
    {

    }

    private void AmplifyEdges()
    {
        /* Max edge should never be lower than min edge, 
         * or min edge should never surpass max edge, according to amplifyng order,
         * those two are different things */
        var rand = new System.Random();
        int otherIncreaseAmount = 0;
        if (rand.Next() % 2 == 0)
        {
            _currentMaxEdge += EdgeIncreaseAmount[GetWeightedRandom(EdgeIncreaseAmountWeights)];
            otherIncreaseAmount = EdgeIncreaseAmount[GetWeightedRandom(EdgeIncreaseAmountWeights)];
            while(_currentMinEdge + otherIncreaseAmount >= _currentMaxEdge)
            {
                --otherIncreaseAmount;
            }
            _currentMinEdge += otherIncreaseAmount;
        }
        else
        {
            _currentMinEdge += EdgeIncreaseAmount[GetWeightedRandom(EdgeIncreaseAmountWeights)];
            otherIncreaseAmount = EdgeIncreaseAmount[GetWeightedRandom(EdgeIncreaseAmountWeights)];
            while(_currentMaxEdge + otherIncreaseAmount <= _currentMinEdge)
            {
                ++otherIncreaseAmount;
            }
            _currentMaxEdge += otherIncreaseAmount;
        }
    }

    public int GetWeightedRandom(int [] weights)
    {
        int total = 0;
        foreach(int x in weights)
        {
            total += x;
        }

        for(int i = 0; i < weights.Length; ++i)
        {
            if (total < weights[i])
            {
                return i;
            }
            total -= weights[i];
        }
        /* This shouldn't happen */
        return 0;
    }
}
