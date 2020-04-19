using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class RaceFinishEventArgs : EventArgs
{
    public RaceFinishEventArgs(bool win, string failreason = "")
    {
        Win = win;
        FailReason = failreason;
    }

    public bool Win;
    public string FailReason;
}


/// <summary>
/// Manages the current race, spawning in of obstacles and enemies during it. Handles post-race checkpoint scene as well.
/// </summary>
public class RaceCoordinator : MonoBehaviour
{
    private enum RaceState
    {
        IntroToRace,
        Racing,
        OutroToCheckpoint,
        OutroToCheckpoint_Wait,
        CheckpointFadeIn,
        Checkpoint,
        CheckpointFadeOut,
        End,

        Invalid
    }

    public enum SpawnLocation
    {
        Back,
        Front,
        Sides
    }

    public bool DebugIgnoreRaceTimer = false;
    public bool DebugStopObstacleSpawning;
    public bool DebugSpawnEnemy = false;
    public bool DebugSpawnObstacle;

    public float ObstacleSpawnTimer = 0;
    public float TimeUntilNextObstacle = 5;

    public int MaxNumberOfObstacles = 4;
    public int CurrentNumberOfObstacles;


    public SpawnLocation NextEnemySpawnLocation;

    public EventHandler OnRaceFinished;

    [HideInInspector]
    public GameObject PlayerGameObject = null;
    [HideInInspector]
    public GameObject PlayerCheckpointGameObject = null;
    [HideInInspector]
    public GameObject CheckpointScoreUi = null;

    /// <summary>
    /// Length of this part of the race, in seconds
    /// </summary>
    [HideInInspector]
    public float RaceLengthTimer = 10;

    /// <summary>
    /// Current time of the race
    /// </summary>
    [HideInInspector]
    public float RaceTime;

    [HideInInspector]
    public bool RaceInProgress;

    private RaceState stage = RaceState.IntroToRace;

    public void Update()
    {
        switch (stage)
        {
            //Creates the player and brings them into the scene
            case RaceState.IntroToRace:
                UpdateRaceIntro();
                break;

            //Wait until the race timer is up
            case RaceState.Racing:
                UpdateRace();
                break;

            //Move the player out of frame and fade out
            case RaceState.OutroToCheckpoint:
                UpdateRaceOutro();
                break;

            //Show the checkpoint scene
            case RaceState.Checkpoint:
                UpdateCheckpoint();
                break;

            //Finish the race and let the gameplay manager relaunch
            case RaceState.End:

                Destroy(PlayerGameObject.gameObject);

                PlayerGameObject = null;
                Service.Grid.PlayerActor = null;

                OnRaceFinished?.Invoke(this, new RaceFinishEventArgs(true));
                stage++;

                break;
        }
    }

    void UpdateRaceIntro()
    {
        if (PlayerGameObject == null)
        {
            Service.Game.FadeInIfBackedOut();

            PlayerGameObject = (GameObject)Instantiate(Service.Prefab.PlayerActor);
            PlayerGameObject.transform.position = Service.Grid.GetPlayerSpawnPosition();

            Service.Grid.PlayerActor = PlayerGameObject.GetComponent<GridActor>();

            Service.Grid.PlayerActor.TargetPosition = new Vector2Int(Service.Grid.Columns / 2, Service.Grid.Rows / 2);

            Assert.IsNotNull(Service.Grid.PlayerActor);

            //Move onto the racing stage
            stage = RaceState.Racing;
        }
    }

    void UpdateRace()
    {
#if UNITY_EDITOR

        if (DebugSpawnEnemy)
        {
            DebugSpawnEnemy = false;
            SpawnNewEnemy(NextEnemySpawnLocation);
        }

        if (DebugSpawnObstacle)
        {
            DebugSpawnObstacle = false;
            SpawnNewObstacle();
        }
#endif

        //While racing, add onto the race time and wait for it to expire
        RaceTime += Time.deltaTime;
        ObstacleSpawnTimer += Time.deltaTime;

#if UNITY_EDITOR
        if (DebugStopObstacleSpawning)
        {
            ObstacleSpawnTimer = 0;
        }
        if (DebugIgnoreRaceTimer)
        {
            RaceTime = 0;
        }
#endif

        if (ObstacleSpawnTimer >= TimeUntilNextObstacle)
        {
            ObstacleSpawnTimer = 0;
            SpawnNewObstacle();
        }

        if (RaceTime >= RaceLengthTimer)
        {
            stage = RaceState.OutroToCheckpoint;
        }
    }

    void UpdateRaceOutro()
    {
        //Once faded out the screen, we want to fade it back into the checkpoint
        void OnFadedOutFromRace(object sender, EventArgs e)
        {
            //Once faded into the checkpoint, we want to update the checkpoint
            void OnFadedIntoCheckpoint(object sender1, EventArgs e1)
            {
                // ReSharper disable once DelegateSubtraction
                // Move onto the checkpoint update stage
                Service.Game.OnFadeCoroutineComplete -= OnFadedIntoCheckpoint;
                stage = RaceState.Checkpoint;
            }

            foreach (var actor in Service.Grid.Actors)
            {
                if (actor is EnemyController enemy)
                {
                    Destroy(enemy.gameObject);
                }
            }

            Service.Grid.Actors.Clear();

            // ReSharper disable once DelegateSubtraction
            // Move onto the fade into checkpoint stage
            Service.Game.OnFadeCoroutineComplete -= OnFadedOutFromRace;
            stage = RaceState.CheckpointFadeIn;

            //create the gameobject for the player resting on the checkpoint screen. includes ui
            PlayerCheckpointGameObject = (GameObject) Instantiate(Service.Prefab.PlayerActorResting);
            CheckpointScoreUi = (GameObject) Instantiate(Service.Prefab.CheckpointUi);

            //Then launch a fade in
            Service.Game.OnFadeCoroutineComplete += OnFadedIntoCheckpoint;
            Service.Game.StartFader(false);
        }

        // Race to outro, move the player off screen
        Service.Grid.PlayerActor.TargetPosition = new Vector2Int(Service.Grid.Columns / 2, Service.Grid.Rows + 4);
        Service.Grid.PlayerActor.LockTargetPosition = true;

        stage = RaceState.OutroToCheckpoint_Wait;

        //Then start a fade out on the screen
        Service.Game.OnFadeCoroutineComplete += OnFadedOutFromRace;
        Service.Game.StartFader(true);
    }

    void UpdateCheckpoint()
    {
        void OnFadedOutFromCheckpoint(object sender, EventArgs e)
        {
            Destroy(PlayerCheckpointGameObject);
            Destroy(CheckpointScoreUi);

            // ReSharper disable once DelegateSubtraction
            // Move onto the End stage
            Service.Game.OnFadeCoroutineComplete -= OnFadedOutFromCheckpoint;
            stage = RaceState.End;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            stage = RaceState.CheckpointFadeOut;

            Service.Game.OnFadeCoroutineComplete += OnFadedOutFromCheckpoint;
            Service.Game.StartFader(true);
        }
    }


    #region Spawning functions

    void SpawnNewObstacle()
    {
        if (CurrentNumberOfObstacles >= MaxNumberOfObstacles)
        {
            return;
        }

        var gridPosition = new Vector2Int();

        gridPosition.x = Service.Grid.GetRandomHorizontalTile();
        gridPosition.y = Service.Grid.Rows + Random.Range(2, 10);

        var obstacle = (GameObject)Instantiate(Service.Prefab.ObstacleActor);
        obstacle.transform.position = Service.Grid.GetTileWorldPosition(gridPosition);

        var grid = obstacle.GetComponent<GridActor>();

        Assert.IsNotNull(grid);

        grid.TargetPosition = new Vector2Int(gridPosition.x, -5);
        grid.UseLinearMovementSpeed = true;
        grid.LockTargetPosition = true;
        grid.SelfDestroyOnTargetReached = true;

        CurrentNumberOfObstacles++;
        grid.OnActorDestroy += (sender, args) =>
        {
            CurrentNumberOfObstacles--;
        };

    }

    void SpawnNewEnemy(SpawnLocation loc)
    {
        var gridPosition = new Vector2Int();
        var targetGridPosition = new Vector2Int();

        int locSide = 0;

        //Get a spawn location first
        switch (loc)
        {
            case SpawnLocation.Back:
                targetGridPosition = Service.Grid.GetLeastCrowdedTileInRange(new Vector2Int(0, 0),
                                        new Vector2Int(Service.Grid.Columns - 1, 1), new Vector2Int(), true);
                break;
            case SpawnLocation.Front:
                targetGridPosition = Service.Grid.GetLeastCrowdedTileInRange(new Vector2Int(0, Service.Grid.Rows - 2),
                                        new Vector2Int(Service.Grid.Columns - 1, Service.Grid.Rows-1), new Vector2Int(), false, true);
                break;
            case SpawnLocation.Sides:

                locSide = Random.Range(0, 2);
                targetGridPosition = Service.Grid.GetLeastCrowdedTileInRange(new Vector2Int(locSide == 0 ? 0 : Service.Grid.Columns - 2, 0),
                                        new Vector2Int(Service.Grid.Columns - 1, Service.Grid.Rows - 1), new Vector2Int(), 
                                        false, false, locSide == 0 ? -4 : Service.Grid.Rows + 4);
                break;
        }

        if (targetGridPosition == new Vector2Int(-1, -1))
        {
            return;
        }

        //Find a target position to bring them onto the board
        switch (loc)
        {
            case SpawnLocation.Back:
            case SpawnLocation.Front:
                gridPosition.x = targetGridPosition.x;
                gridPosition.y = loc == SpawnLocation.Back ? -4 : Service.Grid.Rows + 4;
                break;
            case SpawnLocation.Sides:

                gridPosition.x = locSide == 0 ? -4 : Service.Grid.Columns + 4;
                gridPosition.y = targetGridPosition.y;

                break;
        }
        
        var enemy = (GameObject) Instantiate(Service.Prefab.EnemyActor);
        var enemyController = enemy.GetComponent<EnemyController>();

        //Assign spawn location and target position
        enemy.transform.position = Service.Grid.GetTileWorldPosition(gridPosition);
        enemyController.TargetPosition = targetGridPosition;

        Service.Grid.Actors.Add(enemyController);


    }


    #endregion
}
