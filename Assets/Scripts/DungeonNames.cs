public enum Direction
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    DirectionCount = 4
}

public enum Tile
{
    Normal = 0,
    DoorTile = 1,
    ExitTile = 2,
    WallTile =  3,
    TileCount = 4
}

public enum FilledType
{
    MonsterGoblin = 1,
    TreasureLow = 2,
    TreasureMid = 3,
    TreasureHigh = 4,
    Trap1 = 5
}

public struct Indexes
{
    public int j;
    public int i;

    public Indexes(int x, int y)
    {
        j = x;
        i = y;
    }
}