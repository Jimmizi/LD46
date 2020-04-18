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
    

    private bool stageLaunchedCoroutine = false;

    private float titlePlayTimer;

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

    private IEnumerator fadeOutCanvasGroupEnumerator(CanvasGroup group, float fadeSpeed, bool moveStage = true)
    {
        stageLaunchedCoroutine = true;

        while (group.alpha > 0)
        {
            group.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        Debug.Log($"TitleScreen:PlayCoroutine - Done with fade out for {group.name}.");
        if (moveStage)
        {
            stageLaunchedCoroutine = false;
            stage++;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (stage)
        {
            case Stage.Idle:
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
            //case Stage.FadingTitle:
            //    if (!stageLaunchedCoroutine)
            //    {
            //        StartCoroutine(fadeOutCanvasGroupEnumerator(TitleTextGroup, TitleFadeSpeed));
            //    }
            //    break;
            case Stage.Destroy:
                Destroy(this);
                break;
        }
    }


}
