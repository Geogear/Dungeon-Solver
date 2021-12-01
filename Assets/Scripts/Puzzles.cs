public static class CTP
{
    private static int _currentColourMax = 1;
    private static int[] _edgeIncreaseAmount;
    private static int[] _edgeIncreaseAmountWeights;
    private static int[] _currentEdges;
    private static int[] _currentMinMax;
    private static int[,] _puzzleMatrix;

    public static void InitCTP(int min = 2, int max = 3)
    {
        _edgeIncreaseAmount = new int[2] { 1, 2 };
        _edgeIncreaseAmountWeights = new int[2] { 70, 30 };
        _currentEdges = new int[2] { LevelGenerator.rand.Next(min, max + 1), LevelGenerator.rand.Next(min, max + 1) };
        _currentMinMax = new int[2] { min, max };
        _puzzleMatrix = null;
    }

    public static void IncreaseEdgeMinMax()
    {
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
        _currentEdges[0] = LevelGenerator.rand.Next(_currentMinMax[0], _currentMinMax[1] + 1);
        _currentEdges[1] = LevelGenerator.rand.Next(_currentMinMax[0], _currentMinMax[1] + 1);
        _puzzleMatrix = new int[_currentEdges[0], _currentEdges[1]];
        for (int i = 0; i < _currentEdges[0]; ++i)
        {
            for (int j = 0; j < _currentEdges[1]; ++j)
            {
                _puzzleMatrix[i, j] = LevelGenerator.rand.Next(_currentColourMax, _currentColourMax + 1);
            }
        }
    }

    public static int GetEdge(bool width) => (width) ? _currentEdges[1] : _currentEdges[0];
}