using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    private static readonly int MaxRoomHeight = 23;
    private static readonly int MaxRoomWidth = 23;

    public static int _minTilesRequired = 0;
    private static bool _exitRoomExists = false;
    private static int _totalTileCreated = 0;   
    private static float _tileUnit = 1f;
    private static Dictionary<Vector2, bool> _existingEdges = new Dictionary<Vector2, bool>();

    private Vector2 _enteringDoorCoord = new Vector2();
    private List<Room> _leadingRooms = new List<Room>();
    private List<Direction> _doorDirections = new List<Direction>();    
    private List<List<int>> _tiles = new List<List<int>>();
    private List<Indexes> _edgeOverlappings = null;

    private int _enteringIndexI = -1;
    private int _enteringIndexJ = -1;

    /* This parameters are for this room to directly use, calculated by the parent room. */
    public bool CreateRoom(Vector2 enteringDoorCord, Direction enteringDoorDirection)
    {
        _enteringDoorCoord.Set(enteringDoorCord.x, enteringDoorCord.y);

        /* First a 1x1 room should be checked for overlapping, if it does, then this room can't be created,
            return false to the parent */
        _enteringIndexI = _enteringIndexJ = 0;
        if (DoEdgesOverlapping(1, 1))
        {
            return false;
        }

        /* Determine edges, check overlapping if does, enter the loop, in the end fill up tiles, i.e. just create the matrix */
        Indexes firstRoomSize = CalcRoomSize();
        DetermineEnteringDoorIndexes(enteringDoorDirection, new System.Random(), firstRoomSize.i, firstRoomSize.j);
        if (DoEdgesOverlapping(firstRoomSize.i, firstRoomSize.j))
        {
            /* TODO, fast edge overlapping, change the existing one with a bool */
            /* TODO, use room expansion technique if first initiated room size is overlapping,
                start with 1x1 increase edges one by one, try door index changes too, at each step check for edge overlaps 
                no need to record for edge overlaps for this, might change the exiting one or create a new faster one.*/
            /* TODO, try changing the door location of the first sizes, before entering the loop */
            bool canIncreaseHeight = true, canIncreaseWidth = true;
            int curWidth = 1, curHeight = 1;
            /* Loop until can't increase the size anymore or one of the sizes hits the max possible */
            while ((canIncreaseHeight || canIncreaseWidth)
                && curWidth < firstRoomSize.j && curHeight < firstRoomSize.i)
            {

            }
        }

        FillUpTiles(enteringDoorDirection);       

        /* TODO
           Determine room size, determine the entering door indexes etc., increase totalTileCreated and update minTilesCreated if necessary 
           Check if exit room is created, if not take your shot to create it
           Check if min tile count is satisfied, if not determine leading doors and their directions etc. */
        return true;
    }

    public void OpenNewDictionary()
    {
        _existingEdges = new Dictionary<Vector2, bool>();
    }

    public Vector2 GetDoorTileCoordinate()
    {
        return _enteringDoorCoord;
    }

    private void FillUpTiles(Direction enteringDoorDirection)
    {       
        /* TODO, just generate the matrix, and fill the values */

        /* After the room size and door location is finalized */
        _tiles[_enteringIndexI][_enteringIndexJ] = (int)Tile.DoorTile;
    }

    /* If edges are overlapping returns true */
    private bool DoEdgesOverlapping(int height, int width, bool doFast = true)
    {
        if (doFast)
        {
            return DoEdgesOverlappingFast(height, width);
        }
        _edgeOverlappings = new List<Indexes>();
        /* For the first row and last row */
        for (int j = 0; j < width; ++j)
        {
            if (_existingEdges.ContainsKey(CalcCoordinates(0, j)))
            {
                _edgeOverlappings.Add(new Indexes(j, 0));
            }

            if (_existingEdges.ContainsKey(CalcCoordinates(height - 1, j)))
            {
                _edgeOverlappings.Add(new Indexes(j, height - 1));
            }
        }

        /* For the first and last column */
        for (int i = 1; i < height - 1; ++i)
        {
            if (_existingEdges.ContainsKey(CalcCoordinates(i, 0)))
            {
                _edgeOverlappings.Add(new Indexes(i, 0));
            }

            if (_existingEdges.ContainsKey(CalcCoordinates(i, width - 1)))
            {
                _edgeOverlappings.Add(new Indexes(i, width - 1));
            }
        }
        return _edgeOverlappings.Count != 0;
    }

    private bool DoEdgesOverlappingFast(int height, int width)
    {
        /* For the first row and last row */
        for (int j = 0; j < width; ++j)
        {
            if (_existingEdges.ContainsKey(CalcCoordinates(0, j))
                || _existingEdges.ContainsKey(CalcCoordinates(height - 1, j)))
            {
                return true;
            }
        }

        /* For the first and last column */
        for (int i = 1; i < height - 1; ++i)
        {
            if (_existingEdges.ContainsKey(CalcCoordinates(i, 0))
                || _existingEdges.ContainsKey(CalcCoordinates(i, width - 1)))
            {
                return true;
            }
        }
        return false;
    }

    private void DetermineEnteringDoorIndexes(Direction enteringDoorDirection, System.Random rand, int height, int width)
    {
        /* Placing the door on the designated edge of the room */
        int upperLimit = (enteringDoorDirection == Direction.Up || enteringDoorDirection == Direction.Down) ? width : height;
        int doorRow = rand.Next(0, upperLimit);
        int doorColumn = -1;

        switch (enteringDoorDirection)
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
        _enteringIndexI = doorRow; _enteringIndexJ = doorColumn;
    }

    private void RegisterEdgeTilesToDictionary()
    {
        /* For the first row and last row */
        for (int j = 0; j < _tiles[0].Count; ++j)
        {
            AddRecordToDictionary(0, j);
            AddRecordToDictionary(_tiles.Count - 1, j);
        }

        /* For the first and last column */
        for (int i = 1; i < _tiles.Count-1; ++i)
        {
            AddRecordToDictionary(i, 0);
            AddRecordToDictionary(i, _tiles[0].Count-1);
        }
    }

    private void AddRecordToDictionary(int i, int j)
    {
        _existingEdges.Add(CalcCoordinates(i, j), true);
    }

    private Vector2 CalcCoordinates(int i, int j)
    {
        Vector2 vec2 = new Vector2();
        vec2.x = _enteringDoorCoord.x + (j - _enteringIndexJ) * _tileUnit;
        vec2.y = _enteringDoorCoord.y + (_enteringIndexI - i) * _tileUnit;
        return vec2;
    }

    /* TODO, Uselesss? Already recording in DetermineEtneringDoorIndexes */
    private void RecordEnteringTileIndexes()
    {
        for (int i = 0; i < _tiles.Count; ++i)
        {
            for (int j = 0; j < _tiles[i].Count; ++j)
            {
                if (_tiles[i][j] == (int)Tile.DoorTile)
                {
                    _enteringIndexI = i;
                    _enteringIndexJ = j;
                    return;
                }
            }
        }
    }

    private Indexes CalcRoomSize()
    {
        var rand = new System.Random();
        float width = -1, height = -1;
        int upperWidthLimit = (Mathf.CeilToInt(LevelGenerator.GetCurWidth() / 2) >= MaxRoomWidth) ? MaxRoomWidth
            : Mathf.CeilToInt(LevelGenerator.GetCurWidth() / 2),
        upperHeightLimit = (Mathf.CeilToInt(LevelGenerator.GetCurHeight() / 2) >= MaxRoomHeight) ? MaxRoomHeight
            : Mathf.CeilToInt(LevelGenerator.GetCurHeight() / 2);

        width = rand.Next(2, upperWidthLimit + 1);
        height = rand.Next(2, upperHeightLimit + 1);

        return new Indexes((int)width, (int)height);
    }
}
