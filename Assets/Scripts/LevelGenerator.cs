using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    /* TODO, 
      + All dungeon as one matrix. 
      + Level generator be able to make entering room with, different rotations. */
    
    private static readonly int[] EntranceRoom1 =
        {1, 1, 1,
         1, 1, 1};
    private static readonly int[] EntranceRoom2 =
        {1, 1,
         1, 1,
         1, 1};

    private static readonly int [] EdgeIncreaseAmount =
        {1, 2, 3, 4};
    private static readonly int[] EdgeIncreaseAmountWeights =
        {20, 50, 20, 10};

    private int _currentMaxEdge = 5;
    private int _currentMinEdge = 3;

    public static System.Random rand = null;

    private static int _currentSeed = new System.Random().Next();
    private static int _currentDungeonHeight = -1;
    private static int _currentDungeonWidth = -1;

    // Start is called before the first frame update
    void Start()
    {
        rand = new System.Random(_currentSeed);
        /* Only place where, GenerateLevel is not called with AmplifyEdges*/
        //GenerateLevel();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void GenerateLevel()
    {
        /* Calculating min total tile required for this generation */
        _currentDungeonHeight = rand.Next(_currentMinEdge, _currentMaxEdge + 1);
        _currentDungeonWidth = rand.Next(_currentMinEdge, _currentMaxEdge + 1);
        Room._minTilesRequired = _currentDungeonHeight * _currentDungeonWidth;
        Room.SetExitRoomLimit();
    }

    public static int GetWeightedRandom(int [] weights)
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

    public static int GetCurSeed() => _currentSeed;
    public static int GetCurHeight() => _currentDungeonHeight;
    public static int GetCurWidth() => _currentDungeonWidth;

    private void AmplifyEdges()
    {
        /* Max edge should never be lower than min edge, 
         * or min edge should never surpass max edge, according to amplifyng order,
         * those two are different things */
        int otherIncreaseAmount = 0;
        if (rand.Next() % 2 == 0)
        {
            _currentMaxEdge += EdgeIncreaseAmount[GetWeightedRandom(EdgeIncreaseAmountWeights)];
            otherIncreaseAmount = EdgeIncreaseAmount[GetWeightedRandom(EdgeIncreaseAmountWeights)];
            while (_currentMinEdge + otherIncreaseAmount >= _currentMaxEdge)
            {
                --otherIncreaseAmount;
            }
            _currentMinEdge += otherIncreaseAmount;
        }
        else
        {
            _currentMinEdge += EdgeIncreaseAmount[GetWeightedRandom(EdgeIncreaseAmountWeights)];
            otherIncreaseAmount = EdgeIncreaseAmount[GetWeightedRandom(EdgeIncreaseAmountWeights)];
            while (_currentMaxEdge + otherIncreaseAmount <= _currentMinEdge)
            {
                ++otherIncreaseAmount;
            }
            _currentMaxEdge += otherIncreaseAmount;
        }
    }
}
