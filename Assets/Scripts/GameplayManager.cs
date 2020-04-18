using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Manages creating races throughout the game.
/// </summary>
public class GameplayManager : MonoBehaviour
{
    public EventHandler OnGameFinished;

#if UNITY_EDITOR
    public bool DebugEndGame;
#endif

    public RaceCoordinator CurrentRace = null;
    public int RaceCount { get; private set; } = 0;

    private bool raceInProgress = true;

    void Awake()
    {
        Service.Game = this;
    }

    void OnDestroy()
    {
        Service.Game = null;
        CurrentRace = null;
    }

    void Update()
    {
        if (!raceInProgress)
        {
            return;
        }

        // When we do not have a race, lets make one
        if (CurrentRace == null)
        {
            MakeNewRace();
        }
        else
        {
            CurrentRace.Update();
        }

#if UNITY_EDITOR
        if (DebugEndGame)
        {
            DebugEndGame = false;
            OnGameFinished?.Invoke(this, new EventArgs());
            raceInProgress = false;
        }
#endif
    }

    /// <summary> 
    /// Create a player off the bottom of the screen ready to move into the frame
    /// </summary>
    /// <returns>the new player</returns>
    public GameObject CreatePlayerObjectForRaceStart()
    {
        var player = (GameObject)Instantiate(Service.Prefab.PlayerActor);
        player.transform.position = Service.Grid.GetPlayerSpawnPosition();

        return player;
    }
    public void DestroyPlayerObject(GameObject player)
    {
        Destroy(player);
    }

    void MakeNewRace()
    {
        CurrentRace = new RaceCoordinator();
        
        //If we don't win on finish, end the game. otherwise we'll come back around and create a new race
        CurrentRace.OnRaceFinished += (sender, args) =>
        {
            if (args is RaceFinishEventArgs data)
            {
                if (!data.Win)
                {
                    OnGameFinished?.Invoke(this, data);
                    raceInProgress = false;
                }
            }

            CurrentRace = null;
        };

        RaceCount++;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR

        float posY = 8.8f;

        Handles.Label(new Vector3(-1.5f, posY, 0), $"Race Count: {RaceCount}");
        posY -= 0.25f;

        if (CurrentRace != null)
        {
            Handles.Label(new Vector3(-1.5f, posY, 0), $"Time: {CurrentRace.RaceTime} / {CurrentRace.RaceLengthTimer}");
        }
#endif
    }
}
