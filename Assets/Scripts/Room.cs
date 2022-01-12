using System.Collections.Generic;
using UnityEngine;

public class Room
{
    private static readonly int[] ChildAmount =
    {1, 2, 3};
    private static readonly int[] ChildAmountWeights =
    {75, 20, 5};
    private static readonly int RoomHeightMax = 23;
    private static readonly int RoomWidthMax = 23;
    private static readonly float ExitRoomLimitMax = 0.8f;
    private static readonly float ExitRoomLimitMin = 0.4f;
    private static readonly float ExitRoomLimitDecrease = 0.02f;

    public static int _minTilesRequired = 0;
    private static float _exitRoomLimitCur = ExitRoomLimitMax;
    private static int _numOfRooms = 0;
    private static bool _exitRoomExists = false;
    private static int _totalTileCreated = 0;   
    private static float _tileUnit = 1f;
    private static Dictionary<Vector2, bool> _existingEdges = new Dictionary<Vector2, bool>();
    private static Dictionary<Vector2, bool> _existingWalls = new Dictionary<Vector2, bool>();

    private Direction _enteringDoorDirection;
    private Vector2 _enteringDoorCoord = new Vector2();
    private List<Room> _leadingRooms = new List<Room>();
    private List<Direction> _doorDirections = new List<Direction>();
    private List<Indexes> _leadingDoorIndexes = new List<Indexes>();
    private int[,] _tiles = null;

    private int _enteringIndexI = -1;
    private int _enteringIndexJ = -1;
    private int _roomHeight = -1;
    private int _roomWidth = -1;
    private int _roomId = -1;

    /* This parameters are for this room to directly use, calculated by the parent room. */
    public bool CreateRoom(Vector2 enteringDoorCord, Direction enteringDoorDirection)
    {
        _enteringDoorCoord.Set(enteringDoorCord.x, enteringDoorCord.y);
        _enteringDoorDirection = enteringDoorDirection;

        /* First a 1x1 room should be checked for overlapping, if it does, then this room can't be created,
            return false to the parent */
        _enteringIndexI = _enteringIndexJ = 0;
        if (DoEdgesOverlappingFast(1, 1))
        {
            return false;
        }

        /* Room Expansion technique for overlapping rooms,
         * Determine edges, check overlapping if does,
         * enter the loop, in the end fill up tiles, i.e. create the matrix, assign properties */
        Indexes firstRoomSize = CalcRoomSize();
        DetermineEnteringDoorIndexes(firstRoomSize.i, firstRoomSize.j);
        if (DoEdgesOverlappingFast(firstRoomSize.i, firstRoomSize.j) &&
            !IsThereLegalRoomWithOtherDoor(firstRoomSize.i, firstRoomSize.j))
        {

            firstRoomSize = RoomExpansion(firstRoomSize);;

            /* After leaving the loop, the door indexes are lost. Retrieve them. But this will tend to set door indexes, 
                * of rooms that are created by expansion method, set similiarly */
            IsThereLegalRoomWithOtherDoor(firstRoomSize.i, firstRoomSize.j);
            if (DoEdgesOverlappingFast(firstRoomSize.i, firstRoomSize.j))
            {
                return false;
            }
        }

        if (DoEdgesOverlappingFast(firstRoomSize.i, firstRoomSize.j))
        {
            return false;
        }
        FillUpTiles(firstRoomSize.i, firstRoomSize.j);

        /* Deciding on exit room. */
        if (CanMakeExitRoom())
        {
            MakeThisExitRoom();
        }

        /* Children generating loop. */
        if (_totalTileCreated < _minTilesRequired)
        {
            GenerateChildren();
        }            
        return true;
    }

    public int GetRoomWidth() => _roomWidth;
    public int GetRoomHeight() => _roomHeight;
    public int[,] GetRoom() => _tiles;
    public int GetRoomId() => _roomId;
    public Direction GetEnteringDoorDirection() => _enteringDoorDirection;
    public Indexes GetEnteringIndexes() => new Indexes(_enteringIndexJ, _enteringIndexI);
    public Vector2 GetDoorTileCoordinate() => _enteringDoorCoord;
    public List<Room> GetChildRooms() => _leadingRooms;
    public List<Indexes> GetChildDoorIndexes() => _leadingDoorIndexes;
    public List<Direction> GetChildDoorDirections() => _doorDirections;
    public static int GetNumOfRooms() => _numOfRooms;

    public static void SetExitRoomLimit()
    {
        if (_exitRoomLimitCur > ExitRoomLimitMin)
        {
            _exitRoomLimitCur -= ExitRoomLimitDecrease;
        }       
    }

    public void MakeThisEnteringRoom()
    {
        _enteringDoorDirection = Direction.DirectionCount;
        Direction oneExit = (Direction)LevelGenerator.rand.Next((int)Direction.DirectionCount);
        _enteringDoorCoord.x = 0; _enteringDoorCoord.y = 0;
        _enteringIndexI = _enteringIndexJ = 0;

        if (oneExit == Direction.Up || oneExit == Direction.Down)
        {
            _tiles = new int[2, 3];
            _roomHeight = 2;
            _roomWidth = 3;
            LevelGenerator.IncreaseY();
        }
        else
        {
            _tiles = new int[3, 2];
            _roomHeight = 3;
            _roomWidth = 2;
            LevelGenerator.IncreaseX();
        }

        _totalTileCreated += _roomHeight * _roomWidth;
        ++_numOfRooms;
        LevelGenerator.UpdateDungeonSize(CalcCoordinates(0, 0), CalcCoordinates(_roomHeight-1, _roomWidth-1));
        RegisterEdgeTilesToDictionary();

        Room child = new Room();
        child.CreateRoom(CalcLeadingDoorCoords(oneExit), RevertDirection(oneExit));
        _leadingRooms.Add(child);
        _doorDirections.Add(oneExit);
    }

    public static void OpenNewDictionary()
    {
        _existingEdges = new Dictionary<Vector2, bool>();
        _existingWalls = new Dictionary<Vector2, bool>();
    }

    private Indexes RoomExpansion(Indexes firstRoomSize)
    {
        /* Use room expansion technique if first initiated room size is overlapping,
            start with 1x1 increase edges one by one with switching if you can,
            try door index changes too, at each step check for edge overlaps */
        bool canIncreaseHeight = true, canIncreaseWidth = true, increaseHeight = true, increasedWidth = false;
        int curWidth = 1, curHeight = 1;
        /* Loop until can't increase the size anymore or one of the sizes hits the max possible */
        while ((canIncreaseHeight || canIncreaseWidth)
            && curWidth < firstRoomSize.j && curHeight < firstRoomSize.i)
        {
            /* First, increase an edge, do this by switching between the edges, if an edge increase is invalidated,
             * only the other edge will be increased */
            if (canIncreaseHeight && increaseHeight)
            {
                ++curHeight;
                increaseHeight = !canIncreaseWidth;
                increasedWidth = false;
            }
            else
            {
                ++curWidth;
                increaseHeight = canIncreaseHeight;
                increasedWidth = true;
            }

            /* Getting the new entering door indexes for the expanded room. */
            DetermineEnteringDoorIndexes(curHeight, curWidth);

            /* If no overlappings or there are overlappings but a legal room exists with another door location, continue */
            if (!DoEdgesOverlappingFast(curHeight, curWidth) ||
                IsThereLegalRoomWithOtherDoor(curHeight, curWidth))
            {
                continue;
            }

            if (increasedWidth)
            {
                canIncreaseWidth = false;
                --curWidth;
            }
            else
            {
                canIncreaseHeight = false;
                --curHeight;
            }
        }
        return new Indexes(curWidth, curHeight);
    }

    private void GenerateChildren()
    {
        int amountOfChildren = ChildAmount[LevelGenerator.GetWeightedRandom(ChildAmountWeights)];
        List<Direction> directionList = new List<Direction>();
        for(int i = 0; i < (int)Direction.DirectionCount; ++i)
        {
            if (i != (int)_enteringDoorDirection)
            {
                directionList.Add((Direction)i);
            }          
        }

        for (int i = 0; i < amountOfChildren; ++i)
        {
            int selectedIndex = LevelGenerator.rand.Next(directionList.Count);

            Room newChild = new Room();
            /* Create the new room, sending it the correct params, if room creation fails, try to make this exit room. */
            if (!newChild.CreateRoom(CalcLeadingDoorCoords(directionList[selectedIndex]), RevertDirection(directionList[selectedIndex])))
            {
                /* If child wasn't created this door doesn't exist. */
                _leadingDoorIndexes.RemoveAt(_leadingDoorIndexes.Count-1);
                if (!_exitRoomExists)
                {
                    MakeThisExitRoom();
                    directionList.RemoveAt(selectedIndex);                   
                }
                continue;
            }

            _leadingRooms.Add(newChild);
            _doorDirections.Add(directionList[selectedIndex]);
            directionList.RemoveAt(selectedIndex);
        }
    }

    /* U <-> D, L <-> R*/
    private Direction RevertDirection(Direction direction)
    {
        int changeAmount = -1;
        if (direction == Direction.Up || direction == Direction.Left)
        {
            changeAmount = 1;
        }

        return direction + changeAmount;
    }

    private Vector2 CalcLeadingDoorCoords(Direction direction)
    {
        /* First determines indexes, then calcs coords,
         * returns the one tile unit spaced coordinate, for the child to use. */
        Indexes ix = DetermineDoorIndexes(_roomHeight, _roomWidth, direction);
        Vector2 vec2 = CalcCoordinates(ix.i, ix.j);

        _leadingDoorIndexes.Add(new Indexes(ix.j, ix.i));

        switch (direction)
        {
            case Direction.Up:
                vec2.y -= 2*_tileUnit;
                break;
            case Direction.Down:
                vec2.y += 2*_tileUnit;
                break;
            case Direction.Left:
                vec2.x -= 2*_tileUnit;
                break;
            case Direction.Right:
                vec2.x += 2*_tileUnit;
                break;
        }
        return vec2;
    }

    /* Decides if this room can be made an exit room with created tile ratio and a limit. Can be improved. */
    private bool CanMakeExitRoom()
    {
        if (_exitRoomExists)
        {
            return false;
        }
        return ((float)_totalTileCreated / _minTilesRequired) >= _exitRoomLimitCur;       
    }

    /* Selects a random tile to be the exit tile, this randomness can be improved. */
    private void MakeThisExitRoom()
    {
        int j = 0, i = 0;
        bool selectJ = true, selectI = true;
        _exitRoomExists = true;

        if (_roomHeight == 1)
        {
            selectI = false;
        }
        else if (_roomWidth == 1)
        {
            selectJ = false;
        }

        while (true)
        {
            if (selectJ)
            {
                j = LevelGenerator.rand.Next(0, _roomWidth);
            }
            if (selectI)
            {
                i = LevelGenerator.rand.Next(0, _roomHeight);
            }
 
            if (_tiles[i, j] == (int)Tile.DoorTile)
            {
                continue;
            }

            _tiles[i, j] = (int)Tile.ExitTile;
            break;
        }
    }

    /* Tries all possible door index for this room and returns if found a legal room, i.e. room with no edge overlappings */
    private bool IsThereLegalRoomWithOtherDoor(int height, int width)
    {
        int prevI = _enteringIndexI, prevJ = _enteringIndexJ;
        _enteringIndexI = _enteringIndexJ = 0;
        switch (_enteringDoorDirection)
        {
            case Direction.Right:
                _enteringIndexJ = width - 1;
                break;
            case Direction.Down:
                _enteringIndexI = height - 1;
                break;
        }

        if (_enteringDoorDirection == Direction.Up || _enteringDoorDirection == Direction.Down)
        {
            for(; _enteringIndexJ < width; ++_enteringIndexJ)
            {
                if (!DoEdgesOverlappingFast(height, width))
                {
                    return true;
                }
            }
        }
        else
        {
            for (; _enteringIndexI < height; ++_enteringIndexI)
            {
                if (!DoEdgesOverlappingFast(height, width))
                {
                    return true;
                }
            }
        }

        _enteringIndexI = prevI; _enteringIndexJ = prevJ;
        return false;
    }

    private void FillUpTiles(int height, int width)
    {
        _roomHeight = height;
        _roomWidth = width;
        _tiles = new int[height, width];

        /* After the room size and door location is finalized */
        _tiles[_enteringIndexI, _enteringIndexJ] = (int)Tile.DoorTile;
        _totalTileCreated += _roomHeight * _roomWidth;
        _roomId = ++_numOfRooms;
        RegisterEdgeTilesToDictionary();

        /* Increase dungeon size */
        LevelGenerator.UpdateDungeonSize(CalcCoordinates(0, 0), CalcCoordinates(_roomHeight - 1, _roomWidth - 1));

        if (_enteringDoorDirection == Direction.Up || _enteringDoorDirection == Direction.Down)
        {
            LevelGenerator.IncreaseY();
        }
        else
        {
            LevelGenerator.IncreaseX();
        }
    }

    private bool DoEdgesOverlappingFast(int height, int width)
    {
        /* For the first row and last row */
        for (int j = 0; j < width; ++j)
        {
            Vector2 calced1 = CalcCoordinates(0, j);
            Vector2 calced2 = CalcCoordinates(height - 1, j);
            if (_existingEdges.ContainsKey(calced1) || _existingWalls.ContainsKey(calced1)
                || _existingEdges.ContainsKey(calced2) ||_existingWalls.ContainsKey(calced2))
            {
                return true;
            }
        }

        /* For the first and last column */
        for (int i = 1; i < height - 1; ++i)
        {
            Vector2 calced1 = CalcCoordinates(i, 0);
            Vector2 calced2 = CalcCoordinates(i, width - 1);
            if (_existingEdges.ContainsKey(calced1) || _existingWalls.ContainsKey(calced1)
                || _existingEdges.ContainsKey(calced2) ||_existingWalls.ContainsKey(calced2))
            {
                return true;
            }
        }

        return false;
    }

    private void DetermineEnteringDoorIndexes(int height, int width)
    {
        Indexes ix = DetermineDoorIndexes(height, width, _enteringDoorDirection);
        _enteringIndexI = ix.i; _enteringIndexJ = ix.j;
    }

    private Indexes DetermineDoorIndexes(int height, int width, Direction doorDirection)
    {
        /* Placing the door on the designated edge of the room */
        int upperLimit = (doorDirection == Direction.Up || doorDirection == Direction.Down) ? width : height;
        int doorRow = LevelGenerator.rand.Next(0, upperLimit);
        int doorColumn = -1;

        switch (doorDirection)
        {
            case Direction.Up:
                doorColumn = doorRow;
                doorRow = 0;
                break;
            case Direction.Down:
                doorColumn = doorRow;
                doorRow = height - 1;
                break;
            case Direction.Left:
                doorColumn = 0;
                break;
            case Direction.Right:
                doorColumn = width - 1;
                break;
        }
        return new Indexes(doorColumn, doorRow);
    }

    private void RegisterEdgeTilesToDictionary()
    {
        /* Add 4 corners. */
        AddRecordToDictionary(-1, -1, true);
        AddRecordToDictionary(-1, _roomWidth, true);
        AddRecordToDictionary(_roomHeight, -1, true);
        AddRecordToDictionary(_roomHeight, _roomWidth, true);

        /* Special case for rooms with one length width or height. */
        if (_roomWidth == 1 || _roomHeight == 1)
        {
            for (int i = 0; i < _roomHeight; ++i)
            {
                for (int j = 0; j < _roomWidth; ++j)
                {
                    /* Add the normal edge. */
                    AddRecordToDictionary(i, j);
                    /* Add the wall edge. */
                    AddRecordToDictionary(i - 1, j, true);
                }
            }
            return;
        }

        /* For the first row and last row. */
        for (int j = 0; j < _roomWidth; ++j)
        {
            /* Add the normal edges. */
            AddRecordToDictionary(0, j);
            AddRecordToDictionary(_roomHeight - 1, j);
            /* Add the wall edges. */
            AddRecordToDictionary(-1, j, true);
            AddRecordToDictionary(_roomHeight, j, true);
        }

        /* For the first and last column. */
        for (int i = 1; i < _roomHeight-1; ++i)
        {
            /* Add the normal edges. */
            AddRecordToDictionary(i, 0);
            AddRecordToDictionary(i, _roomWidth-1);
            /* Add the wall edges. */
            AddRecordToDictionary(i, -1, true);
            AddRecordToDictionary(i, _roomWidth, true);
        }
    }

    private void AddRecordToDictionary(int i, int j, bool wallTile = false)
    {
        /* Since wall edges can overlap with each other. */
        if (wallTile)
        {
            if (_existingWalls.ContainsKey(CalcCoordinates(i, j)))
            {
                return;
            }
            _existingWalls.Add(CalcCoordinates(i, j), true);
            return;
        }
        _existingEdges.Add(CalcCoordinates(i, j), true);
    }

    private Vector2 CalcCoordinates(int i, int j)
    {
        Vector2 vec2 = new Vector2();
        vec2.x = _enteringDoorCoord.x + (j - _enteringIndexJ) * _tileUnit;
        vec2.y = _enteringDoorCoord.y + (i - _enteringIndexI) * _tileUnit;
        return vec2;
    }

    private Indexes CalcRoomSize()
    {
        float width = -1, height = -1;
        int upperWidthLimit = (Mathf.CeilToInt((float)LevelGenerator.GetCurWidth() / 2) >= RoomWidthMax) ? RoomWidthMax
            : Mathf.CeilToInt((float)LevelGenerator.GetCurWidth() / 2),
        upperHeightLimit = (Mathf.CeilToInt((float)LevelGenerator.GetCurHeight() / 2) >= RoomHeightMax) ? RoomHeightMax
            : Mathf.CeilToInt((float)LevelGenerator.GetCurHeight() / 2);

        width = LevelGenerator.rand.Next(2, upperWidthLimit + 1);
        height = LevelGenerator.rand.Next(2, upperHeightLimit + 1);

        return new Indexes((int)width, (int)height);
    }

    public static void ClearData()
    {
        _minTilesRequired = 0;
        _exitRoomLimitCur = ExitRoomLimitMax;
        _numOfRooms = 0;
        _exitRoomExists = false;
        _totalTileCreated = 0;
        _tileUnit = 1f;
        _existingEdges = new Dictionary<Vector2, bool>();
        _existingWalls = new Dictionary<Vector2, bool>();
    }
}