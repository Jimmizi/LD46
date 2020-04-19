using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : GridActor
{
    public enum EnemyPersonality
    {
        Aggressive,     //Will try to take the player out
        Persistent,     //Will try to stick around the field, occasionally firing towards the player
        PassiveRacer    //Will just try to get ahead and off screen
    }

    private GridActor playerActorRef = null;

    private GridActor combatTarget;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }
}
