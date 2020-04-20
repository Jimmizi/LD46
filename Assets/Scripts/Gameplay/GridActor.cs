using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GridActor : MonoBehaviour
{
    public EventHandler OnActorDestroy;

    /// <summary>
    /// When strafing left or right, how much will the actor angle/tilt in that direction
    /// </summary>
    [SerializeField]
    public float StrafeAngleAmount;

    [SerializeField]
    public AnimationCurve StrafeAngleCurve;

    [SerializeField]
    public float MoveSpeed;
    
    private Vector2Int targetGridPosition = new Vector2Int(-1, -1);

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

    public Vector2Int CurrentTile => Service.Grid.GetWorldTilePosition(transform.position);

    public bool LockTargetPosition;

    public bool UseLinearMovementSpeed;

    //If this actor reaches their target position, they will destroy themselves
    public bool SelfDestroyOnTargetReached;

    //If this actor reaches their target position, they will destroy themselves, BUT only if this target position is not on the board
    public bool SelfDestroyOnTargetReached_OnlyIfNotOnBoard;

    /// <summary>
    /// Target position in grid tiles
    /// </summary>
    public Vector2Int TargetPosition
    {
        get => targetGridPosition;
        set
        {
            if (LockTargetPosition)
            {
                return;
            }

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

    private bool doingObstacleShake;

    /// <summary>
    /// Move in this direction and add it to the target position
    /// </summary>
    public void MoveInDirection(int x, int y)
    {
        if (LockTargetPosition)
        {
            return;
        }

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
        //If it has been assigned to don't override
        if (TargetPosition == new Vector2Int(-1, -1))
        {
            TargetPosition = Service.Grid.GetWorldTilePosition(transform.position);
        }
    }
    
    protected void Start()
    {
        AssignToNearestGridPoint();
    }

    void OnDestroy()
    {
        OnActorDestroy?.Invoke(this, new EventArgs());
    }

    public void PushDown()
    {
        MoveInDirection(0, -1);
    }

    // Update is called once per frame
    protected void Update()
    {
        if (Service.Game && !Service.Game.IsRaceInProgress())
        {
            return;
        }

        if (Service.Grid && Service.Grid.IsWorldPositionOnTerrain(this.transform.position))
        {
            var healthComp = GetComponent<HealthComponent>();
            if (healthComp)
            {
                healthComp.Offset(-100);
            }
        }

        ProcessAngling();

        if (!UseLinearMovementSpeed)
        {
            transform.position = Vector3.Lerp(transform.position, CurrentMove.TargetWorldPosition, MoveSpeed * Time.deltaTime * GameplayManager.GlobalTimeMod);
        }
        else
        {
            var heading = CurrentMove.TargetWorldPosition - new Vector2(transform.position.x, transform.position.y);
            var dist = heading.magnitude;
            var dir = heading / dist;

            var dir3 = new Vector3(dir.x, dir.y, 0);

            transform.position += dir3 * (MoveSpeed * Time.deltaTime * GameplayManager.GlobalTimeMod);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, CurrentMove.TargetWorldPosition);
    }

    public void SetJustHitObstacle()
    {
        if (!doingObstacleShake)
        {
            StartCoroutine(DoObstacleShake());
        }
    }

    IEnumerator DoObstacleShake()
    {
        doingObstacleShake = true;
        int angleChangesDone = 0;
        float time = 0;

        const float angleInterval = 0.075f;

        int dir = -1;

        while (angleChangesDone < 8)
        {
            time += Time.deltaTime * GameplayManager.GlobalTimeMod;

            var mod = time / angleInterval;

            // Negative Z is right, Positive Z is left
            var heading = transform.eulerAngles;
            heading.z = 5 * dir;
            transform.eulerAngles = heading;

            if (time > angleInterval)
            {
                dir = -dir;
                angleChangesDone++;
                time = 0;
            }
            yield return null;
        }

        var heading1 = transform.eulerAngles;
        heading1.z = 0;
        transform.eulerAngles = heading1;
        doingObstacleShake = false;

    }

    private void ProcessAngling()
    {
        if (doingObstacleShake)
        {
            return;
        }

        var dist = Vector2.Distance(transform.position, CurrentMove.TargetWorldPosition);
        var mod = dist / CurrentMove.StartingDistance;

        var turnAmount = StrafeAngleCurve.Evaluate(1f - mod);
        
        // Negative Z is right, Positive Z is left
        var heading = transform.eulerAngles;
        heading.z = (StrafeAngleAmount * turnAmount) * -CurrentMove.TurnDirection;
        transform.eulerAngles = heading;

        //Arbitrary threshold
        if (mod <= 0.02f)
        {
            if (SelfDestroyOnTargetReached)
            {
                if (!SelfDestroyOnTargetReached_OnlyIfNotOnBoard
                    || !Service.Grid.IsTileOnGrid(targetGridPosition))
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}