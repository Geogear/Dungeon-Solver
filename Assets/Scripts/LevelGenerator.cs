using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private static readonly int [] EdgeIncreaseAmount =
        {1, 2, 3, 4};
    private static readonly int[] EdgeIncreaseAmountWeights =
        {20, 50, 20, 10};
    private static readonly string[] TagsToDestroy =
        { "Enemy", "Treasure", "LevelExit", "Trap", "HealingStatue" };
    public static readonly int MaxLevel = 20;
    public static readonly int EnemyBaseRenderOrder = 2;

    public UnityEngine.Tilemaps.Tile _normalTile;
    public UnityEngine.Tilemaps.Tile _doorTile;
    public UnityEngine.Tilemaps.Tile _exitTile;
    public UnityEngine.Tilemaps.Tile _genericWallTile;
    public UnityEngine.Tilemaps.Tilemap _tileMap;

    public System.Collections.Generic.List<GameObject> _enemyPrefabs;
    public GameObject _treasurePrefab;
    public GameObject _trapPrefab;
    public GameObject _levelExitPrefab;
    public GameObject _healingStatuePrefab;
    public GameObject _background;

    public GameObject _playerObject;
    private PlayerCharacter _playerScript;
    private GameObject _instantiatedBG;

    [SerializeField] private UnityEngine.UI.Text _seedText;

    [SerializeField]private int _currentMaxEdge = 5;
    [SerializeField]private int _currentMinEdge = 3;
    [SerializeField]private int _customSeed = 0;  

    public static System.Random rand = null;
    public static Indexes _dungeonSize = new Indexes(0, 0);
    public static Cell[,] _dungeonMatrix = null;
    private static int _currentSeed = new System.Random().Next();
    private static int _currentDungeonHeight = -1;
    private static int _currentDungeonWidth = -1;
    private static int _currentLevel = 0;
    private static int _differenceY = 0;
    private static int _differenceX = 0;
    private static int _currentEnemyRenderOrder = EnemyBaseRenderOrder;
    private static Indexes _XYMax = new Indexes(0, 0);
    private static Indexes _XYMin = new Indexes(0, 0);
    private static Room _enteringRoom = null;

    // Start is called before the first frame update
    void Start()
    {
        if (_customSeed > 0)
        {
            _currentSeed = _customSeed;
        }
        Debug.Log("Seed:" + _currentSeed);
        rand = new System.Random(_currentSeed);
        /* Only place where, GenerateLevel is not called with AmplifyEdges. */
        _instantiatedBG = Instantiate(_background, new Vector3(transform.position.x, transform.position.y, 1), Quaternion.identity);
        GenerateLevel();
        PrintDungeonMatrix(false);
        VisualizeDungeon();
        _playerObject.SetActive(true);
        _playerScript = _playerObject.GetComponent<PlayerCharacter>();
        _seedText.text = "Seed:" + _currentSeed;
        EnemyCharacter._tileMap = _tileMap;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void GenerateLevel()
    {
        /* Keep track. */
        ++_currentLevel;

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
                    switch (_dungeonMatrix[i, j]._tileType)
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
        return -1;
    }

    public static float RandomFloat(float minBoundary, float maxBoundary)
    {
        float randomFloat = rand.Next((int)minBoundary, (int)maxBoundary + 1);
        randomFloat += (float)rand.NextDouble();
        if (randomFloat < minBoundary)
        {
            randomFloat = minBoundary;
        }
        else if (randomFloat > maxBoundary)
        {
            randomFloat = maxBoundary;
        }
        return randomFloat;
    }

    public static void ClearLGData()
    {
        rand = null;
        _currentSeed = new System.Random().Next();
        _currentDungeonHeight = -1;
        _currentDungeonWidth = -1;
        _currentLevel = 0;
        _dungeonMatrix = null;
        _dungeonSize = new Indexes(0, 0);
        _XYMax = new Indexes(0, 0);
        _XYMin = new Indexes(0, 0);
        _enteringRoom = null;
    }

    public static Indexes GetDifIndex() => new Indexes(_differenceX, _differenceY);

    public void GenerateNextLevel()
    {
        ClearLevel();
        AmplifyEdges();
        GenerateLevel();
        PrintDungeonMatrix(false);
        VisualizeDungeon();
        _playerScript.SetToStartPos();
    }

    public static int GetCurSeed() => _currentSeed;
    public static int GetCurHeight() => _currentDungeonHeight;
    public static int GetCurWidth() => _currentDungeonWidth;
    public static int GetCurrentLvl() => _currentLevel;
    public static int IncreaseX() => ++_XYMax.j;
    public static int IncreaseY() => ++_XYMax.i;

    private void VisualizeDungeon()
    {
        _currentEnemyRenderOrder = EnemyBaseRenderOrder;
        Vector3Int curCell = new Vector3Int();
        Vector3Int originCell = _tileMap.WorldToCell(transform.position);
        /* This makes the 0,0 point of the entrance tile, (_dungeon.Size.i-1 and
         * _dungeonSize.j-1 on the dungeon matrix)to be on the originCell. */
        _differenceY = _dungeonSize.i/2 - 1 - originCell.y;
        _differenceX = _dungeonSize.j/2 - 1 - originCell.x;
        bool exitFound = false;
        UnityEngine.Tilemaps.Tile tileToPut = null;
        GameObject prefabToPut = null;
         for (int i = 0; i < _dungeonSize.i; ++i)
        {
            curCell.y = i - _differenceY;
            for (int j = 0; j < _dungeonSize.j; ++j)
            {
                curCell.x = j - _differenceX;
                if (_dungeonMatrix[i, j]._tileType != -1)
                {
                    switch (_dungeonMatrix[i, j]._tileType%10)
                    {
                        case (int)Tile.Normal:
                            tileToPut = _normalTile;
                            _dungeonMatrix[i, j]._walkable = true;
                            break;
                        case (int)Tile.DoorTile:
                            tileToPut = _doorTile;
                            _dungeonMatrix[i, j]._walkable = true;
                            break;
                        case (int)Tile.ExitTile:
                            exitFound = true;
                            tileToPut = _exitTile;
                            _dungeonMatrix[i, j]._walkable = true;
                            break;
                        case (int)Tile.WallTile:
                            tileToPut = _genericWallTile;
                            break;
                    }                    
                    _tileMap.SetTile(curCell, tileToPut);

                    int filledType = _dungeonMatrix[i, j]._tileType / 10;
                    Vector3 pos = _tileMap.CellToWorld(curCell);
                    if (tileToPut == _exitTile)
                    {
                        prefabToPut = _levelExitPrefab;
                        pos.y += 0.5f; pos.x += 0.455f;
                    }
                    else if (filledType == (int)FilledType.MonsterGoblin)
                    {
                        prefabToPut = _enemyPrefabs[0];
                        pos.y += 0.6f; pos.x += 0.5f;
                    }
                    else if (filledType >= (int)FilledType.TreasureLow && filledType <= (int)FilledType.TreasureHigh)
                    {
                        prefabToPut = _treasurePrefab;
                        pos.y += 0.5f; pos.x += 0.5f;
                        Treasure.AddTreasure(pos, filledType);
                        _dungeonMatrix[i, j]._walkable = false;
                    }
                    else if (filledType >= (int)FilledType.TrapLow && filledType <= (int)FilledType.TrapHigh)
                    {
                        prefabToPut = _trapPrefab;
                        pos.y += 0.65f; pos.x += 0.5f;
                        Spikes.SetDamageMultiplierForNext((FilledType)filledType);
                    }
                    else if(filledType == (int)FilledType.HealingStatue)
                    {
                        prefabToPut = _healingStatuePrefab;
                        pos.y += 0.80f; pos.x += 0.45f;
                        _dungeonMatrix[i, j]._walkable = false;
                    }else if(filledType >= (int)FilledType.MonsterOrc && filledType <= (int)FilledType.MonsterGolem3)
                    {
                        prefabToPut = _enemyPrefabs[filledType-(int)FilledType.MonsterOrc+1];
                        pos.y += 0.6f; pos.x += 0.5f;
                    }

                    if (prefabToPut != null)
                    {                      
                        GameObject go = Instantiate(prefabToPut, pos, Quaternion.identity);
                        if(filledType == (int)FilledType.MonsterGoblin ||
                            (filledType >= (int)FilledType.MonsterOrc && filledType <= (int)FilledType.MonsterGolem3))
                        {
                            go.GetComponent<SpriteRenderer>().sortingOrder = _currentEnemyRenderOrder++;
                        }
                        prefabToPut = null;
                    }                 
                }
            }
        }
        Debug.Assert(exitFound, "Exit not found on dungeon visualizer.");

        SetBG();
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
            if ((_dungeonMatrix[i + increaseI, j]._tileType == (int)tile && _dungeonMatrix[i - decreaseI, j]._tileType == oppositeTile)
                || (_dungeonMatrix[i + increaseI, j]._tileType == oppositeTile && _dungeonMatrix[i - decreaseI, j]._tileType == (int)tile))
            {
                return true;
            }
        }

        if (increaseJ == 1 && decreaseJ == 1)
        {
            if ((_dungeonMatrix[i, j + increaseJ]._tileType == (int)tile && _dungeonMatrix[i, j - decreaseJ]._tileType == oppositeTile)
                || (_dungeonMatrix[i, j + increaseJ]._tileType == oppositeTile && _dungeonMatrix[i, j - decreaseJ]._tileType == (int)tile))
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

        return ((_dungeonMatrix[i, j - decreaseJ]._tileType == (int)tile || _dungeonMatrix[i, j + increaseJ]._tileType == (int)tile
            || _dungeonMatrix[i + increaseI, j]._tileType == (int)tile || _dungeonMatrix[i - decreaseI, j]._tileType == (int)tile)
            || (!fourNeighbourMode && (
            _dungeonMatrix[i + increaseI, j - decreaseJ]._tileType == (int)tile || _dungeonMatrix[i + increaseI, j + increaseJ]._tileType == (int)tile
            || _dungeonMatrix[i - decreaseI, j - decreaseJ]._tileType == (int)tile || _dungeonMatrix[i - decreaseI, j + increaseJ]._tileType == (int)tile)));
    }

    private void CreateDungeonMatrix()
    {
        _dungeonSize.i = _XYMax.i - _XYMin.i + 1; _dungeonSize.j = _XYMax.j - _XYMin.j + 1;        
        _dungeonSize.i *= 4; _dungeonSize.j *= 4;
        Indexes firstRoomIndexes = new Indexes(_dungeonSize.j/2, _dungeonSize.i/2);

        _dungeonMatrix = new Cell[_dungeonSize.i, _dungeonSize.j];
        for (int i = 0; i < _dungeonSize.i; ++i)
        {
            for (int j = 0; j < _dungeonSize.j; ++j)
            {
                _dungeonMatrix[i, j] = new Cell(-1);
            }
        }

        RoomFiller.SetMonsterSpawnRates(_currentLevel);
        PutRoom(firstRoomIndexes, _enteringRoom);
    }

    private void PutRoom(Indexes enteringDoorIndexesDungeon, Room room)
    {       
        Indexes enteringDoorIndexesRoom = room.GetEnteringIndexes();
        Indexes newOrigin = new Indexes(enteringDoorIndexesDungeon.j - enteringDoorIndexesRoom.j,
            enteringDoorIndexesDungeon.i - enteringDoorIndexesRoom.i);

        /* If not first room, fill it using RoomFiller. */
        int[,] roomMatrix = room.GetRoom();
        bool notOnFirstRoom = room.GetEnteringDoorDirection() != Direction.DirectionCount;
        if (notOnFirstRoom)
        {
            RoomFiller.FillRoom(new Indexes(room.GetRoomWidth(), room.GetRoomHeight()), room.GetEnteringIndexes(), room.GetChildDoorIndexes());
        }
        for (int i = 0; i < room.GetRoomHeight(); ++i)
        {
            for (int j = 0; j < room.GetRoomWidth(); ++j)
            {
                _dungeonMatrix[newOrigin.i + i, newOrigin.j + j]._tileType = roomMatrix[i, j];
                if (notOnFirstRoom)
                {
                    _dungeonMatrix[newOrigin.i + i, newOrigin.j + j]._tileType += RoomFiller._filledTypes[i, j] * 10;
                }              
            }
        }

        /* Place healing statue in the first room only. */
        if(!notOnFirstRoom && CanPutHealingStatute())
        {
            int tmpI = -1, tmpJ = -1;
            switch(room.GetChildDoorDirections()[0])
            {
                case Direction.Up:
                    tmpI = 1; tmpJ = 2;
                    break;
                case Direction.Down:
                    tmpI = 1;
                    tmpJ = (room.GetChildDoorIndexes()[0].j == 0) ? 2 : 0;
                    break;
                case Direction.Left:
                    tmpI = 2; tmpJ = 1;
                    break;
                case Direction.Right:
                    tmpI = 2; tmpJ = 0;
                    break;
            }
            _dungeonMatrix[newOrigin.i + tmpI, newOrigin.j + tmpJ]._tileType += (int)FilledType.HealingStatue * 10;
        }

        /* Surround with walls. */
        for (int j = -1; j <= room.GetRoomWidth(); ++j)
        {
            _dungeonMatrix[newOrigin.i -1, newOrigin.j + j]._tileType = (int)Tile.WallTile;
            _dungeonMatrix[newOrigin.i + room.GetRoomHeight(), newOrigin.j + j]._tileType = (int)Tile.WallTile;
        }

        for (int i = 0; i < room.GetRoomHeight(); ++i)
        {
            _dungeonMatrix[newOrigin.i + i, newOrigin.j - 1]._tileType = (int)Tile.WallTile;
            _dungeonMatrix[newOrigin.i + i, newOrigin.j + room.GetRoomWidth()]._tileType = (int)Tile.WallTile;
        }

        /* Switch the doortile to its correct place. */
        _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i, newOrigin.j + enteringDoorIndexesRoom.j]._tileType = (int)Tile.Normal;
        switch (room.GetEnteringDoorDirection())
        {
            case Direction.Up:
                _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i-1, newOrigin.j + enteringDoorIndexesRoom.j]._tileType = (int)Tile.DoorTile;
                break;
            case Direction.Down:
                _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i+1, newOrigin.j + enteringDoorIndexesRoom.j]._tileType = (int)Tile.DoorTile;
                break;
            case Direction.Left:
                _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i, newOrigin.j + enteringDoorIndexesRoom.j-1]._tileType = (int)Tile.DoorTile;
                break;
            case Direction.Right:
                _dungeonMatrix[newOrigin.i + enteringDoorIndexesRoom.i, newOrigin.j + enteringDoorIndexesRoom.j+1]._tileType = (int)Tile.DoorTile;
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

    private void SetBG()
    {
        _instantiatedBG.transform.localScale = new Vector3(_dungeonSize.j, _dungeonSize.i, 1);
    }

    private void ClearLevel()
    {
        _tileMap.ClearAllTiles();
        Room.ClearData();
        Treasure.ClearData();
        /* Get gameobects by tag and destroy them. 
         +destroy traps
         +destroy other? */
        for (int i = 0; i < TagsToDestroy.Length; ++i)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(TagsToDestroy[i]);
            foreach(GameObject obj in objects)
            {
                Destroy(obj);
            }
        }      
    }

    private bool CanPutHealingStatute()
    {
        Debug.Log("lvl: " +_currentLevel);
        return (_currentLevel > 10) ? _currentLevel % 2 == 0
            : _currentLevel % 4 == 0;
    }
}