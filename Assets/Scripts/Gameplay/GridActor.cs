using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GridActor : MonoBehaviour
{
    /// <summary>
    /// When strafing left or right, how much will the actor angle/tilt in that direction
    /// </summary>
    public float StrafeAngleAmount;
    public AnimationCurve StrafeAngleCurve;

    public float MoveSpeed;
    public AnimationCurve MoveSpeedCurve;


    //

    private Vector2Int targetGridPosition;

    private struct MovementData
    {
        public Vector2 TargetWorldPosition;
        public float StartingDistance;
        
        /// <summary>
        /// Direction of turning. -1 = left. 0 = forward/backward. 1 = right.
        /// </summary>
        public int TurnDirection;

    }
    private MovementData CurrentMove = new MovementData();

    public Vector2Int TargetPosition
    {
        get => targetGridPosition;
        set
        {
            //We're turning left
            if (value.x < targetGridPosition.x)
            {
                CurrentMove.TurnDirection = -1;
            }
            //We're turning right
            else if (value.x > targetGridPosition.x)
            {
                CurrentMove.TurnDirection = 1;
            }
            else //Going forward/backward
            {
                CurrentMove.TurnDirection = 0;
            }

            targetGridPosition = value;
            CurrentMove.TargetWorldPosition = new Vector2(Service.Grid.GetTileScale + Service.Grid.Spacing.x, 
                                                Service.Grid.GetTileScale + Service.Grid.Spacing.y) * targetGridPosition;

            CurrentMove.StartingDistance = Vector2.Distance(transform.position, CurrentMove.TargetWorldPosition);


        }
    }

    /// <summary>
    /// Move in this direction and add it to the target position
    /// </summary>
    public void MoveInDirection(int x, int y)
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

    private void AssignToNearestGridPoint()
    {
        // Not a very precise way of grabbing the nearest grid position but good enough for now
        int posX = Mathf.FloorToInt(transform.position.x / Service.Grid.GetTileScale);
        int posY = Mathf.FloorToInt(transform.position.y / Service.Grid.GetTileScale);

        TargetPosition = new Vector2Int(posX, posY);
    }
    
    void Start()
    {
        AssignToNearestGridPoint();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessAngling();
        transform.position = Vector3.Lerp(transform.position, CurrentMove.TargetWorldPosition, MoveSpeed * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, CurrentMove.TargetWorldPosition);
    }


    private void ProcessAngling()
    {
        var dist = Vector2.Distance(transform.position, CurrentMove.TargetWorldPosition);
        var mod = dist / CurrentMove.StartingDistance;

        var turnAmount = StrafeAngleCurve.Evaluate(1f - mod);
        
        // Negative Z is right, Positive Z is left
        var heading = transform.eulerAngles;
        heading.z = (StrafeAngleAmount * turnAmount) * -CurrentMove.TurnDirection;
        transform.eulerAngles = heading;
    }
}