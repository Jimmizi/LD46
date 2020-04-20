using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetExplosionSound : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string ExplosionEvent;

    private FMOD.Studio.EventInstance explosionEventInstance;


    void Start()
    {
        explosionEventInstance = FMODUnity.RuntimeManager.CreateInstance(ExplosionEvent);
        explosionEventInstance.start();
    }

    void OnApplicationQuit()
    {
        if (explosionEventInstance.isValid())
        {
            explosionEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            explosionEventInstance.release();
            explosionEventInstance.clearHandle();
        }
    }
}
