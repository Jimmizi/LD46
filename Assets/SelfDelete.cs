using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDelete : MonoBehaviour
{
    public float TimeToDestruct;

    public bool FadeBeforeDestroy;

    private float timer;
    private bool startedFade;

    private CanvasGroup myGroup;

    void Start()
    {
        myGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        timer += Time.deltaTime * GameplayManager.GlobalTimeMod;

        if (timer >= TimeToDestruct)
        {
            if (!FadeBeforeDestroy || myGroup == null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                if (!startedFade)
                {
                    startedFade = true;
                    StartCoroutine(FadeOut());
                }
            }
        }
    }

    private IEnumerator FadeOut()
    {
        while (myGroup.alpha > 0)
        {
            myGroup.alpha -= Time.deltaTime * GameplayManager.GlobalTimeMod;
            yield return null;
        }

        myGroup.alpha = 0;
        Destroy(this.gameObject);
    }
}
