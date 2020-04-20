using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class StormController : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string WindEvent;

    private FMOD.Studio.EventInstance windInst;

    private const float VignetteVaule = 0.75f;
    private const float FullValue = 1.0f;

    public bool Transitioning;

    public float TransitionSpeedInSeconds = 1;

    public float CurrentStrength;
    public float TargetStrength;

    private float nextTransistionSpeed;
    private float transitionStep;

    private Material stormMat;

    public void PlayWind()
    {
        windInst = FMODUnity.RuntimeManager.CreateInstance(WindEvent);
        windInst.start();
    }

    void Awake()
    {
        Service.Storm = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Service.Storm = this;
        nextTransistionSpeed = TransitionSpeedInSeconds;

        stormMat = GetComponent<MeshRenderer>().material;
        Assert.IsNotNull(stormMat);
    }

    public bool IsFullStorm()
    {
        return !Transitioning && Math.Abs(CurrentStrength - FullValue) < 0.005f;
    }
    public bool IsVignetteStorm()
    {
        return !Transitioning && Math.Abs(CurrentStrength - VignetteVaule) < 0.005f;
    }

    /// <summary>
    /// Set how long it takes to transition IN SECONDS
    /// </summary>
    /// <param name="val"></param>
    public void SetNextSpeed(float val)
    {
        nextTransistionSpeed = val;
    }

    public void SetFull(bool instant = false)
    {
        if (!instant)
        {
            PlayWind();
        }

        Set(FullValue, instant);
    }

    public void SetVignette(bool instant = false)
    {
        Set(VignetteVaule, instant);
    }

    public void Set(float val, bool instant = false)
    {
        TargetStrength = val;
        Transitioning = !instant;

        transitionStep = (TargetStrength - CurrentStrength) * (1f / nextTransistionSpeed);
        nextTransistionSpeed = TransitionSpeedInSeconds;

        if (instant)
        {
            CurrentStrength = TargetStrength;
            stormMat.SetFloat("_Turnitup", CurrentStrength);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Transitioning)
        {
            CurrentStrength += transitionStep * Time.deltaTime;

            if (Math.Abs(CurrentStrength - TargetStrength) < 0.005f)
            {
                CurrentStrength = TargetStrength;
                Transitioning = false;
            }

            stormMat.SetFloat("_Turnitup", CurrentStrength);
        }
    }
}
