using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDisplayer : MonoBehaviour
{
    /* TODO instantiated as a child of the player. 
       + have gameobject fields for ctp tiles. */
    [SerializeField] private GameObject _cTPTile;
    [SerializeField] private Transform _puzzleOptionsAnchor;

    private Color[] _ctpColours = {
        Color.yellow, Color.green, Color.blue,
        Color.red, Color.black, Color.white, Color.magenta,};

    private SpriteRenderer _spriteRenderer = null;

    private float constantGap = 1.0f;
    private bool _success = false;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OpenCTPPuzzle()
    {
        if (!CTP.IsInit())
        {
            CTP.InitCTP();
        }
        CTP.InitPuzzleMatrix();
        SpriteRenderer cTPRenderer = _cTPTile.GetComponent<SpriteRenderer>();
        Vector3 anchorPos = new Vector3(_puzzleOptionsAnchor.transform.position.x,
            _puzzleOptionsAnchor.transform.position.y, _puzzleOptionsAnchor.transform.position.z);
        _spriteRenderer.enabled = true;

        /* Display the original. */
        DisplayCTPMatrix(anchorPos, CTP._puzzleMatrix, cTPRenderer);

        /* Display the fakes. */
        CTP.FillFakePuzzle();
        anchorPos.x += CTP.GetEdge(true) * cTPRenderer.size.x * _cTPTile.transform.lossyScale.x + constantGap;
        DisplayCTPMatrix(anchorPos, CTP._fakeMatrix, cTPRenderer);

        CTP.FillFakePuzzle();
        anchorPos.y -= CTP.GetEdge(false) * cTPRenderer.size.y * _cTPTile.transform.lossyScale.y + constantGap;
        DisplayCTPMatrix(anchorPos, CTP._fakeMatrix, cTPRenderer);

        CTP.FillFakePuzzle();
        anchorPos.x = _puzzleOptionsAnchor.transform.position.x;
        DisplayCTPMatrix(anchorPos, CTP._fakeMatrix, cTPRenderer);
    }

    private void DisplayCTPMatrix(Vector3 pos, int[,] puzzleMatrix, SpriteRenderer cTPRenderer)
    {     
        Vector3 curPos = new Vector3();
        for (int i = 0; i < CTP.GetEdge(false); ++i)
        {
            curPos.y = pos.y - i * cTPRenderer.size.y * _cTPTile.transform.lossyScale.y;
            for (int j = 0; j < CTP.GetEdge(true); ++j)
            {
                cTPRenderer.color = _ctpColours[puzzleMatrix[i, j]];
                curPos.x = pos.x + j * cTPRenderer.size.x * _cTPTile.transform.lossyScale.x;
                Instantiate(_cTPTile, curPos, Quaternion.identity);
            }
        }
    }

    public void OpenPuzzle()
    {
        OpenCTPPuzzle();
    }

    public void ClosePuzzle()
    {
        /* TODO Destroy by tag "Puzzle". */
        _spriteRenderer.enabled = false;
    }

    public bool IsSuccess() => _success;
}
