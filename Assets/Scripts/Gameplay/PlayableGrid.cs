using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayableGrid : MonoBehaviour
{
#if UNITY_EDITOR
    public bool DrawDebug;
#endif

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

    /// <summary>
    /// List of actors currently on the grid
    /// </summary>
    [HideInInspector]
    public List<GridActor> Actors = new List<GridActor>();

    /// <summary>
    /// The current player
    /// </summary>
    [HideInInspector]
    public GridActor PlayerActor = null;

    /// <summary>
    /// Used to make it a little easier to find empty areas
    /// </summary>
    private float[,] presenceInfluenceMap;

    private const float influenceMapUpdateTime = 1f;
    private float updateInfluenceMapTimer = -2f;

    public Gradient influenceMapGradient;

    public Vector2 Spacing = new Vector2(8f, 8f);

    public Vector2 GetPlayerSpawnPosition()
    {
        return GetTileWorldPosition(Columns / 2, -4);
    }

    public Vector2 GetTileWorldPosition(Vector2Int vec)
    {
        return GetTileWorldPosition(vec.x, vec.y);
    }
    public Vector2 GetTileWorldPosition(int x, int y)
    {
        return new Vector2(Service.Grid.GetTileScale + Service.Grid.Spacing.x,
            Service.Grid.GetTileScale + Service.Grid.Spacing.y) * new Vector2Int(x, y);
    }

    public Vector2Int GetWorldTilePosition(Vector2 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt((worldPos.x / GetTileScale) + GetTileScale/2),
                              Mathf.FloorToInt((worldPos.y / GetTileScale) + GetTileScale/2));
    }
    
    public bool IsPathClearBetweenTiles(Vector2Int source, Vector2Int target)
    {
        return IsRaycastSuccessfulBetweenPoints(GetTileWorldPosition(source), GetTileWorldPosition(target));
    }

    public int GetRandomHorizontalTile()
    {
        return Random.Range(0, Columns);
    }
    public int GetRandomVerticalTile()
    {
        return Random.Range(0, Rows);
    }

    public bool IsTileOnGrid(Vector2Int vec, int threshold = 3)
    {
        return IsTileOnGrid(vec.x, vec.y, threshold);
    }
    public bool IsTileOnGrid(int x, int y, int threshold = 3)
    {
        if (x < 0 - threshold || x >= Columns + threshold)
        {
            return false;
        }

        if (y < 0 - threshold || y >= Rows + threshold)
        {
            return false;
        }

        return true;
    }


    void Awake()
    {
        Service.Grid = this;
    }

    void Start()
    {
        presenceInfluenceMap = new float[Columns, Rows];
    }

    private bool IsRaycastSuccessfulBetweenPoints(Vector2 source, Vector2 target)
    {
        var heading = target - source;
        var dist = heading.magnitude;
        var dir = heading / dist;

        var hit = Physics2D.Raycast(source, dir, dist);

        return hit.collider == null;
    }

    void Update()
    {
        updateInfluenceMapTimer += Time.deltaTime;

        if (updateInfluenceMapTimer >= influenceMapUpdateTime)
        {
            updateInfluenceMapTimer = 0f;
            UpdateInfluenceMap();
        }
    }

    void UpdateInfluenceMap()
    {
        // Just some dirty quick way to spread influence to nearest neighbours, not very good

        bool[,] dirtyFlags = new bool[Columns, Rows];

        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                if (dirtyFlags[x, y])
                {
                    continue;
                }
                if (presenceInfluenceMap[x, y] <= 0.0f)
                {
                    continue;
                }

                float totalInfl = presenceInfluenceMap[x, y];
                dirtyFlags[x, y] = true;
                int additions = 1;

                if (x > 0)
                {
                    totalInfl += presenceInfluenceMap[x-1, y];
                    dirtyFlags[x - 1, y] = true;
                    additions++;
                }

                if (x < Columns - 1)
                {
                    totalInfl += presenceInfluenceMap[x + 1, y];
                    dirtyFlags[x + 1, y] = true;
                    additions++;
                }

                if (y > 0)
                {
                    totalInfl += presenceInfluenceMap[x, y - 1];
                    dirtyFlags[x, y - 1] = true;
                    additions++;
                }

                if (y < Rows - 1)
                {
                    totalInfl += presenceInfluenceMap[x, y + 1];
                    dirtyFlags[x, y + 1] = true;
                    additions++;
                }

                totalInfl /= additions;
                totalInfl *= 0.95f;

                if (totalInfl < 0.1f)
                {
                    totalInfl = 0;
                }

                presenceInfluenceMap[x, y] = totalInfl;

                if (x > 0)
                {
                    presenceInfluenceMap[x - 1, y] = totalInfl;
                }

                if (x < Columns - 1)
                {
                    presenceInfluenceMap[x + 1, y] = totalInfl;
                }

                if (y > 0)
                {
                    presenceInfluenceMap[x, y - 1] = totalInfl;
                }

                if (y < Rows - 1)
                {
                    presenceInfluenceMap[x, y + 1] = totalInfl;
                }

            }
        }

        void AddActorToInfluenceMap(GridActor a)
        {
            var tile = GetWorldTilePosition(a.transform.position);

            if (IsTileOnGrid(tile, 0))
            {
                presenceInfluenceMap[tile.x, tile.y] = 1.0f;
            }
        }

        Actors.ForEach(x => AddActorToInfluenceMap(x));
        AddActorToInfluenceMap(PlayerActor);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!DrawDebug)
        {
            return;
        }

        for (int x = 0; x < Columns; x++)
        {
            float posX = (GetTileScale + Spacing.x) * x;
            
            for (int y = 0; y < Rows; y++)
            {
                float posY = (GetTileScale + Spacing.y) * y;

                Gizmos.DrawWireCube(new Vector3(posX, posY), new Vector3(GetTileScale, GetTileScale, GetTileScale));

                

                if (presenceInfluenceMap != null)
                {
                    var col = influenceMapGradient.Evaluate(presenceInfluenceMap[x, y]);
                    col.a = 0.25f;
                    Gizmos.color = col;

                    Gizmos.DrawCube(new Vector3(posX, posY), new Vector3(GetTileScale, GetTileScale, GetTileScale));

                    string floatString = presenceInfluenceMap[x, y].ToString("0.00");
                    Handles.Label(new Vector3(posX - (GetTileScale/2) + 0.05f, posY + (GetTileScale/2) - 0.05f), $"{floatString}");
                }

            }
        }
    }
#endif
}
