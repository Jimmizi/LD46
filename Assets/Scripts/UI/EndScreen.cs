using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    public float FadeInSpeed;
    public float FadeOutSpeed;

    public CanvasGroup PlayButtonGroup;
    public CanvasGroup BackgroundGroup;
    public CanvasGroup EndTextGroup;

    private bool launchedFader = false;
    private bool fadedOut = false;

    public void Awake()
    {
        Service.End = this;
    }
    
    public void StartRestart()
    {
        if (!launchedFader)
        {
            launchedFader = true;

            Service.Storm.SetFull();

            //StartCoroutine(fadeInBackground(BackgroundGroup));
            StartCoroutine(fadeOutCanvasGroupEnumerator(Service.Flow.GameUICanvasGroup));
            StartCoroutine(fadeOutCanvasGroupEnumerator(PlayButtonGroup));
            StartCoroutine(fadeOutCanvasGroupEnumerator(EndTextGroup));
        }
    }

    void Update()
    {
        if (!launchedFader)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartRestart();
            }
        }
    }

    public bool ReadyToFadeBackIn => launchedFader && fadedOut;

    public void FadeBackIn()
    {
        if (launchedFader && fadedOut && Service.Storm.IsFullStorm())
        {
            Service.End = null;
            Destroy(this);
            //StartCoroutine(fadeOutCanvasGroupEnumerator(BackgroundGroup, true));
        }
    }

    private IEnumerator fadeInBackground(CanvasGroup group)
    {
        while (group.alpha < 1)
        {
            group.alpha += FadeInSpeed * Time.deltaTime;
            yield return null;
        }

        Debug.Log($"EndScreen:fadeInBackground - Done with fade out for {group.name}.");
        group.alpha = 1;
        
    }

    private IEnumerator fadeOutCanvasGroupEnumerator(CanvasGroup group, bool destroyOnDone = false)
    {
        while (group.alpha > 0)
        {
            group.alpha -= FadeOutSpeed * Time.deltaTime;
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
