using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public EventHandler OnGameFinished;

    public bool DebugEndGame;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (DebugEndGame)
        {
            DebugEndGame = false;
            OnGameFinished?.Invoke(this, new EventArgs());
        }
    }
}
