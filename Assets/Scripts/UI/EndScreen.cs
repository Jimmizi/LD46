using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string SpacePressedEvent;
    private FMOD.Studio.EventInstance spacePressInst;

    public float FadeInSpeed;
    public float FadeOutSpeed;

    public CanvasGroup MainGroup;

    public CanvasGroup PlayButtonGroup;
    public CanvasGroup BackgroundGroup;
    public CanvasGroup EndTextGroup;

    private bool launchedFader = false;
    private bool fadedOut = false;

    public void Awake()
    {
        Service.End = this;
    }

    void Start()
    {
        MainGroup.alpha = 0;
        StartCoroutine(FadeScreenIn());
    }

    IEnumerator FadeScreenIn()
    {
        while (MainGroup.alpha < 1)
        {
            MainGroup.alpha += 2.5f * Time.deltaTime;
            yield return null;
        }

        MainGroup.alpha = 1;
    }

    IEnumerator FadeScreenOut()
    {
        while (MainGroup.alpha > 0)
        {
            MainGroup.alpha -= 1f * Time.deltaTime;
            yield return null;
        }

        MainGroup.alpha = 0;

        fadedOut = true;
        //Service.End = null;
        //Destroy(this);
    }

    public void StartRestart()
    {
        if (!launchedFader)
        {
            spacePressInst = FMODUnity.RuntimeManager.CreateInstance(SpacePressedEvent);
            spacePressInst.start();

            launchedFader = true;

            Service.Storm.SetFull();
            StartCoroutine(FadeScreenOut());
            ////StartCoroutine(fadeInBackground(BackgroundGroup));
            //StartCoroutine(fadeOutCanvasGroupEnumerator(Service.Flow.GameUICanvasGroup));
            //StartCoroutine(fadeOutCanvasGroupEnumerator(PlayButtonGroup));
            //StartCoroutine(fadeOutCanvasGroupEnumerator(EndTextGroup));
        }
    }

    void Update()
    {
        if (MainGroup.alpha == 1 && !launchedFader)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartRestart();
            }
        }
    }

    public bool ReadyToFadeBackIn => launchedFader && fadedOut;

    public void DestroyThis()
    {
        Service.End = null;
        Destroy(this.gameObject);
    }

    private IEnumerator fadeInBackground(CanvasGroup group)
    {
        while (group.alpha < 1)
        {
            group.alpha += FadeInSpeed * Time.deltaTime * GameplayManager.GlobalTimeMod;
            yield return null;
        }

        Debug.Log($"EndScreen:fadeInBackground - Done with fade out for {group.name}.");
        group.alpha = 1;
        
    }

    private IEnumerator fadeOutCanvasGroupEnumerator(CanvasGroup group, bool destroyOnDone = false)
    {
        while (group.alpha > 0)
        {
            group.alpha -= FadeOutSpeed * Time.deltaTime * GameplayManager.GlobalTimeMod;
            yield return null;
        }

        Debug.Log($"EndScreen:fadeOutCanvasGroupEnumerator - Done with fade out for {group.name}.");
        group.alpha = 0;

        if (destroyOnDone)
        {
            Service.End = null;
            Destroy(this);
        }

        fadedOut = true;
    }
}
