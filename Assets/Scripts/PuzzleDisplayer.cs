using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDisplayer : MonoBehaviour
{
    /* TODO instantiated as a child of the player. 
       + have gameobject fields for ctp tiles. */
    [SerializeField] private GameObject _cTPTile;
    private Color[] _ctpColours = {
        Color.yellow, Color.green, Color.blue,
        Color.red, Color.black, Color.white, Color.magenta,};

    // Start is called before the first frame update
    void Start()
    {  
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
        Vector3 origin = new Vector3();
        SpriteRenderer cTPRenderer = _cTPTile.GetComponent<SpriteRenderer>();
        for (int i = 0; i < CTP.GetEdge(false); ++i)
        {
            origin.y = transform.position.y - i * cTPRenderer.size.y * _cTPTile.transform.lossyScale.y;
            for (int j = 0; j < CTP.GetEdge(true); ++j)
            {
                cTPRenderer.color = _ctpColours[CTP._puzzleMatrix[i, j]];
                origin.x = transform.position.x + j * cTPRenderer.size.x * _cTPTile.transform.lossyScale.x;
                Instantiate(_cTPTile, origin, Quaternion.identity);
            }
        }
        
    }

    private void ClosePuzzle()
    {
        /* TODO Destroy by tag "Puzzle". */
    }

    public void OpenPuzzle()
    {
        OpenCTPPuzzle();
    }
}
