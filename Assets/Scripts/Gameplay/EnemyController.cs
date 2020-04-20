using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : GridActor
{
    public enum EnemyPersonality
    {
        Random = -1,
        Aggressive,     //Will try to take the player out
        Persistent,     //Will try to stick around the field, occasionally firing towards the player
        PassiveRacer    //Will just try to get ahead and off screen
    }

    private EnemyPersonality personality;


    public EnemyPersonality Personality
    {
        get => personality;
        set
        {
            personality = value;
            if (personality == EnemyPersonality.Random)
            {
                if (Application.isPlaying)
                {
                    personality = (EnemyPersonality)Random.Range(0, 3);
                }
            }
        }
    }

    private GridActor playerActorRef = null;

    private GridActor combatTarget;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        Personality = Personality;

        var health = GetComponent<HealthComponent>();
        if (health)
        {
            health.OnHealthChanged += (component, f, previousHealth) =>
            {
                SetJustHitObstacle();
            };
        }
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }
}
