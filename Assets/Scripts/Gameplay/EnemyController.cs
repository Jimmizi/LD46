using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class EnemyController : GridActor
{
    private const int NUM_LANES = 3;
    private const int NUM_IMMEDIATE_LANES = 3;

    public enum EnemyPersonality
    {
        Random = -1,
        Aggressive, //Will try to take the player out
        Persistent, //Will try to stick around the field, occasionally firing towards the player
        PassiveRacer //Will just try to get ahead and off screen
    }

    private enum ActionState
    {
        PickWaitingTime, //Generate time to wait for
        Waiting, //Waiting for the timer to tick over
        StoreWorldState, //Store current information about the world
        PickAction, //Try and pick the best action to do
        SetupAction, //Any setup required for this action
        DoAction, //Perform the action
        PostAction //Any cleanup needed for this action before going back to Waiting
    }

    public struct DecisionMakingData
    {
        public enum MainLane
        {
            Left,
            Middle,
            Right
        }

        /// <summary>
        /// What lane this npc wants to try to be in.
        /// </summary>
        public MainLane CurrentMainLaneTarget;

        // Are the main target-able lanes blocked
        public bool[] MainLaneBlocked;
        public int[] MainLaneBlockedDistance;

        // Is the lane to the left, in front, and right blocked?
        public bool[] ImmediateLanesBlocked;
        public int[] ImmediateLanesBlockedDistance;

        public List<AbilitySlot> AvailableAbilities;

        public float DistanceToTarget;
    }

    [Serializable]
    public struct DecisionTuning
    {
        /// <summary>
        /// At how far away should the ai care about there being terrain in front
        /// </summary>
        public int InFrontAvoidanceDistance;
    }

    [Serializable]
    public struct PersonalityStats
    {
        public RandomStatFloat TimeBetweenActions;


    }

    public DecisionTuning Tune = new DecisionTuning();

    public PersonalityStats AggressiveStats = new PersonalityStats();
    public PersonalityStats PersistentStats = new PersonalityStats();
    public PersonalityStats PassiveStats = new PersonalityStats();

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
                    personality = (EnemyPersonality) Random.Range(0, 3);
                }
            }
        }
    }

    private AbilitiesComponent abilities;
    private HealthComponent health;
    private GridActor combatTarget;

    private ActionState currentActionState;

    private float timeUntilNextAction;
    private float actionTimer;

    private AbilitySlot activateSlot;

    [HideInInspector]
    public DecisionMakingData decisionData = new DecisionMakingData();

    /* Steps per action time:
     * 1). Decide upon the action type. Take into account:
     *      a. Distance to combat target
     *      b. Obstacles incoming
     *      c. Terrain incoming (find a path and weight the most prevalent direction)
     *      d. Personality weighting for ability categories
     *      e. Abilities in deck
     * 2). If needed, pick a best target for this action
     * 3). Do the action
     */

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        Personality = Personality;

        abilities = GetComponent<AbilitiesComponent>();
        health = GetComponent<HealthComponent>();
        if (health)
        {
            health.OnHealthChanged += (component, f, previousHealth) =>
            {
                SetJustHitObstacle();
            };
        }

        Assert.IsNotNull(abilities);
        Assert.IsNotNull(health);

        combatTarget = Service.Grid.PlayerActor;
    }

    // Update is called once per frame
    new void Update()
    {
        if (!Service.Game?.CurrentRace?.RaceInProgress ?? false)
        {
            return;
        }

        base.Update();

        switch (currentActionState)
        {
            case ActionState.PickWaitingTime:

                actionTimer = 0;
                activateSlot = null;
                timeUntilNextAction = Stats.TimeBetweenActions.Generate();

                currentActionState++;

                break;
            case ActionState.Waiting:

                actionTimer += Time.deltaTime;

                if (actionTimer >= timeUntilNextAction)
                {
                    currentActionState++;
                }

                break;
            case ActionState.StoreWorldState:

                if (StoreWorldState())
                {
                    currentActionState++;
                }
                else
                {
                    currentActionState = ActionState.PickWaitingTime;
                    break;
                }

                break;
            case ActionState.PickAction:

                if (DetermineNextAction())
                {
                    currentActionState++;
                }

                break;
            case ActionState.SetupAction:

                if (activateSlot == null)
                {
                    Debug.LogError("Activate slot is null :o");
                    currentActionState = ActionState.PickWaitingTime;
                    break;
                }

                if (InitialiseAction())
                {
                    currentActionState++;
                }

                break;
            case ActionState.DoAction:

                abilities.ActivateAbility(activateSlot.slotIndex);
                currentActionState++;

                break;
            case ActionState.PostAction:

                currentActionState = ActionState.PickWaitingTime;

                break;
        }
    }

    bool StoreWorldState()
    {
        bool IsObstacleBlocking(Vector2Int origin, out int dist)
        {
            dist = 0;

            foreach (var ob in Service.Game.CurrentRace.ObstacleList)
            {
                var gridActor = ob.GetComponent<GridActor>();
                if (!gridActor)
                {
                    continue;
                }

                var obstacleTile = gridActor.CurrentTile;

                //if not on origin lane not valid
                if (obstacleTile.x != origin.x)
                {
                    continue;
                }

                //Not valid if below the origin
                if (obstacleTile.y < origin.y)
                {
                    continue;
                }

                dist = obstacleTile.y - origin.y;
                return true;
            }

            return false;
        }

        decisionData.MainLaneBlocked = new bool[NUM_LANES];
        decisionData.MainLaneBlockedDistance = new int[NUM_LANES];

        // To the left, in front, and right of me
        decisionData.ImmediateLanesBlocked = new bool[NUM_IMMEDIATE_LANES];
        decisionData.ImmediateLanesBlockedDistance = new int[NUM_IMMEDIATE_LANES];

        decisionData.AvailableAbilities = new List<AbilitySlot>();


        // Grab terrain blocking data on main lanes
        for (int i = 0; i < NUM_LANES; i++)
        {
            var tilePoint = new Vector2Int(GetMainLaneTileX(i), TargetPosition.y);

            decisionData.MainLaneBlocked[i] = !Service.Grid.AreSpacesInDirectionFreeFromTerrain( tilePoint,
                                                                                                new Vector2Int(0, 1), 
                                                                                                Tune.InFrontAvoidanceDistance, 
                                                                                                out decisionData.MainLaneBlockedDistance[i]);

            //If not blocked by terrain, check if an obstacle is blocking it
            if (!decisionData.MainLaneBlocked[i])
            {
                decisionData.MainLaneBlocked[i] = IsObstacleBlocking(tilePoint, out decisionData.MainLaneBlockedDistance[i]);
            }
        }


        // Grab immediate lane data
        for (int i = 0; i < NUM_IMMEDIATE_LANES; i++)
        {
            Vector2Int tilePos = CurrentTile;

            if (i == 0)
            {
                tilePos.x--;
            }
            else if (i == 2)
            {
                tilePos.x++;
            }

            //If off the grid, it's invalid
            if (tilePos.x < 0 || tilePos.x >= Service.Grid.Columns)
            {
                decisionData.ImmediateLanesBlocked[i] = true;
                continue;
            }

            decisionData.ImmediateLanesBlocked[i] = !Service.Grid.AreSpacesInDirectionFreeFromTerrain(tilePos,
                new Vector2Int(0, 1),
                Tune.InFrontAvoidanceDistance,
                out decisionData.ImmediateLanesBlockedDistance[i]);

            //If not blocked by terrain, check if an obstacle is blocking it
            if (!decisionData.ImmediateLanesBlocked[i])
            {
                decisionData.ImmediateLanesBlocked[i] = IsObstacleBlocking(tilePos, out decisionData.ImmediateLanesBlockedDistance[i]);
            }

        }


        // Add all actionable abilities
        foreach (var abilSlot in abilities.Slots)
        {
            // Cannot use an ability on cooldown
            if (abilSlot.isOnCooldown)
            {
                continue;
            }
            
            // Do not bother with evaluating identical abilities
            if (decisionData.AvailableAbilities.Count > 0
                && decisionData.AvailableAbilities.Any(x => x.ability?.IsIdentical(abilSlot.ability) ?? false))
            {
                continue;
            }

            decisionData.AvailableAbilities.Add(abilSlot);
        }


        // Distance to target
        if (combatTarget)
        {
            decisionData.DistanceToTarget = Vector3.Distance(combatTarget.transform.position, transform.position);
        }
        else decisionData.DistanceToTarget = 0;


        if (decisionData.AvailableAbilities.Count == 0)
        {
            // This should not be getting hit at all. Would be worried if it did.
            Debug.LogError("Didn't manage to find an ability");
            return false;
        }

        return true;
    }

    bool DetermineNextAction()
    {
        activateSlot = decisionData.AvailableAbilities[0];

        return true;
    }

    bool InitialiseAction()
    {


        return true;
    }

    private int GetMainLaneTileX(int lane)
    {
        return GetMainLaneTileX((DecisionMakingData.MainLane) lane);
    }
    private int GetMainLaneTileX(DecisionMakingData.MainLane lane)
    {
        switch (lane)
        {
            case DecisionMakingData.MainLane.Left:
                return Mathf.FloorToInt(Service.Grid.Columns * 0.25f);

            default:
            case DecisionMakingData.MainLane.Middle:
                return Mathf.FloorToInt(Service.Grid.Columns * 0.5f);

            case DecisionMakingData.MainLane.Right:
                return Mathf.FloorToInt(Service.Grid.Columns * 0.75f);
        }
    }

    public PersonalityStats Stats
    {
        get
        {
            switch (personality)
            {
                case EnemyPersonality.PassiveRacer:
                    return PassiveStats;

                case EnemyPersonality.Persistent:
                    return PersistentStats;

                default:
                case EnemyPersonality.Aggressive:
                    return AggressiveStats;
            }
        }
    }

}
