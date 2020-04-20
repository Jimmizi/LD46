using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string MenuMusicEvent;

    [FMODUnity.EventRef]
    public string GameMusicEvent;

    private FMOD.Studio.EventInstance menuMusicInstance;
    private FMOD.Studio.EventInstance gameLoopInstance;


    public void PlayMenuMusic()
    {
        menuMusicInstance = FMODUnity.RuntimeManager.CreateInstance(MenuMusicEvent);
        menuMusicInstance.start();
        menuMusicInstance.setTimelinePosition(22000);
    }

    public void StopMenuMusic()
    {
        menuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void PlayGameLoopMusic()
    {
        gameLoopInstance = FMODUnity.RuntimeManager.CreateInstance(GameMusicEvent);
        gameLoopInstance.start();
        
    }

    public void StopGameLoopMusic()
    {
        gameLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    void OnApplicationQuit()
    {
        if (menuMusicInstance.isValid())
        {
            menuMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            menuMusicInstance.release();
            menuMusicInstance.clearHandle();
        }

        if (gameLoopInstance.isValid())
        {
            gameLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            gameLoopInstance.release();
            gameLoopInstance.clearHandle();
        }
    }

    void Awake()
    {
        Service.Music = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Service.Music = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
