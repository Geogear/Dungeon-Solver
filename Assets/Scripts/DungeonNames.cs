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
    TileCount = 2
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