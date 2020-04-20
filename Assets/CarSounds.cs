using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSounds : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string SkidEvent;

    [FMODUnity.EventRef]
    public string EngineEvent;

    [FMODUnity.EventRef]
    public string CrashEvent;

    private FMOD.Studio.EventInstance engineInstance;
    private FMOD.Studio.EventInstance skidInstance;
    private FMOD.Studio.EventInstance crashInstance;

    public void StartEngine()
    {
        engineInstance = FMODUnity.RuntimeManager.CreateInstance(EngineEvent);
        engineInstance.start();
    }

    public void StopEngine()
    {
        engineInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void PlaySkid()
    {
        skidInstance = FMODUnity.RuntimeManager.CreateInstance(SkidEvent);
        skidInstance.start();
    }

    public void PlayCrash()
    {
        crashInstance = FMODUnity.RuntimeManager.CreateInstance(CrashEvent);
        crashInstance.start();
    }

    void OnApplicationQuit()
    {
        if (skidInstance.isValid())
        {
            skidInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            skidInstance.release();
            skidInstance.clearHandle();
        }

        if (crashInstance.isValid())
        {
            crashInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            crashInstance.release();
            crashInstance.clearHandle();
        }

        if (engineInstance.isValid())
        {
            engineInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            engineInstance.release();
            engineInstance.clearHandle();
        }
    }
}
