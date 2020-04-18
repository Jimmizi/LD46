using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridActor : MonoBehaviour
{
    private Vector2Int targetGridPosition;
    private Vector2 targetActualPosition;

    public Vector2Int TargetPosition
    {
        get => targetGridPosition;
        set
        {
            targetGridPosition = value;
            targetActualPosition = new Vector2(Service.Grid.GetTileScale + Service.Grid.Spacing.x, 
                                                Service.Grid.GetTileScale + Service.Grid.Spacing.y) * targetGridPosition;
        }
    }

    public void AddDirection(int x, int y)
    {
        Vector2Int target = targetGridPosition + new Vector2Int(x, y);


        if (target.x < 0 || target.y < 0
                         || target.x >= Service.Grid.Columns
                         || target.y >= Service.Grid.Rows)
        {
            return;
        }

        TargetPosition = target;
    }

    
    void Start()
    {
        // Not a very precise way of grabbing the nearest grid position but good enough for now
        int posX = Mathf.FloorToInt(transform.position.x / Service.Grid.GetTileScale);
        int posY = Mathf.FloorToInt(transform.position.y / Service.Grid.GetTileScale);

        TargetPosition = new Vector2Int(posX, posY);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetActualPosition, Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetActualPosition);
    }
}