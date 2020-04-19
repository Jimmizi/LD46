using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class StormController : MonoBehaviour
{
    public bool Transitioning;

    public float TransitionSpeedInSeconds = 1;

    public float CurrentStrength;
    public float TargetStrength;

    private float nextTransistionSpeed;
    private float transitionStep;

    private Material stormMat;

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
        return !Transitioning && Math.Abs(CurrentStrength - 1) < 0.005f;
    }
    public bool IsVignetteStorm()
    {
        return !Transitioning && Math.Abs(CurrentStrength - 0.2f) < 0.005f;
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
        Set(1f, instant);
    }

    public void SetVignette(bool instant = false)
    {
        Set(0.2f, instant);
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
