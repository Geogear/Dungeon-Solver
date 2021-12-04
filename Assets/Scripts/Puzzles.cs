public static class CTP
{
    private static bool _init = false;
    private static int _exp = 0;
    private static int _solutionPieces = 2;
    private static int _currentColourMax = 1;
    private static int[] _edgeIncreaseAmount;
    private static int[] _edgeIncreaseAmountWeights;
    private static int[] _currentEdges;
    private static int[] _currentMinMax;

    public static int[,] _puzzleMatrix;
    public static int[,] _fakeMatrix;

    private static void FillMatrix(bool fake)
    {
        int[,] currentMatrix = fake ? _fakeMatrix : _puzzleMatrix;

        /* Put a random colour at each tile. TODO this should be smarter. */
        for (int i = 0; i < _currentEdges[0]; ++i)
        {
            for (int j = 0; j < _currentEdges[1]; ++j)
            {
                currentMatrix[i, j] = LevelGenerator.rand.Next(_currentColourMax + 1);
            }
        }
    }

    public static void InitCTP(int min = 2, int max = 3)
    {
        _edgeIncreaseAmount = new int[2] { 1, 2 };
        _edgeIncreaseAmountWeights = new int[2] { 80, 20 };
        _currentEdges = new int[2] { LevelGenerator.rand.Next(min, max + 1), LevelGenerator.rand.Next(min, max + 1) };
        _currentMinMax = new int[2] { min, max };
        _puzzleMatrix = null;
        _init = true;
    }

    public static void IncreaseEdgeMinMax()
    {
        /* Increase min or max, if min increased and min surpassed max, swap them. */
        int index = LevelGenerator.rand.Next() % 2;
        _currentMinMax[index] += _edgeIncreaseAmount[LevelGenerator.GetWeightedRandom(_edgeIncreaseAmountWeights)];
        if (index == 0 && _currentMinMax[0] > _currentMinMax[1])
        {
            int tmp = _currentMinMax[0];
            _currentMinMax[0] = _currentMinMax[1];
            _currentMinMax[1] = tmp;
        }
    }

    public static void InitPuzzleMatrix()
    {
        /* Get random edge lengths. Init the matrixes. */
        _currentEdges[0] = LevelGenerator.rand.Next(_currentMinMax[0], _currentMinMax[1] + 1);
        _currentEdges[1] = LevelGenerator.rand.Next(_currentMinMax[0], _currentMinMax[1] + 1);
        _puzzleMatrix = new int[_currentEdges[0], _currentEdges[1]];
        _fakeMatrix = new int[_currentEdges[0], _currentEdges[1]];

        FillMatrix(false);
    }

    public static void FillFakePuzzle()
    {
        /* TODO, check for not same with the actual puzzle. */
        FillMatrix(true);
    }

    public static void SolvedSuccessfully()
    {
        /* Increase experience. */
        _exp += LevelGenerator.rand.Next(1, _currentEdges[LevelGenerator.rand.Next()%2] + 1);
        if (_exp >= _currentEdges[0] * _currentEdges[0]* 1)
        {
            IncreaseEdgeMinMax();
        }
    }

    public static int GetEdge(bool width) => (width) ? _currentEdges[1] : _currentEdges[0];
    public static int GetSolutionPieceCount() => _solutionPieces;
    public static bool IsInit() => _init;
}