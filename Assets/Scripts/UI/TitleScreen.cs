using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public CanvasGroup CanvasFade;

    public EventHandler OnTitleFadedOut;

    public GameObject Title;
    public CanvasGroup PressToPlayCanvasGroup;
    
    private bool stageLaunchedCoroutine = false;
    private bool launchedGame;

    private Vector2 OriginalTitlePosition;

    private float delayTimer = 2f;

    private const float titleMoveUp = 1500f;

    private bool AllowPlay;

    private enum Stage
    {
        WaitBeforeIntro = 0,
        Intro,
        Idle,

        Fading,
       // FadingTitle, happens with fading background

        Destroy
    }

    private Stage stage;

    public void Play()
    {
        if (AllowPlay && stage == Stage.Idle)
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

        launchedGame = true;
        OnTitleFadedOut?.Invoke(this, new EventArgs());
    }

    void Start()
    {
        OriginalTitlePosition = Title.transform.position;
        Title.transform.position += new Vector3(0, titleMoveUp);
        PressToPlayCanvasGroup.alpha = 0;

        

        StartCoroutine(WaitAtStart());
    }

    // Update is called once per frame
    void Update()
    {
        switch (stage)
        {
            case Stage.Intro:

                delayTimer -= Time.deltaTime;

                if (delayTimer <= 0)
                {
                    StartCoroutine(BringInTitle());
                    stage++;
                }

                break;
            case Stage.Idle:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Play();
                }
                break;
            case Stage.Fading:
                if (!stageLaunchedCoroutine)
                {
                    Service.Music.StopMenuMusic();
                    StartCoroutine(fadeOutCanvasGroupEnumerator(CanvasFade, 2));
                }
                break;
            //case Stage.FadingBackground:
            //    if (!stageLaunchedCoroutine)
            //    {
            //        //StartCoroutine(fadeOutCanvasGroupEnumerator(BackgroundGroup, BackgroundFadeSpeed, false, true));
            //        //StartCoroutine(fadeOutCanvasGroupEnumerator(TitleTextGroup, TitleFadeSpeed));
            //    }
            //    break;
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

    IEnumerator WaitAtStart()
    {
        yield return new WaitForSeconds(1f);

        Service.Music.PlayMenuMusic();
        stage = Stage.Intro;
    }

    IEnumerator BringInTitle()
    {
        while (Title.transform.position.y > OriginalTitlePosition.y)
        {
            Title.transform.position -= new Vector3(0, titleMoveUp * 2 * Time.deltaTime,0);
            yield return null;
        }

        StartCoroutine(BringInPressToPlay());
    }

    IEnumerator BringInPressToPlay()
    {
        while (PressToPlayCanvasGroup.alpha < 1)
        {
            PressToPlayCanvasGroup.alpha += Time.deltaTime * 4;
            yield return null;
        }

        AllowPlay = true;
    }
}
