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

    public static int [,] GetSolutionPiecesMask()
    {
        int[,] mask = new int[_currentEdges[0], _currentEdges[1]];
        int nonMarked = _currentEdges[0] * _currentEdges[1], simplePartition = nonMarked / _solutionPieces,
            amountToMark = 0;
        nonMarked -= (int)(simplePartition * LevelGenerator.RandomFloat(0.85f, 1.0f));

        for (int p = 1; p < _solutionPieces; ++p)
        {
            amountToMark = (simplePartition >= nonMarked) ? nonMarked :
                (int)(simplePartition * LevelGenerator.RandomFloat(0.85f, 1.0f));
            nonMarked -= amountToMark;

            /* Select starting index for markings. */
            Indexes startingIndexes = new Indexes(LevelGenerator.rand.Next(_currentEdges[1]),
                LevelGenerator.rand.Next(_currentEdges[0]));
            while (mask[startingIndexes.i, startingIndexes.j] != 0)
            {
                startingIndexes.i = LevelGenerator.rand.Next(_currentEdges[0]);
                startingIndexes.j = LevelGenerator.rand.Next(_currentEdges[1]);
            }
            mask[startingIndexes.i, startingIndexes.j] = p;
            --amountToMark;

            /* Init needed lists. */
            System.Collections.Generic.List<Indexes> markingOrigins = new System.Collections.Generic.List<Indexes>();
            System.Collections.Generic.List<Direction> directions = new System.Collections.Generic.List<Direction>();

            for (int i = 0; i < (int)Direction.DirectionCount; ++i)
            {
                directions.Add((Direction)i);
            }
            markingOrigins.Clear();
            markingOrigins.Add(startingIndexes);
            /* Do markings. For every mark, start with the origin, look at 4 directions, in random order
             if empty, fill and add that point to the origin points list. */
            while (markingOrigins.Count > 0 && amountToMark > 0)
            {
                while (directions.Count > 0 && amountToMark > 0)
                {
                    int currentDirectionIndex = LevelGenerator.rand.Next(directions.Count),
                        differinceI = 0, differinceJ = 0;
                    switch (directions[currentDirectionIndex])
                    {
                        case Direction.Up:
                            if (markingOrigins[0].i > 0
                                && mask[markingOrigins[0].i - 1, markingOrigins[0].j] == 0)
                            {
                                differinceI = -1;
                            }
                            break;
                        case Direction.Down:
                            if (markingOrigins[0].i < _currentEdges[0] - 1
                                && mask[markingOrigins[0].i + 1, markingOrigins[0].j] == 0)
                            {
                                differinceI = 1;
                            }
                            break;
                        case Direction.Left:
                            if (markingOrigins[0].j > 0
                                && mask[markingOrigins[0].i, markingOrigins[0].j - 1] == 0)
                            {
                                differinceJ = -1;
                            }
                            break;
                        case Direction.Right:
                            if (markingOrigins[0].j < _currentEdges[1] - 1 &&
                                mask[markingOrigins[0].i, markingOrigins[0].j + 1] == 0)
                            {
                                differinceJ = 1;
                            }
                            break;
                    }
                    if (differinceI != 0 || differinceJ != 0)
                    {
                        mask[markingOrigins[0].i + differinceI, markingOrigins[0].j + differinceJ] = p;
                        markingOrigins.Add(new Indexes(markingOrigins[0].j + differinceJ, markingOrigins[0].i + differinceI));
                        --amountToMark;
                    }
                    directions.RemoveAt(currentDirectionIndex);
                }
                markingOrigins.RemoveAt(0);
            }
        }

        return mask;
    }

    public static int GetEdge(bool width) => (width) ? _currentEdges[1] : _currentEdges[0];
    public static int GetSolutionPieceCount() => _solutionPieces;
    public static bool IsInit() => _init;
}