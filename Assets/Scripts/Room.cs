using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public static int _minTilesRequired = 0;
    private static bool _exitRoomExists = false;
    private static int _totalTileCreated = 0;   
    private static float _tileUnit = 1f;
    private static Dictionary<Vector2, bool> _existingEdges = new Dictionary<Vector2, bool>();

    private Vector2 _enteringDoorCoord = new Vector2();
    private List<Room> _leadingRooms = new List<Room>();
    private List<Direction> _doorDirections = new List<Direction>();    
    private List<List<int>> _tiles = new List<List<int>>();

    private int _enteringIndexI = -1;
    private int _enteringIndexJ = -1;

    /* This parameters are for this room to directly use, calculated by the parent room. */
    public bool CreateRoom(Vector2 enteringDoorCord, Direction enteringDoorDirection)
    {
        _enteringDoorCoord.Set(enteringDoorCord.x, enteringDoorCord.y);

        /* TODO, first a 1x1 room should be checked for overlapping, if it does, then this room cant be created
            return false, */

        FillUpTiles(enteringDoorDirection);
        RecordEnteringTileIndexes();

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
        var rand = new System.Random();
        /* TODO, Fill all room tiles as normal tiles */

        /* TODO, so each step it has to come up with the new door indexes, and sometimes problem could be not the size but the door placement,
         so code should do a smart decision based on the overlappings, should think of each overlapping case */
        /* 1.decide width and height, 2.check if overlapping edges exist go to 1., 3.increase _totalTileCreated 
         + As the dungeon gets bigger, the rooms should get bigger as well*/

        int roomHeight = -1, roomWidth = -1,
        lastHeight = LevelGenerator.GetCurHeight(),
        lastWidth = LevelGenerator.GetCurWidth();

        /* Start of the room generating loop, because it needs entering room indexes to calculate other points */
        DetermineEnteringDoorIndexes(enteringDoorDirection, rand, lastHeight, lastWidth);


        /* After the room size and door location is finalized */
        _tiles[_enteringIndexI][_enteringIndexJ] = (int)Tile.DoorTile;

    }

    /* TODO, If edges are overlapping returns true */
    private bool DoEdgesOverlapping(int height, int width)
    {
        /* For the first row and last row */
        for (int j = 0; j < width; ++j)
        {

        }

        /* For the first and last column */
        for (int i = 1; i < height - 1; ++i)
        {

        }
        return true;
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
        vec2.y = _enteringDoorCoord.y + (i - _enteringIndexI) * _tileUnit;
        return vec2;
    }

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
}
