using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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

    public EventHandler OnRaceFinished;

    [HideInInspector]
    public GameObject PlayerGameObject = null;
    
    [HideInInspector]
    public GridActor PlayerGridActor = null;

    [HideInInspector]
    public GameObject PlayerCheckpointGameObject = null;
    [HideInInspector]
    public GameObject CheckpointScoreUi = null;

    /// <summary>
    /// Length of this part of the race, in seconds
    /// </summary>
    [HideInInspector]
    public float RaceLengthTimer = 5;

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
                PlayerGridActor = null;

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

            PlayerGridActor = PlayerGameObject.GetComponent<GridActor>();

            PlayerGridActor.TargetPosition = new Vector2Int(Service.Grid.Columns / 2, Service.Grid.Rows / 2);

            Assert.IsNotNull(PlayerGridActor);

            //Move onto the racing stage
            stage++;
        }
    }

    void UpdateRace()
    {
        //While racing, add onto the race time and wait for it to expire
        RaceTime += Time.deltaTime;

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
        PlayerGridActor.TargetPosition = new Vector2Int(Service.Grid.Columns / 2, Service.Grid.Rows + 4);
        PlayerGridActor.LockTargetPosition = true;

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

}
