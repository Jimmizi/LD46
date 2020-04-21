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
    [FMODUnity.EventRef]
    public string SpacePressedEvent;
    private FMOD.Studio.EventInstance spacePressInst;

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
    public bool DebugForceCheckpoint = false;
    public bool DebugStopObstacleSpawning;
    public bool DebugStopEnemySpawning;
    public bool DebugSpawnEnemy = false;
    public bool DebugSpawnObstacle;

    private float ObstacleSpawnTimer = 0;
    private float TimeUntilNextObstacle = 5;

    private float EnemySpawnTimer;
    private float TimeUntilNextEnemy = 5;
   

    public List<GameObject> ObstacleList = new List<GameObject>();

    public SpawnLocation NextEnemySpawnLocation;

    public EventHandler OnRaceFinished;

    [HideInInspector]
    public GameObject PlayerGameObject = null;

    [HideInInspector]
    public GameObject TerrainGameObject = null;

    [HideInInspector]
    public GameObject PlayerCheckpointGameObject = null;
    [HideInInspector]
    public GameObject CheckpointScoreUi = null;

    /// <summary>
    /// Length of this part of the race, in seconds
    /// </summary>
    public float RaceLengthTimer = 10;

    /// <summary>
    /// Current time of the race
    /// </summary>
    [HideInInspector]
    public float RaceTime;

    [HideInInspector]
    public bool RaceInProgress = true;

    private RaceState stage = RaceState.IntroToRace;

    void Start()
    {
        RaceInProgress = true;
        RaceLengthTimer = Service.Flow.BaseRaceTimer +
                          Service.Flow.TimeIncreasePerCheckpoint.Generate(Service.Game.RaceCount - 1);
        RaceLengthTimer = Mathf.Clamp(RaceLengthTimer, 0, Service.Flow.MaxRaceTime);
    }

    public void Shutdown()
    {
        if (TerrainGameObject)
        {
            Destroy(TerrainGameObject);
        }

        if (PlayerGameObject)
        {
            Destroy(PlayerGameObject);
        }

        foreach (var ob in ObstacleList)
        {
            if (ob)
            {
                Destroy(ob);
            }
        }

        ObstacleList.Clear();

        foreach (var e in Service.Grid.Actors)
        {
            if (e)
            {
                Destroy(e.gameObject);
            }
        }

        Service.Grid.Actors.Clear();
        Service.Grid.PlayerActor = null;
    }

    public void Update()
    {
        if (!RaceInProgress)
        {
            return;
        }

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

                Shutdown();
                
                OnRaceFinished?.Invoke(this, new RaceFinishEventArgs(true));
                stage++;

                break;
        }
    }

    void GenerateTimerForNextEnemy(bool firstRound = false)
    {
        TimeUntilNextEnemy = Service.Flow.TimeBetweenEnemySpawns.Generate(Service.Game.RaceCount - 1);
        TimeUntilNextEnemy += Service.Flow.EnemySpawnTimeChangePerCheckpoint.Generate(Service.Game.RaceCount - 1);

        if (firstRound)
        {
            TimeUntilNextEnemy += Service.Flow.FirstRoundEnemySpawnDelay;
        }
    }

    void GenerateTimerForNextObstacle(bool firstRound = false)
    {
        TimeUntilNextObstacle = Service.Flow.TimeBetweenObstacleSpawns.Generate(Service.Game.RaceCount - 1);
        TimeUntilNextObstacle += Service.Flow.ObstacleSpawnTimeChangePerCheckpoint.Generate(Service.Game.RaceCount - 1);

        if (firstRound)
        {
            TimeUntilNextObstacle += Service.Flow.FirstRoundObstacleSpawnDelay;
        }
    }

    private IEnumerator fadeInGameUI()
    {
        while (Service.Flow.GameUICanvasGroup.alpha < 1)
        {
            Service.Flow.GameUICanvasGroup.alpha += 2 * Time.deltaTime * GameplayManager.GlobalTimeMod;
            yield return null;
        }

        Service.Flow.GameUICanvasGroup.alpha = 1;
    }

    private IEnumerator fadeOutGameUI()
    {
        while (Service.Flow.GameUICanvasGroup.alpha > 0)
        {
            Service.Flow.GameUICanvasGroup.alpha -= 2 * Time.deltaTime * GameplayManager.GlobalTimeMod;
            yield return null;
        }

        Service.Flow.GameUICanvasGroup.alpha = 0;
    }

    void UpdateRaceIntro()
    {
        //Service.Flow.GameUICanvasGroup.alpha = 1;
        StartCoroutine(fadeInGameUI());

        GenerateTimerForNextEnemy(true);
        GenerateTimerForNextObstacle(true);

        if (TerrainGameObject == null)
        {
            TerrainGameObject = (GameObject) Instantiate(Service.Prefab.TerrainCollectionPrefab);
        }

        if (PlayerGameObject == null)
        {
            Service.Game.FadeInIfBackedOut();

            PlayerGameObject = (GameObject)Instantiate(Service.Prefab.PlayerActor);
            PlayerGameObject.transform.position = Service.Grid.GetPlayerSpawnPosition();

            var healthComp = PlayerGameObject.GetComponent<HealthComponent>();

            Service.Grid.PlayerActor = PlayerGameObject.GetComponent<GridActor>();

            Service.Grid.PlayerActor.TargetPosition = new Vector2Int(Service.Grid.Columns / 2, (Service.Grid.Rows / 2) - 1);

            healthComp.OnHealthDepleted += (component, health, previousHealth) =>
            {
                Service.Game.EndGame();
            };

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
        ObstacleSpawnTimer += Time.deltaTime * GameplayManager.GlobalTimeMod;
        EnemySpawnTimer += Time.deltaTime * GameplayManager.GlobalTimeMod;

#if UNITY_EDITOR
        if (DebugStopObstacleSpawning)
        {
            ObstacleSpawnTimer = 0;
        }
        if (DebugIgnoreRaceTimer)
        {
            RaceTime = 0;
        }

        if (DebugStopEnemySpawning)
        {
            EnemySpawnTimer = 0;
        }

        if (DebugForceCheckpoint)
        {
            RaceTime = RaceLengthTimer;
        }
#endif

        if (ObstacleSpawnTimer >= TimeUntilNextObstacle)
        {
            ObstacleSpawnTimer = 0;
            SpawnNewObstacle();
        }

        if (EnemySpawnTimer >= TimeUntilNextEnemy)
        {
            EnemySpawnTimer = 0;
            SpawnNewEnemy(NextEnemySpawnLocation);
        }

        if (RaceTime >= RaceLengthTimer)
        {
            stage = RaceState.OutroToCheckpoint;

            // Destroy collider when about to zoom up into the screen otherwise the player can die
            var col = PlayerGameObject.GetComponent<BoxCollider2D>();
            if (col)
            {
                Destroy(col);
            }
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

            StartCoroutine(fadeOutGameUI());

            // Kill everything
            Shutdown();

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
        Service.Grid.PlayerActor.TargetPosition = new Vector2Int(Service.Grid.PlayerActor.TargetPosition.x, Service.Grid.Rows + 4);
        Service.Grid.PlayerActor.LockTargetPosition = true;
        Service.Grid.PlayerActor.MoveSpeed /= 2;

        stage = RaceState.OutroToCheckpoint_Wait;

        Service.Score.AddScore(ScoreController.ScoreType.ReachedCheckpoint);

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
            spacePressInst = FMODUnity.RuntimeManager.CreateInstance(SpacePressedEvent);
            spacePressInst.start();

            stage = RaceState.CheckpointFadeOut;

            Service.Game.OnFadeCoroutineComplete += OnFadedOutFromCheckpoint;
            Service.Game.StartFader(true);
        }
    }


    public void FlushNullObstacles()
    {
        for (int i = ObstacleList.Count - 1; i >= 0; i--)
        {
            if (ObstacleList[i] == null)
            {
                ObstacleList.RemoveAt(i);
            }
        }
    }

    #region Spawning functions


    void SpawnNewObstacle()
    {
        GenerateTimerForNextObstacle();

        FlushNullObstacles();

        if (ObstacleList.Count >= Service.Flow.MaxObstacles)
        {
            return;
        }

        var gridPosition = new Vector2Int();

        List<int> availableXPositions = new List<int>();

        for (int x = 0; x < Service.Grid.Columns; x++)
        {
            if (Service.Grid.AreSpacesInDirectionFreeFromTerrain(new Vector2Int(x, Service.Grid.Rows - 1),
                new Vector2Int(0, -1), Service.Grid.Rows))
            {
                availableXPositions.Add(x);
            }
        }

        if (availableXPositions.Count > 0)
        {
            availableXPositions.Shuffle();

            gridPosition.x = availableXPositions[0];
        }
        else
        {
            gridPosition.x = Service.Grid.GetRandomHorizontalTile();
        }

        gridPosition.y = Service.Grid.Rows + Random.Range(2, 10);

        var obstacle = (GameObject)Instantiate(Service.Prefab.ObstacleActor);
        obstacle.transform.position = Service.Grid.GetTileWorldPosition(gridPosition);

        ObstacleList.Add(obstacle);

        var grid = obstacle.GetComponent<GridActor>();

        Assert.IsNotNull(grid);

        grid.TargetPosition = new Vector2Int(gridPosition.x, -5);
        grid.UseLinearMovementSpeed = true;
        grid.LockTargetPosition = true;
        grid.SelfDestroyOnTargetReached = true;
    }

    void SpawnNewEnemy(SpawnLocation loc)
    {
        GenerateTimerForNextEnemy();

        if (Service.Grid.Actors.Count >= Service.Flow.MaxEnemies)
        {
            return;
        }

        var gridPosition = new Vector2Int();
        var targetGridPosition = new Vector2Int();

        int locSide = 0;

        //Get a spawn location first
        switch (loc)
        {
            case SpawnLocation.Back:
                targetGridPosition = Service.Grid.GetLeastCrowdedTileInRange(new Vector2Int(0, 0), new Vector2Int(Service.Grid.Columns - 1, 1),
                                            new Vector2Int(), new Vector2Int(0, 1), true);
                break;
            case SpawnLocation.Front:
                targetGridPosition = Service.Grid.GetLeastCrowdedTileInRange(new Vector2Int(0, Service.Grid.Rows - 2), new Vector2Int(Service.Grid.Columns - 1, Service.Grid.Rows-1),
                                        new Vector2Int(), new Vector2Int(0, -1), false, true);
                break;
            case SpawnLocation.Sides:

                locSide = Random.Range(0, 2);
                targetGridPosition = Service.Grid.GetLeastCrowdedTileInRange(new Vector2Int(locSide == 0 ? 0 : Service.Grid.Columns - 2, 0),
                                        new Vector2Int(Service.Grid.Columns - 1, Service.Grid.Rows - 1), new Vector2Int(), new Vector2Int(locSide == 0 ? 1 : -1, 0),
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
