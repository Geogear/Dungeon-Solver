using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDisplayer : MonoBehaviour
{
    /* TODO as the ctp tiles increase, starting point of the matrixes go down. */

    public Vector3 _currentTreasurePos;
    public int CTPStartingEdgeMin = 2;
    public int CTPStartingEdgeMax = 3;
    public int CTPStartingColourMax = 1;
    public int CTPSolutionPieceCount = 2;

    [SerializeField] private GameObject _cTPTile;
    [SerializeField] private Transform _puzzleOptionsAnchor;

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

    private float[] _cTPDisplaySPCoordsX =
{
        8.0f, 15.0f, 1.0f, 22.0f
    };
    private float _cTPDisplaySPCoordY = 2.0f;

    private PlayerCharacter _playerCharacter = null;
    private SpriteRenderer _spriteRenderer = null;
    private Vector3 _BGOrigin = new Vector3();

    private float _bGXUnitLen = 0.0f;
    private float _bGYUnitLen = 0.0f;
    private bool _success = false;
    private bool _open = false;

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
    }

    // Update is called once per frame
    void Update()
    {
        /* TODO, if displaying puzzle check for ray hit, and call on success for the puzzle on right.
         * Treasure reward and punish has to be called from here. 
        if (_spriteRenderer.enabled)
        {
            CTP.SolvedSuccessfull();
        }*/

    }

    private void OpenCTPPuzzle()
    {
        if (!CTP.IsInit())
        {
            CTP.InitCTP(CTPStartingEdgeMin, CTPStartingEdgeMax, CTPStartingColourMax, CTPSolutionPieceCount);
        }
        /* Set and init needed. */
        SetBGOrigin();
        CTP.InitPuzzleMatrix();
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
        _cTPTile.GetComponent<UnityEngine.UI.Text>().text = "Real";
        int currentIndex = indexList[LevelGenerator.rand.Next(indexList.Count)];
        indexList.Remove(currentIndex);
        anchorPos.x = _BGOrigin.x + _cTPDisplayCoords[currentIndex].x;
        anchorPos.y = _BGOrigin.y - _cTPDisplayCoords[currentIndex].y;
        DisplayCTPMatrix(anchorPos, CTP._puzzleMatrix, cTPRenderer);

        /* Display the fakes. */
        _cTPTile.GetComponent<UnityEngine.UI.Text>().text = "Fake";
        while (indexList.Count > 0)
        {
            /* Get index. Set anchor. */
            currentIndex = indexList[LevelGenerator.rand.Next(indexList.Count)];
            indexList.Remove(currentIndex);
            anchorPos.x = _BGOrigin.x + _cTPDisplayCoords[currentIndex].x;
            anchorPos.y = _BGOrigin.y - _cTPDisplayCoords[currentIndex].y;
            /* Fill fake then display it. */
            CTP.FillFakePuzzle();
            DisplayCTPMatrix(anchorPos, CTP._fakeMatrix, cTPRenderer);
        }

        _cTPTile.GetComponent<UnityEngine.UI.Text>().text = "";
        DisplayCTPSolutionPieces(cTPRenderer);
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

    private void SetBGOrigin()
    {
        /* Set origin. */
        _BGOrigin.x = transform.position.x - _spriteRenderer.size.x * transform.lossyScale.x / 2;
        _BGOrigin.y = transform.position.y + _spriteRenderer.size.y * transform.lossyScale.y / 2;
        _BGOrigin.z = transform.position.z;
    }

    private void DisplayPieces()
    {

    }

    public void OpenPuzzle()
    {
        OpenCTPPuzzle();
        _open = true;
    }

    public void ClosePuzzle()
    {
        /* TODO Destroy by tag "Puzzle". */
        GameObject[] puzzleObjects = GameObject.FindGameObjectsWithTag("Puzzle");
        foreach(GameObject puzzleObject in puzzleObjects)
        {
            Destroy(puzzleObject);
        }
        _spriteRenderer.enabled = false;
        _open = false;
    }

    public bool IsSuccess() => _success;
    public bool IsOpen() => _open;
}
