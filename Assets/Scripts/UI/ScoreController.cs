using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class ScoreController : MonoBehaviour
{
    public enum ScoreType
    {
        PerHealthTick,
        ReachedCheckpoint,
        KilledEnemy
    }

    public bool DelayScoreSetting;

    public List<DialController> DialControllers;
    [Range(0, 32000)]
    public int DebugSetScore = 0;
    
    public static int currentScore = 0;


    void Awake()
    {
        Service.Score = this;
    }
    void Start()
    {
        Service.Score = this;
        Assert.IsNotNull(DialControllers);

        if (!DelayScoreSetting)
        {
            SetScore(currentScore);
        }
        else
        {
            StartCoroutine(DelayInitialScoreSet());
        }
    }

    IEnumerator DelayInitialScoreSet()
    {
        yield return new WaitForSeconds(1f);

        SetScore(currentScore);
    }

    void Update() {
        if (DebugSetScore != currentScore) {
            //SetScore(DebugSetScore);
        }
    }
    
    public static IEnumerable<int> GetDigits(int source) {
        int individualFactor = 0;
        int tennerFactor = Convert.ToInt32(Math.Pow(10, source.ToString().Length));
        do {
            source -= tennerFactor * individualFactor;
            tennerFactor /= 10;
            individualFactor = source / tennerFactor;

            yield return individualFactor;
        } while (tennerFactor > 1);
    }

    public void AddScore(ScoreType scoretype)
    {
        int extrascore = 0;
        switch (scoretype)
        {
            case ScoreType.PerHealthTick:
                extrascore = 10;
                break;
            case ScoreType.ReachedCheckpoint:
                extrascore = 250;
                break;
            case ScoreType.KilledEnemy:
                extrascore = 500;
                break;
        }

        SetScore(currentScore + extrascore);
    }

    public void SetScore(int score) 
    {
        currentScore = Math.Abs(score);
        int digitIndex = 0;
        int[] digits = GetDigits(currentScore).ToArray();
        
        for (int i = 0; i < DialControllers.Count; i++) 
        {
            if (i < digits.Length) {
                DialControllers[digitIndex].SetTo(digits[digits.Length - i - 1]);
            } else {
                DialControllers[i].SetToEmpty();
            }

            digitIndex++;
        }
        
    }
}
