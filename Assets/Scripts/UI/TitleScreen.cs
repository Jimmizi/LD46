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

    public EventHandler OnPlayPressed;
    
    private bool stageLaunchedCoroutine = false;

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
            OnPlayPressed?.Invoke(this, new EventArgs());
            stage++;
        }
    }

    private IEnumerator fadeOutCanvasGroupEnumerator(CanvasGroup group, float fadeSpeed, bool moveStage = true)
    {
        stageLaunchedCoroutine = true;

        while (group.alpha > 0)
        {
            group.alpha -= (1f / fadeSpeed) * Time.deltaTime;
            yield return null;
        }

        group.alpha = 0;

        Debug.Log($"TitleScreen:fadeOutCanvasGroupEnumerator - Done with fade out for {group.name}.");
        if (moveStage)
        {
            stageLaunchedCoroutine = false;
            stage++;
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
                    StartCoroutine(fadeOutCanvasGroupEnumerator(BackgroundGroup, BackgroundFadeSpeed, false));
                    StartCoroutine(fadeOutCanvasGroupEnumerator(TitleTextGroup, TitleFadeSpeed));
                }
                break;
            case Stage.Destroy:
                Destroy(this);
                break;
        }
    }
}
