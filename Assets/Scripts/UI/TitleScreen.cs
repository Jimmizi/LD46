using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public float PlayButtonFadeSpeed;
    public float BackgroundFadeSpeed;
    public float TitleFadeSpeed;

    public CanvasGroup PlayButtonGroup;
    public CanvasGroup BackgroundGroup;
    public CanvasGroup TitleTextGroup;

    public EventHandler OnTitleFadedOut;
    
    private bool stageLaunchedCoroutine = false;
    private bool launchedGame;

    private enum Stage
    {
        Idle = 0,

        FadingPlayButton,
        FadingBackground,
       // FadingTitle, happens with fading background

        Destroy
    }

    private Stage stage;

    public void Play()
    {
        if (stage == Stage.Idle)
        {
            stage++;
        }
    }

    private IEnumerator fadeOutCanvasGroupEnumerator(CanvasGroup group, float fadeSpeed, bool moveStage = true, bool startGame = false)
    {
        stageLaunchedCoroutine = true;

        while (group.alpha > 0)
        {
            group.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        group.alpha = 0;

        Debug.Log($"TitleScreen:fadeOutCanvasGroupEnumerator - Done with fade out for {group.name}.");
        if (moveStage)
        {
            stageLaunchedCoroutine = false;
            stage++;
        }

        if (startGame)
        {
            if (!launchedGame)
            {
                launchedGame = true;
                OnTitleFadedOut?.Invoke(this, new EventArgs());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (stage)
        {
            case Stage.Idle:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Play();
                }
                break;
            case Stage.FadingPlayButton:
                if (!stageLaunchedCoroutine)
                {
                    StartCoroutine(fadeOutCanvasGroupEnumerator(PlayButtonGroup, PlayButtonFadeSpeed));
                }
                break;
            case Stage.FadingBackground:
                if (!stageLaunchedCoroutine)
                {
                    StartCoroutine(fadeOutCanvasGroupEnumerator(BackgroundGroup, BackgroundFadeSpeed, false, true));
                    StartCoroutine(fadeOutCanvasGroupEnumerator(TitleTextGroup, TitleFadeSpeed));
                }
                break;
            case Stage.Destroy:

                if (!launchedGame)
                {
                    launchedGame = true;
                    OnTitleFadedOut?.Invoke(this, new EventArgs());
                }

                Destroy(this);
                break;
        }
    }
}
