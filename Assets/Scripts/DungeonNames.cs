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
    TrapHigh = 7,
    HealingStatue = 8,
    MonsterOrc = 9,
    MonsterOgre = 10,
    MonsterGolem1 = 11,
    MonsterGolem2 = 12,
    MonsterGolem3 = 13,
    BossWraith1 = 14,
    BossWraith2 = 15,
    BossWraith3 = 16
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
    MoveSpeed = 2,
    AttackRate = 3,
    LevelNumber = 4,
    IconTypeCount = 5
}

public enum PFState
{
    Wait = 0,
    OnRoute = 1,
    PFStateCount = 2
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

public struct FlickerData
{
    public UnityEngine.Color _color;
    public UnityEngine.SpriteRenderer _spriteRenderer;
    public float _baseFlickerTime;
    public float _currentFlickerTime;
    public float _baseFlickTime;
    public float _currentFlickTime;
    public float _flickerAlpha;
    public bool _flickering;
    public bool _flick;

    public FlickerData(UnityEngine.SpriteRenderer spriteRenderer, float baseFlickerTime = 1.3f, float baseFlickTime = 0.1f,
        float flickerAlpha = 0.5f)
    {
        _color = new UnityEngine.Color(spriteRenderer.color.r, spriteRenderer.color.g,
            spriteRenderer.color.b, spriteRenderer.color.a);
        _spriteRenderer = spriteRenderer;
        _baseFlickerTime = baseFlickerTime;
        _currentFlickerTime = 0.0f;
        _baseFlickTime = baseFlickTime;
        _currentFlickTime = 0.0f;
        _flickerAlpha = flickerAlpha;
        _flickering = false;
        _flick = true;
    }

    /* Returns _flickering for convenience. */
    public bool Flicker()
    {
        if (_flickering)
        {
            _currentFlickerTime -= UnityEngine.Time.deltaTime;
            if (_currentFlickerTime <= float.Epsilon)
            {
                _flickering = false;
                _flick = true;
                _color.a = 1.0f;
                _spriteRenderer.color = _color;
            }
            else
            {
                _currentFlickTime -= UnityEngine.Time.deltaTime;
                if (_currentFlickTime <= float.Epsilon)
                {
                    _currentFlickTime = _baseFlickTime;
                    _color.a = _flick ? _flickerAlpha : 1.0f;
                    _spriteRenderer.color = _color;
                    _flick = !_flick;
                }
            }
        }
        return _flickering;
    }

    public void TriggerFlick()
    {
        _flickering = true;
        _flick = true;
        _currentFlickTime = 0.0f;
        _currentFlickerTime = _baseFlickerTime;
    }
}

public struct Cell
{
    public static int _normalCost = 10;
    public static int _diagonalCost = 14;
    public int _tileType;
    public int _gCost;
    public bool _walkable;

    public Cell(int tileType, int gCost = 0, bool walkable = false)
    {
        _tileType = tileType;
        _gCost = gCost;    
        _walkable = walkable;
    }

    public int CalculateH(int x, int y, Indexes target)
    {
        int difI = target.i - y;
        int difJ = target.j - x;

        difI = (difI < 0) ? -difI : difI;
        difJ = (difJ < 0) ? -difJ : difJ;

        return difI * _normalCost + difJ * _normalCost;
    }

    public int CalculateF(int x, int y, Indexes target) => _gCost + CalculateH(x, y, target);
}