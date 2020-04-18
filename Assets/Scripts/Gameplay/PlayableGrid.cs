using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableGrid : MonoBehaviour
{
    /// <summary>
    /// width/x of the grid
    /// </summary>
    public int Columns = 6;

    /// <summary>
    /// height/y of the grid
    /// </summary>
    public int Rows = 6;

    public int PixelArtScale = 4;
    public float EditorTileSizePixels = 0.32f;

    public float GetTileScale => EditorTileSizePixels * PixelArtScale;
    public float GetTotalPaddingX => Spacing.x * Columns;
    public float GetTotalPaddingY => Spacing.y * Rows;

    public Vector2 Spacing = new Vector2(8f, 8f);

    public


    void Awake()
    {
        Service.Grid = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        for (int x = 0; x < Columns; x++)
        {
            float posX = (GetTileScale + Spacing.x) * x;
            
            for (int y = 0; y < Rows; y++)
            {
                
                float posY = (GetTileScale + Spacing.y) * y;

                Gizmos.DrawWireSphere(new Vector3(posX, posY), GetTileScale / 4);
            }
        }
    }
}
