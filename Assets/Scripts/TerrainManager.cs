using System;
using UnityEngine;
using System.Collections;
using UnityEngine.WSA;

public class TerrainManager : MonoBehaviour {
    
    /// <summary>
    /// Speed at which the player travels through terrain (tiles per second)
    /// </summary>
    public float Speed = 10.0f;
    

    /// <summary>
    /// GameObject holding the generated Tilemap
    /// </summary>
    public TerrainGenerator Generator;

    /// <summary>
    /// GameObject holding the generated Tilemap
    /// </summary>
    public GameObject Tilemap;

    private float advance = 0.0f;
    private int rowIndex;
    
    private void Start() 
    {
        rowIndex = Service.Grid.Rows / 4 + 1;
    }
    

    void Update () 
    {
        if (Service.Game?.CurrentRace == null 
            || !Service.Game.CurrentRace.RaceInProgress)
        {
            for (int y = 0; y < Service.Grid.Rows; y++)
            {
                for (int x = 0; x < Service.Grid.Columns; x++)
                {
                    Service.Grid.TerrainCollisions[x, y] = false;
                }
            }

            return;
        }

        if (Tilemap != null) 
        {
            Tilemap.transform.Translate(Time.deltaTime * Speed * Vector3.down);
        }

        advance += Time.deltaTime * Speed;

        if (advance >= 1.0f) {
            advance = advance - 1.0f;
            rowIndex++;

            if (Service.Grid)
            {
                for (int y = 0; y < Service.Grid.Rows; y++)
                {
                    for (int x = 0; x < Service.Grid.Columns; x++)
                    {
                        /* TerrainMap is half the size of the actual map */
                        int ix = ((x - Service.Grid.Columns / 2) + Generator.TerrainWidth) / 2;
                        int iy = (y + rowIndex) / 2;

                        var data = Generator.TerrainMap[Generator.TerrainLength-1, ix];

                        if (ix > 0 && ix < Generator.TerrainWidth &&
                            iy > 0 && iy < Generator.TerrainLength)
                        {
                            var val = Generator.TerrainMap[Generator.TerrainLength - iy, ix];
                            Service.Grid.TerrainCollisions[x, y] = val > 0;
                        }
                        else
                        {
                            Service.Grid.TerrainCollisions[x, y] = false;
                        }
                    }
                }
            }
        }

    }
    
}