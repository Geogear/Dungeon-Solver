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

    private int _currentMaxEdge = 69; //5
    private int _currentMinEdge = 31; //3

    public static System.Random rand = null;

    public UnityEngine.Tilemaps.Tile _normalTile;
    public UnityEngine.Tilemaps.Tile _doorTile;
    public UnityEngine.Tilemaps.Tile _exitTile;
    public UnityEngine.Tilemaps.Tile _genericWallTile;
    public UnityEngine.Tilemaps.Tilemap _tileMap;

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
        GenerateLevel();
        PrintDungeonMatrix();
        VisualizeDungeon();
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
        Room.OpenNewDictionary();

        /* Init entering room. */
        _enteringRoom = new Room();
        _enteringRoom.MakeThisEnteringRoom();

        /* After generation ends, creates and fills the dungeon matrix. */
        CreateDungeonMatrix();
    }

    public void PrintDungeonMatrix()
    {
        if (_dungeonMatrix != null)
        {
            for (int i = 0; i < _dungeonSize.i; ++i)
            {
                string str = "";
                for (int j = 0; j < _dungeonSize.j; ++j)
                {
                    switch (_dungeonMatrix[i, j])
                    {
                        case -1:
                            str += 'E';
                            break;
                        case 0:
                            str += 'N';
                            break;
                        case 1:
                            str += 'D';
                            break;
                        case 2:
                            str += 'X';
                            break;
                    }
                    str += '\t';
                }
                Debug.Log(str);
            }
            Debug.Log("room num:" + Room.GetNumOfRooms());
        }
    }

    public static int GetWeightedRandom(int [] weights)
    {
        int total = 0, randomTotal = 0;
        foreach(int x in weights)
        {
            total += x;
        }

        randomTotal = rand.Next(total);

        for(int i = 0; i < weights.Length; ++i)
        {
            if (randomTotal < weights[i])
            {
                return i;
            }
            randomTotal -= weights[i];
        }
        /* This shouldn't happen */
        return 0;
    }

    public static int GetCurSeed() => _currentSeed;
    public static int GetCurHeight() => _currentDungeonHeight;
    public static int GetCurWidth() => _currentDungeonWidth;

    private void VisualizeDungeon()
    {
        /* TODO, for walls, doors and such. Need a tilemapcollider2d, and non-collidable tiles have to have,
         collider type as none, whereas the others have to have collider type, sprite or grid, don't know which
         one would be better, yet. */
        Vector3Int curCell = new Vector3Int();
        bool exitNotFound = true;
        UnityEngine.Tilemaps.Tile tileToPut = null;
         for (int i = 0; i < _dungeonSize.i; ++i)
        {
            curCell.y = i;
            for (int j = 0; j < _dungeonSize.j; ++j)
            {
                curCell.x = j;
                if (_dungeonMatrix[i, j] != -1)
                {
                    switch (_dungeonMatrix[i, j])
                    {
                        case (int)Tile.Normal:
                            tileToPut = _normalTile;
                            break;
                        case (int)Tile.DoorTile:
                            tileToPut = _doorTile;
                            break;
                        case (int)Tile.ExitTile:
                            tileToPut = _exitTile;
                            break;
                    }
                    _tileMap.SetTile(curCell, tileToPut);
                }
            }
        }

        for (int i = 0; i < _dungeonSize.i; ++i)
        {
            curCell.y = i;
            for (int j = 0; j < _dungeonSize.j; ++j)
            {
                curCell.x = j;
                if (_dungeonMatrix[i ,j] == -1)
                {
                    /* If one of the four direction is a door tile and the opposing direction is
                     * eiter normal, door or exit tile. */
                    if (DoesHaveOppositeNeihgbour(i, j, Tile.DoorTile, (int)Tile.Normal)
                        || DoesHaveOppositeNeihgbour(i, j, Tile.DoorTile, (int)Tile.ExitTile)
                        || DoesHaveOppositeNeihgbour(i, j, Tile.DoorTile, (int)Tile.DoorTile))
                    {
                        _tileMap.SetTile(curCell, _normalTile);
                    }/* If any of the eight neighbours is a normal tile */
                    else if (IsNeighbourToTile(i, j, Tile.Normal, false))
                    {
                        _tileMap.SetTile(curCell, _genericWallTile);
                    }else if (exitNotFound && IsNeighbourToTile(i, j, Tile.ExitTile, false))
                    {
                        exitNotFound = false;
                        _tileMap.SetTile(curCell, _exitTile);
                    }
                }
            }
        }
    }

    private bool DoesHaveOppositeNeihgbour(int i, int j, Tile tile, int oppositeTile)
    {
        int increaseI = 1, increaseJ = 1,
            decreaseI = 1, decreaseJ = 1;

        if (i == 0)
        {
            decreaseI = 0;
        }
        else if (i == _dungeonSize.i - 1)
        {
            increaseI = 0;
        }

        if (j == 0)
        {
            decreaseJ = 0;
        }
        else if (j == _dungeonSize.j - 1)
        {
            increaseJ = 0;
        }

        if (increaseI == 1 && decreaseI == 1)
        {
            if ((_dungeonMatrix[i + increaseI, j] == (int)tile && _dungeonMatrix[i - decreaseI, j] == oppositeTile)
                || (_dungeonMatrix[i + increaseI, j] == oppositeTile && _dungeonMatrix[i - decreaseI, j] == (int)tile))
            {
                return true;
            }
        }

        if (increaseJ == 1 && decreaseJ == 1)
        {
            if ((_dungeonMatrix[i, j + increaseJ] == (int)tile && _dungeonMatrix[i, j - decreaseJ] == oppositeTile)
                || (_dungeonMatrix[i, j + increaseJ] == oppositeTile && _dungeonMatrix[i, j - decreaseJ] == (int)tile))
            {
                return true;
            }
        }

        return false;
    }

    /* Returns true if index neihgbours the specified tile. */
    private bool IsNeighbourToTile(int i, int j, Tile tile, bool fourNeighbourMode = true)
    {
        int increaseI = 1, increaseJ = 1, /* TODO, Using increaseI etc. like this creates problems, or does it ?*/
            decreaseI = 1, decreaseJ = 1;

        if (i == 0)
        {
            decreaseI = 0;
        }else if (i == _dungeonSize.i - 1)
        {
            increaseI = 0;
        }

        if (j == 0)
        {
            decreaseJ = 0;
        }
        else if (j == _dungeonSize.j - 1)
        {
            increaseJ = 0;
        }

        return ((_dungeonMatrix[i, j - decreaseJ] == (int)tile || _dungeonMatrix[i, j + increaseJ] == (int)tile
            || _dungeonMatrix[i + increaseI, j] == (int)tile || _dungeonMatrix[i - decreaseI, j] == (int)tile)
            || (!fourNeighbourMode && (
            _dungeonMatrix[i + increaseI, j - decreaseJ] == (int)tile || _dungeonMatrix[i + increaseI, j + increaseJ] == (int)tile
            || _dungeonMatrix[i - decreaseI, j - decreaseJ] == (int)tile || _dungeonMatrix[i - decreaseI, j + increaseJ] == (int)tile)));
    }

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
                    childDoorDungeonIndexes.i += 2;
                    break;
                case Direction.Down:
                    childDoorDungeonIndexes.i -= 2;
                    break;
                case Direction.Left:
                    childDoorDungeonIndexes.j += 2;
                    break;
                case Direction.Right:
                    childDoorDungeonIndexes.j -= 2;
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
