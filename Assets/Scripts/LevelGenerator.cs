using UnityEngine;

public class LevelGenerator : MonoBehaviour
{   
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
    private static int[,] _dungeonMatrix = null;
    private static Indexes _dungeonSize = new Indexes(0, 0);
    private static Room _enteringRoom = null;

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

        /* Init entering room. */
        _enteringRoom = new Room();
        _enteringRoom.MakeThisEnteringRoom();

        /* After generation ends, creates and fills the dungeon matrix. */
        CreateDungeonMatrix();
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

    private void CreateDungeonMatrix()
    {
        _dungeonSize = Room.GetActualDungeonSize();
        Indexes firstRoomIndexes = new Indexes(_dungeonSize.j-1, _dungeonSize.i-1);
        _dungeonSize.i *= 2; _dungeonSize.j *= 2;

        _dungeonMatrix = new int[_dungeonSize.i, _dungeonSize.j];
        for (int i = 0; i < _dungeonSize.i; ++i)
        {
            for (int j = 0; j < _dungeonSize.j; ++j)
            {
                _dungeonMatrix[i, j] = -1;
            }
        }

        PutRoom(firstRoomIndexes, _enteringRoom);
    }

    private void PutRoom(Indexes enteringDoorIndexesDungeon, Room room)
    {       
        Indexes enteringDoorIndexesRoom = room.GetEnteringIndexes();
        Indexes newOrigin = new Indexes(enteringDoorIndexesDungeon.j - enteringDoorIndexesRoom.j,
            enteringDoorIndexesDungeon.i - enteringDoorIndexesRoom.i);

        int[,] roomMatrix = room.GetRoom();
        for (int i = 0; i < room.GetRoomHeight(); ++i)
        {
            for (int j = 0; j < room.GetRoomWidth(); ++j)
            {
                _dungeonMatrix[newOrigin.i + i, newOrigin.j + j] = roomMatrix[i, j];
            }
        }
        /* For each child, call put room with appropriate params. */
        System.Collections.Generic.List<Room> childRooms = room.GetChildRooms();
        System.Collections.Generic.List<Indexes> childDoorIndexes = room.GetChildDoorIndexes();
        for (int i = 0; i < childRooms.Count; ++i)
        {
            Indexes childDoorDungeonIndexes = new Indexes(newOrigin.j + childDoorIndexes[i].j,
                newOrigin.i + childDoorIndexes[i].i);

            /* Since these directions are from the child, for the parent it's actually the reverted one. */
            switch (childRooms[i].GetEnteringDoorDirection())
            {
                case Direction.Up:
                    --childDoorDungeonIndexes.i;
                    break;
                case Direction.Down:
                    ++childDoorDungeonIndexes.i;
                    break;
                case Direction.Left:
                    ++childDoorDungeonIndexes.j;
                    break;
                case Direction.Right:
                    --childDoorDungeonIndexes.j;
                    break;
            }

            PutRoom(childDoorDungeonIndexes, childRooms[i]);
        }

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
