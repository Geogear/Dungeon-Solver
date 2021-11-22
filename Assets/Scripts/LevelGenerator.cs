using UnityEngine;

public class LevelGenerator : MonoBehaviour
{   
    /* NOTE, 1000x1000 caused stackoverflow, lel. 100x100 is a good max, I think. 
       KNOWN BUGS
       + Exit room doesn't exist.
       + Dungeon-matrix is too small. - Make it so that there is a minimum limit for the dungeon matrix. 
       + Walls overlap on tiles. */
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

    public UnityEngine.Tilemaps.Tile _normalTile;
    public UnityEngine.Tilemaps.Tile _doorTile;
    public UnityEngine.Tilemaps.Tile _exitTile;
    public UnityEngine.Tilemaps.Tile _genericWallTile;
    public UnityEngine.Tilemaps.Tilemap _tileMap;

    public GameObject _goblinPrefab;

    [SerializeField]private int _currentMaxEdge = 100;
    [SerializeField]private int _currentMinEdge = 100;   

    public static System.Random rand = null;

    private static int _currentSeed = new System.Random().Next();
    private static int _currentDungeonHeight = -1;
    private static int _currentDungeonWidth = -1;
    private static int[,] _dungeonMatrix = null;
    private static Indexes _dungeonSize = new Indexes(0, 0);
    private static Indexes _XYMax = new Indexes(0, 0);
    private static Indexes _XYMin = new Indexes(0, 0);
    private static Room _enteringRoom = null;

    // Start is called before the first frame update
    void Start()
    {
        rand = new System.Random(_currentSeed);
        /* TODO, Only place where, GenerateLevel is not called with AmplifyEdges
         TODO, Dont forget assigning new values to static properties on each progressed level, if needed. */
        GenerateLevel();
        PrintDungeonMatrix(false);
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

    public void PrintDungeonMatrix(bool print)
    {
        int xi = -1, xj = -1;
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
                            xi = i;
                            xj = j;
                            str += 'X';
                            break;
                    }
                    str += '\t';
                }
                if (print)
                {
                    Debug.Log(str);
                }                   
            }
        }
        Debug.Log("exit indexes, i, j: " + xi + " " + xj);
        Debug.Log("room num:" + Room.GetNumOfRooms());
    }

    public static void UpdateDungeonSize(Vector2 minVec, Vector2 maxVec)
    {
        if ((int)minVec.x < _XYMin.j)
        {
            _XYMin.j = (int)minVec.x;
        }

        if ((int)minVec.y < _XYMin.i)
        {
            _XYMin.i = (int)minVec.y;
        }

        if ((int)maxVec.x > _XYMax.j)
        {
            _XYMax.j = (int)maxVec.x;
        }

        if ((int)maxVec.y > _XYMax.i)
        {
            _XYMax.i = (int)maxVec.y;
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
    public static int IncreaseX() => ++_XYMax.j;
    public static int IncreaseY() => ++_XYMax.i;

    private void VisualizeDungeon()
    {
        Vector3Int curCell = new Vector3Int();
        Vector3Int originCell = _tileMap.WorldToCell(transform.position);
        /* This makes the 0,0 point of the entrance tile, (_dungeon.Size.i-1 and
         * _dungeonSize.j-1 on the dungeon matrix)to be on the originCell. */
        int differenceY = _dungeonSize.i/2 - 1 - originCell.y;
        int differenceX = _dungeonSize.j/2 - 1 - originCell.x;
        bool exitFound = false;
        UnityEngine.Tilemaps.Tile tileToPut = null;
        GameObject prefabToPut = null;
         for (int i = 0; i < _dungeonSize.i; ++i)
        {
            curCell.y = i - differenceY;
            for (int j = 0; j < _dungeonSize.j; ++j)
            {
                curCell.x = j - differenceX;
                if (_dungeonMatrix[i, j] != -1)
                {
                    switch (_dungeonMatrix[i, j]%10)
                    {
                        case (int)Tile.Normal:
                            tileToPut = _normalTile;
                            break;
                        case (int)Tile.DoorTile:
                            tileToPut = _doorTile;
                            break;
                        case (int)Tile.ExitTile:
                            exitFound = true;
                            tileToPut = _exitTile;
                            break;
                        case (int)Tile.WallTile:
                            tileToPut = _genericWallTile;
                            break;
                    }
                    _tileMap.SetTile(curCell, tileToPut);

                    switch(_dungeonMatrix[i, j]/10)
                    {
                        case (int)FilledType.MonsterGoblin:
                            prefabToPut = _goblinPrefab;
                            break;
                    }
                    if (prefabToPut != null)
                    {
                        Vector3 pos = _tileMap.CellToWorld(curCell);
                        pos.y += 0.6f; pos.x += 0.5f;
                        Instantiate(prefabToPut, pos, Quaternion.identity);
                        prefabToPut = null;
                    }                 
                }
            }
        }
        Debug.Assert(exitFound, "Exit not found on dungeon visualizer.");
        /* TODO, after visualizing the tiles, it should put the monsters, doors, treasures etc. */
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
        _dungeonSize.i = _XYMax.i - _XYMin.i + 1; _dungeonSize.j = _XYMax.j - _XYMin.j + 1;
        Indexes firstRoomIndexes = new Indexes(_dungeonSize.j - 1, _dungeonSize.i-1);
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
        bool notOnFirstRoom = room.GetEnteringDoorDirection() != Direction.DirectionCount;
        if (notOnFirstRoom)
        {
            RoomFiller.FillRoom(new Indexes(room.GetRoomWidth(), room.GetRoomHeight()));
        }
        for (int i = 0; i < room.GetRoomHeight(); ++i)
        {
            for (int j = 0; j < room.GetRoomWidth(); ++j)
            {
                _dungeonMatrix[newOrigin.i + i, newOrigin.j + j] = roomMatrix[i, j];
                if (notOnFirstRoom)
                {
                    _dungeonMatrix[newOrigin.i + i, newOrigin.j + j] += RoomFiller._filledTypes[i, j] * 10;
                }               
            }
        }

        /* Surround with walls. */
        for (int j = -1; j <= room.GetRoomWidth(); ++j)
        {
            _dungeonMatrix[newOrigin.i -1, newOrigin.j + j] = (int)Tile.WallTile;
            _dungeonMatrix[newOrigin.i + room.GetRoomHeight(), newOrigin.j + j] = (int)Tile.WallTile;
        }

        for (int i = 0; i < room.GetRoomHeight(); ++i)
        {
            _dungeonMatrix[newOrigin.i + i, newOrigin.j - 1] = (int)Tile.WallTile;
            _dungeonMatrix[newOrigin.i + i, newOrigin.j + room.GetRoomWidth()] = (int)Tile.WallTile;
        }

        /* Switch the doortile to its correct place. */
        _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i, newOrigin.j + enteringDoorIndexesRoom.j] = (int)Tile.Normal;
        switch (room.GetEnteringDoorDirection())
        {
            case Direction.Up:
                _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i-1, newOrigin.j + enteringDoorIndexesRoom.j] = (int)Tile.DoorTile;
                break;
            case Direction.Down:
                _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i+1, newOrigin.j + enteringDoorIndexesRoom.j] = (int)Tile.DoorTile;
                break;
            case Direction.Left:
                _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i, newOrigin.j + enteringDoorIndexesRoom.j-1] = (int)Tile.DoorTile;
                break;
            case Direction.Right:
                _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i, newOrigin.j + enteringDoorIndexesRoom.j+1] = (int)Tile.DoorTile;
                break;
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