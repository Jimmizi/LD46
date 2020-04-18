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
        Checkpoint,
        End,

        Invalid
    }

    public EventHandler OnRaceFinished;

    [HideInInspector]
    public GameObject PlayerGameObject = null;
    
    [HideInInspector]
    public GridActor PlayerGridActor = null;

    /// <summary>
    /// Length of this part of the race, in seconds
    /// </summary>
    [HideInInspector]
    public float RaceLengthTimer = 5;

    [HideInInspector]
    public float RaceTime;

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
                stage++;
                break;

            //Finish the race and let the gameplay manager relaunch
            case RaceState.End:

                Service.Game.DestroyPlayerObject(PlayerGameObject);

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
            PlayerGameObject = Service.Game.CreatePlayerObjectForRaceStart();
            PlayerGridActor = PlayerGameObject.GetComponent<GridActor>();

            PlayerGridActor.TargetPosition = new Vector2Int(Service.Grid.Columns / 2, Service.Grid.Rows / 2);

            Assert.IsNotNull(PlayerGridActor);

            stage++;
        }
    }

    void UpdateRaceOutro()
    {
        PlayerGridActor.TargetPosition = new Vector2Int(Service.Grid.Columns / 2, Service.Grid.Rows + 4);
        PlayerGridActor.LockTargetPosition = true;
        stage++;
    }

    void UpdateRace()
    {
        RaceTime += Time.deltaTime;

        if (RaceTime >= RaceLengthTimer)
        {
            stage++;
        }
    }
}
