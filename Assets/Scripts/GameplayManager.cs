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
    public static float GlobalTimeMod = 1.0f;
    
    public CanvasGroup Fader;
    public float FadeSpeed = 1f;

    public EventHandler OnGameFinished;

#if UNITY_EDITOR
    public bool DebugEndGame;
#endif

    [HideInInspector]
    public RaceCoordinator CurrentRace = null;

    [HideInInspector]
    public int RaceCount { get; private set; } = 0;

    void Awake()
    {
        Debug.Log("Gameplay awake.");
        Service.Game = this;
    }
    void Start()
    {
        Debug.Log("Gameplay Start.");
        Service.Game = this;
    }

    public void Shutdown()
    {
        Debug.Log("Destroying gameplay.");

        if (CurrentRace)
        {
            CurrentRace.Shutdown();
            Destroy(CurrentRace.gameObject);
            CurrentRace = null;
        }
    }

    void Update()
    {
        // When we do not have a race, lets make one
        if (CurrentRace == null)
        {
            MakeNewRace();
        }

#if UNITY_EDITOR
        if (DebugEndGame)
        {
            DebugEndGame = false;
            EndGame();
        }
#endif
    }

    public bool IsRaceInProgress()
    {
        if (CurrentRace == null)
        {
            return false;
        }

        return CurrentRace.RaceInProgress;
    }

    public void EndGame()
    {
        CurrentRace.RaceInProgress = false;
        OnGameFinished?.Invoke(this, new EventArgs());
    }

    void MakeNewRace()
    {
        Debug.Log("Making new race.");

        var raceGo = (GameObject) Instantiate(Service.Prefab.RaceCoordinatorPrefab);
        CurrentRace = raceGo.GetComponent<RaceCoordinator>();

        //If we don't win on finish, end the game. otherwise we'll come back around and create a new race
        CurrentRace.OnRaceFinished += (sender, args) =>
        {
            if (args is RaceFinishEventArgs data)
            {
                if (!data.Win)
                {
                    OnGameFinished?.Invoke(this, data);
                    CurrentRace.RaceInProgress = false;
                }
            }

            Destroy(CurrentRace.gameObject);
        };

        RaceCount++;
    }

    public EventHandler OnFadeCoroutineComplete;

    public void FadeInIfBackedOut()
    {
        //if (Fader.alpha >= 1f)
        //{
        //    StartFader(false);
        //}

        if (Service.Storm.IsFullStorm())
        {
            Service.Storm.SetVignette();
        }
    }

    public void StartFader(bool fadeOut)
    {
        IEnumerator fadeCanvasGroupEnumerator(CanvasGroup group)
        {
            //while (fadeOut ? group.alpha < 1 : group.alpha > 0)
            //{
            //    group.alpha += (fadeOut ? FadeSpeed : -FadeSpeed) * Time.deltaTime;
            //    yield return null;
            //}

            //group.alpha = fadeOut ? 1 : 0;

            if (fadeOut)
            {
                Service.Storm.SetFull();
            }
            else
            {
                Service.Storm.SetVignette();
            }

            while (fadeOut && !Service.Storm.IsFullStorm()
                   || !fadeOut && !Service.Storm.IsVignetteStorm())
            {
                yield return null;
            }

            OnFadeCoroutineComplete?.Invoke(this, new EventArgs());
        }

        StartCoroutine(fadeCanvasGroupEnumerator(Fader));
    }

    

    void OnDrawGizmos()
    {
#if UNITY_EDITOR

        float posY = 8.8f;

        //Handles.Label(new Vector3(-1.5f, posY, 0), $"Race Count: {RaceCount}");
        posY -= 0.25f;

        if (CurrentRace != null)
        {
            //Handles.Label(new Vector3(-1.5f, posY, 0), $"Time: {CurrentRace.RaceTime} / {CurrentRace.RaceLengthTimer}");
        }
#endif
    }
}
