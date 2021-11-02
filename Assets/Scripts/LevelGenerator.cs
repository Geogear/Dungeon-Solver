using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    /* Using separate matrix for each room,
     + Dungeon size is a minimum requirement
     + Each newly created room has a chance to have the door to the exit room, this chance will increase at
     each new room creation
     + Each room can have upto some number of doors according to its size. But if the dungeon minimum size requirement is met,
     it won't have any doors, other than its entering door.
     + Entering door of each room, belongs to its parent room as a door count.
     + Relation between the rooms can be kept as a tree.
     + Room has a tile matrix and a rotation data.
     + Each two room, connected by the door, share two neihgbour tiles, parent room knows the door tile's coordinate
     and the direction the door faces, thus child room learns about its door tile's coordinate.
     + The room decides where is the entering door exactly on its own matrix, but the door tile's real world coordinates are fixed by its parent.
     
     + Make sure that exiting room always exists.
     + Teleport inside the dungeon to make travel easier (later addition)
     + Exiting room has a trap door that leads to a lower level.
     + Keep it simple, at max a room can have 3 children */
    
    private static readonly int[] EntranceRoom =
        {1, 2, 1,
         1, 1, 1};
    private static readonly int[] ExitRoom =
        {1, 2, 1,
         1, 1, 1};

    private static readonly int [] EdgeIncreaseAmount =
        {1, 2, 3, 4};
    private static readonly int[] EdgeIncreaseAmountWeights =
        {20, 50, 20, 10};

    private int _currentMaxEdge = 5;
    private int _currentMinEdge = 3;

    public static System.Random rand = new System.Random();
    public static int _latestSeed = 0;

    private static bool _seeded = false;
    private static int _currentDungeonHeight = -1;
    private static int _currentDungeonWidth = -1;

    // Start is called before the first frame update
    void Start()
    {
        if (!_seeded)
        {
            _seeded = true;
            if (_latestSeed != 0)
            {
                rand = new System.Random(_latestSeed);
            }
        }
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

    public static int GetCurHeight() => _currentDungeonHeight;
    public static int GetCurWidth() => _currentDungeonWidth;
}
