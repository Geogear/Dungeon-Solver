using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDisplayer : MonoBehaviour
{
    public Sprite _openTreasureSprite;
    public Vector3 _currentTreasurePos;
    public int CTPStartingEdgeMin = 2;
    public int CTPStartingEdgeMax = 3;
    public int CTPStartingColourMax = 2;

    [SerializeField] private GameObject _cTPTile;
    [SerializeField] private GameObject _displayMatrixCollider;

    private Color[] _ctpColours = {
        Color.yellow, Color.green, Color.blue,
        Color.red, Color.black, Color.white, Color.magenta,};

    /* Dividing the bg to units, to dynamically scale the displayed puzzle objects,
     and also position them correctly. */
    private const float BGXUnitCount = 27.0f;
    private const float BGYUnitCount = 19.0f;
    /* States the edge size of a matrix. TileEdgeUnit/MinPossibleEdge
     * is a constant edge length for the ctp matrix.
     * TileEdgeUnit/CurrentEdge is used to calculate the current scale.
     * To keep the entire matrix size, same with different tile counts per edge.*/
    private const float TileEdgeUnit = 2.0f;
    /* These coords are in terms of the space with size BGXUnitCount * BGYUnitCount */
    private Vector2[] _cTPDisplayCoords =
    {
        new Vector2(7, 8), new Vector2(15, 8),
        new Vector2(7, 14), new Vector2(15, 14)
    };

    /* 0th one is for the real display matrix. */
    private int[] _displayMatrixCoordIndexes = { -1, -1, -1, 1 };

    private float[] _cTPDisplaySPCoordsX =
{
        7.0f, 15.0f, 1.0f, 21.0f
    };
    private float _cTPDisplaySPCoordY = 2.0f;

    private PlayerCharacter _playerCharacter = null;
    private SpriteRenderer _spriteRenderer = null;
    private Vector3 _BGOrigin = new Vector3();

    private float _bGXUnitLen = 0.0f;
    private float _bGYUnitLen = 0.0f;   
    private bool _open = false;

    private PuzzleType _currentPuzzleType = PuzzleType.PuzzleTypeCount;

    // Start is called before the first frame update
    void Start()
    {
        _playerCharacter = GetComponentInParent<PlayerCharacter>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        /* Set unit lengths. */
        _bGXUnitLen = _spriteRenderer.size.x * transform.lossyScale.x / BGXUnitCount;
        _bGYUnitLen = _spriteRenderer.size.y * transform.lossyScale.y / BGYUnitCount;
        for(int i = 0; i < _cTPDisplayCoords.Length; ++i)
        {
            _cTPDisplayCoords[i].x *= _bGXUnitLen; _cTPDisplayCoords[i].y *= _bGYUnitLen;
        }
        for(int i = 0; i < _cTPDisplaySPCoordsX.Length; ++i)
        {
            _cTPDisplaySPCoordsX[i] *= _bGXUnitLen;
        }
        _cTPDisplaySPCoordY *= _bGYUnitLen;
        CTP.InitCTP(CTPStartingEdgeMin, CTPStartingEdgeMax, CTPStartingColourMax);
    }

    private void OpenCTPPuzzle()
    {
        /* Set and init needed. */
        SetBGOrigin();
        CTP.InitPuzzleMatrix(Treasure.GetTreasureData(_currentTreasurePos));
        SpriteRenderer cTPRenderer = _cTPTile.GetComponent<SpriteRenderer>();       
        Vector3 anchorPos = new Vector3(_BGOrigin.x, _BGOrigin.y, _BGOrigin.z);
        /* Create index list to pull from. */
        List<int> indexList = new List<int>();
        for (int i = 0; i < _cTPDisplayCoords.Length; ++i)
        {
            indexList.Add(i);
        }
        _spriteRenderer.enabled = true;

        /* Set tile size, according to matrix size. */
        _cTPTile.transform.localScale = new Vector3(TileEdgeUnit / CTP.GetEdge(true), TileEdgeUnit/CTP.GetEdge(false),
            _cTPTile.transform.localScale.z);

        /* Display the original. */
        int currentIndex = indexList[LevelGenerator.rand.Next(indexList.Count)];
        _displayMatrixCoordIndexes[0] = currentIndex;
        indexList.Remove(currentIndex);
        anchorPos.x = _BGOrigin.x + _cTPDisplayCoords[currentIndex].x;
        anchorPos.y = _BGOrigin.y - _cTPDisplayCoords[currentIndex].y;
        DisplayCTPMatrix(anchorPos, CTP._puzzleMatrix, cTPRenderer);

        /* Display the fakes. */
        while (indexList.Count > 0)
        {
            /* Get index. Set anchor. */
            currentIndex = indexList[LevelGenerator.rand.Next(indexList.Count)];
            _displayMatrixCoordIndexes[indexList.Count] = currentIndex;
            indexList.Remove(currentIndex);
            anchorPos.x = _BGOrigin.x + _cTPDisplayCoords[currentIndex].x;
            anchorPos.y = _BGOrigin.y - _cTPDisplayCoords[currentIndex].y;
            /* Fill fake then display it. */
            CTP.FillFakePuzzle();
            DisplayCTPMatrix(anchorPos, CTP._fakeMatrix, cTPRenderer);
        }

        DisplayCTPSolutionPieces(cTPRenderer);
        DisplayMatrixColliders();
    }

    private void DisplayCTPMatrix(Vector3 pos, int[,] puzzleMatrix, SpriteRenderer cTPRenderer, bool displayingPieces = false, int displayedValue = -1, int [,] mask = null)
    {
        Vector3 curPos = new Vector3();
        float ctpY = cTPRenderer.size.y * _cTPTile.transform.lossyScale.y,
            ctpX = cTPRenderer.size.x * _cTPTile.transform.lossyScale.x;
        /* Instantiates according to center position so we have to account for tile size and move the pos to up and left. */
        for (int i = 0; i < CTP.GetEdge(false); ++i)
        {
            curPos.y = pos.y - i * ctpY - ctpY/2;
            for (int j = 0; j < CTP.GetEdge(true); ++j)
            {
                if (displayingPieces && mask[i, j] != displayedValue)
                {
                    continue;
                }
                cTPRenderer.color = _ctpColours[puzzleMatrix[i, j]];
                curPos.x = pos.x + j * ctpX + ctpX/2;
                Instantiate(_cTPTile, curPos, Quaternion.identity).transform.SetParent(transform);
            }
        }
    }

    private void DisplayCTPSolutionPieces(SpriteRenderer cTPRenderer)
    {
        int[,] mask = CTP.GetSolutionPiecesMask();
        Vector3 anchorPos = new Vector3(_BGOrigin.x, _BGOrigin.y, _BGOrigin.z);

        anchorPos.y = _BGOrigin.y - _cTPDisplaySPCoordY;
        for (int i = 0; i < CTP.GetSolutionPieceCount(); ++i)
        {            
            anchorPos.x = _BGOrigin.x + _cTPDisplaySPCoordsX[i];
            DisplayCTPMatrix(anchorPos, CTP._puzzleMatrix, cTPRenderer, true, i, mask);
        }

    }

    private void DisplayMatrixColliders()
    {
        Vector3 curPos = new Vector3();
        Vector2 colliderSize = _displayMatrixCollider.GetComponent<BoxCollider2D>().size;
        for (int i = 0; i < _displayMatrixCoordIndexes.Length; ++i)
        {
            curPos.x = _BGOrigin.x + _cTPDisplayCoords[i].x;
            curPos.y = _BGOrigin.y - _cTPDisplayCoords[i].y;
            curPos.x += colliderSize.x / 2; curPos.y -= colliderSize.y / 2;
            Instantiate(_displayMatrixCollider, curPos, Quaternion.identity).transform.SetParent(transform);
        }
    }

    private void SetBGOrigin()
    {
        /* Set origin. */
        _BGOrigin.x = transform.position.x - _spriteRenderer.size.x * transform.lossyScale.x / 2;
        _BGOrigin.y = transform.position.y + _spriteRenderer.size.y * transform.lossyScale.y / 2;
        _BGOrigin.z = transform.position.z;
    }

    private bool CTPDisplaySelection(Vector2 inputPos, ref bool success)
    {
        Vector3 curPos = new Vector3();
        Vector2 colliderSize = _displayMatrixCollider.GetComponent<BoxCollider2D>().size;

        /* First for the real. */
        curPos.x = _BGOrigin.x + _cTPDisplayCoords[_displayMatrixCoordIndexes[0]].x;
        curPos.y = _BGOrigin.y - _cTPDisplayCoords[_displayMatrixCoordIndexes[0]].y;

        /* If in bounds return. */
        if ((inputPos.x >= curPos.x && inputPos.x <= curPos.x + colliderSize.x)
            && (inputPos.y <= curPos.y && inputPos.y >= curPos.y - colliderSize.y))
        {
            success = true;
            return true;
        }

        /* Check for the fake ones. */
        success = false;
        for(int i = 1; i < _displayMatrixCoordIndexes.Length; ++i)
        {
            curPos.x = _BGOrigin.x + _cTPDisplayCoords[_displayMatrixCoordIndexes[i]].x;
            curPos.y = _BGOrigin.y - _cTPDisplayCoords[_displayMatrixCoordIndexes[i]].y;
            if ((inputPos.x >= curPos.x && inputPos.x <= curPos.x + colliderSize.x)
                && (inputPos.y <= curPos.y && inputPos.y >= curPos.y - colliderSize.y))
            {
                return true;
            }
        }
        return false;
    }

    /* TODO here set currentPuzzleType, with some random logic. */
    private void DecideOnPuzzle()
    {
        _currentPuzzleType = PuzzleType.ColourTilePuzze;
    }

    public void OpenPuzzle()
    {      
        DecideOnPuzzle();
        _open = true;
        switch(_currentPuzzleType)
        {
            case PuzzleType.ColourTilePuzze:
                OpenCTPPuzzle();
                break;
        }
    }

    public void ClosePuzzle(bool success)
    {
        /* Puzzle leveling called and puzzle objects destroyed here. */
        if (success)
        {
            switch(_currentPuzzleType)
            {
                case PuzzleType.ColourTilePuzze:
                    CTP.SolvedSuccessfully();
                    break;
            }
        }
        
        GameObject[] puzzleObjects = GameObject.FindGameObjectsWithTag("Puzzle");
        foreach(GameObject puzzleObject in puzzleObjects)
        {
            Destroy(puzzleObject);
        }
        _spriteRenderer.enabled = false;
        _open = false;
    }

    /* Returns if matches with the display selection, and with the bool success sets if the match is successful. */
    public bool MatchedDisplaySelection(Vector2 inputPos, ref bool success)
    {
        bool matched = false;
        switch(_currentPuzzleType)
        {
            case PuzzleType.ColourTilePuzze:
                matched = CTPDisplaySelection(inputPos, ref success);
                break;
        }
        return matched;
    }

    public bool IsOpen() => _open;
}
