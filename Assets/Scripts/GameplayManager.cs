using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages creating races throughout the game.
/// </summary>
public class GameplayManager : MonoBehaviour
{
    public EventHandler OnGameFinished;

    public bool DebugEndGame;

    public RaceCoordinator CurrentRace = null;
    private int racesFinishedCount = 0;

    void Awake()
    {
        Service.Game = this;
    }

    void OnDestroy()
    {
        Service.Game = null;
        if (CurrentRace)
        {
            Destroy(CurrentRace.gameObject);
        }
    }

    void Update()
    {
        // When we do not have a race, lets make one
        if (CurrentRace == null)
        {
            MakeNewRace();
        }

        if (DebugEndGame)
        {
            DebugEndGame = false;
            OnGameFinished?.Invoke(this, new EventArgs());
        }
    }

    void MakeNewRace()
    {
        CurrentRace = new RaceCoordinator();

    }
}
