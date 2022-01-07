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
    HealingStatue = 8
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

public struct Node
{
    public static int normalCost = 10;
    public static int diagonalCost = 14;
    public int gCost;
    public Indexes _coord;
    public Indexes _parent;

    public Node(int coordX, int coordY, int parentX = -1, int parentY = -1)
    {
        _coord = new Indexes(coordX, coordY);
        _parent = new Indexes(parentX, parentY);
        gCost = 0;
    }

    public bool SameByCoord(int x, int y) => _coord.j == x && _coord.i == y;

    public int CalculateH(Indexes target)
    {
        int difI = target.i - _coord.i;
        int difJ = target.j - _coord.j;

        difI = (difI < 0) ? -difI : difI;
        difJ = (difJ < 0) ? -difJ : difJ;

        return difI * normalCost + difJ * normalCost;
    }

    public int CalculateF(Indexes target) => gCost + CalculateH(target);
}