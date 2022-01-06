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
    TrapLow = 5,
    TrapMid = 6,
    TrapHigh = 7
}

public enum CTPColours
{
    Yellow = 0,
    Green = 1,
    Blue = 2,
    Red = 3,
    Black = 4,
    White = 5,
    Magenta = 6,
    CTPCCount = 7
}

public enum TreasureState
{
    EnterTreasure = 0,
    OnTreasure = 1,
    ExitTreasure = 2,
    TreasureStateCount = 3
}

public enum PuzzleType
{
    ColourTilePuzze = 1,
    PuzzleTypeCount = 2
}

public enum IconType
{
    HP = 0,
    AttackDamage = 1,
    LevelNumber = 2,
    IconTypeCount = 3
}

public struct TreasureData
{
    public int _richnessIndex;
    public float _treasureMultiplier;
    public bool _opened;

    public TreasureData(int richnessIndex, float treasureMultipler)
    {
        _richnessIndex = richnessIndex;
        _treasureMultiplier = treasureMultipler;
        _opened = false;
    }
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