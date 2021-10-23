using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    /* 1 for tile, 2 for door. The room is a rectangle and can rotate 90 degrees. */
    private readonly int[] EntranceRoom =
        {1, 2, 1,
         1, 1, 1};

    private readonly int[] ExitRoom =
        {1, 2, 1,
         1, 1, 1};

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateLevel()
    {

    }
}
