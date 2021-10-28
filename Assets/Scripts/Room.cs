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
    public void CreateRooms(Vector2 enteringDoorCord, Direction enteringDoorDirection)
    {
        _enteringDoorCoord.Set(enteringDoorCord.x, enteringDoorCord.y);
        FillUpTiles(enteringDoorDirection);
        RecordEnteringTileIndexes();
        /* TODO
           Determine room size, determine the entering door indexes etc., increase totalTileCreated and update minTilesCreated if necessary 
           Check if exit room is created, if not take your shot to create it
           Check if min tile count is satisfied, if not determine leading doors and their directions etc. */
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
        /* TODO, according to direction mark the entering door and the other tiles */
        /* int [] edges =  TODO, pass the edges as param or just make them static?? 
                        + To understand rooms are overlapping only storing and checking the edges are enough. */
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
        float x = _enteringDoorCoord.x + (j - _enteringIndexJ) * _tileUnit;
        float y = _enteringDoorCoord.y + (i - _enteringIndexI) * _tileUnit;
        _existingEdges.Add(new Vector2(x, y), true);
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
