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
    /// PlayableGrid reference used in conveying terrain collisions
    /// </summary>
    public PlayableGrid Grid;

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
    
    private void Start() {
        rowIndex = Grid.Rows / 4 + 1;
    }
    

    void Update () {
        if (Tilemap != null) {
            Tilemap.transform.Translate(Time.deltaTime * Speed * Vector3.down);
        }

        advance += Time.deltaTime * Speed;

        if (advance >= 1.0f) {
            advance = advance - 1.0f;
            rowIndex++;

            for (int y = 0; y < Grid.Rows; y++) {
                for (int x = 0; x < Grid.Columns; x++) {
                    /* TerrainMap is half the size of the actual map */
                    int ix = ((x - Grid.Columns / 2) + Generator.TerrainWidth) / 2; 
                    int iy = (y + rowIndex) / 2;

                    if (ix > 0 && ix < Generator.TerrainWidth &&
                        iy > 0 && iy < Generator.TerrainLength) {

                        Grid.TerrainCollisions[x, y] = Generator.TerrainMap[Generator.TerrainLength - iy, ix] > 0;
                    } else {
                        Grid.TerrainCollisions[x, y] = false;
                    }
                }
            }
        }

    }
    
}