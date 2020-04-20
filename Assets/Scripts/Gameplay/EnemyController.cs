using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Pair<T1, T2>
{
    public Pair(T1 f, T2 s)
    {
        First = f;
        Second = s;
    }
    public T1 First { get; set; }
    public T2 Second { get; set; }
}

public class ReverseComparer : IComparer<float>
{
    public int Compare(float x, float y)
    {
        // Compare y and x in reverse order.
        return y < x ? 1 : 0;
    }
}

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

    [Serializable]
    public struct DecisionTuning
    {
        /// <summary>
        /// At how far away should the ai care about there being terrain in front
        /// </summary>
        public int InFrontAvoidanceDistance;

        public int IdealTileDistanceAway;
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
    private List<AbilitySlot> shuffleSlots = new List<AbilitySlot>();




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
        public bool[] MainLaneBlockedIsTerrain;

        // Is the lane to the left, in front, and right blocked?
        public bool[] ImmediateLanesBlocked;
        public int[] ImmediateLanesBlockedDistance;
        public bool[] ImmediateLanesBlockedIsTerrain;

        public List<AbilitySlot> AvailableAbilities;

        public float DistanceToTarget;

        public AbilityType LastAbilityTypeUsed;

    }
    [HideInInspector]
    public DecisionMakingData decisionData = new DecisionMakingData();

    private enum ScoreAmount
    {
        Weak,
        Average,
        Good,
        Important,
        Critical
    }

    bool DetermineNextAction()
    {
        float idealDistance = Service.Grid.GetTileScale * Tune.IdealTileDistanceAway;

        //bool wantCloserToTarget = Math.Abs(decisionData.DistanceToTarget - idealDistance) > 0.5f;
        //bool wantToMoveAwayFromObstacle = decisionData.ImmediateLanesBlocked[1] && decisionData.ImmediateLanesBlockedDistance[1] < 3;

        const float ScoreWeak = 0.25f;
        const float ScoreAverage = 0.5f;
        const float ScoreGood = 0.75f;
        const float ScoreImportant = 1.0f;
        const float ScoreCritical = 2f;

        float movementScore = 0f;
        Vector2 directionScore = Vector2.zero;

        float attackScore = 0f;
        float supportScore = 0f;


        #region FIRST: Find out what kind of ability we want to use.


        int scoresAdded = 0;
        float runningScore = 0f;

        void AddScore(ScoreAmount scoretype)
        {
            float amount = 0f;

            switch (scoretype)
            {
                case ScoreAmount.Weak:
                    amount = ScoreWeak;
                    break;
                case ScoreAmount.Average:
                    amount = ScoreAverage;
                    break;
                case ScoreAmount.Good:
                    amount = ScoreGood;
                    break;
                case ScoreAmount.Important:
                    amount = ScoreImportant;
                    break;
                case ScoreAmount.Critical:
                    amount = ScoreCritical;
                    break;
            }

            runningScore += amount;
            scoresAdded++;
        }
        float GetFinalScore()
        {
            if (scoresAdded == 0)
            {
                return 0f;
            }

            float ret = runningScore /= scoresAdded;
            scoresAdded = 0;
            return ret;
        }

        // Only score movement if we have abilities for it
        if (decisionData.AvailableAbilities.Any(x => x.ability.category == AbilityType.Movement))
        {
            // If we're not within 2 tiles of our ideal range
            if (Math.Abs(decisionData.DistanceToTarget - idealDistance) > Service.Grid.GetTileScale * 2.2f)
            {
                AddScore(ScoreAmount.Good);
            }

            // If we're too close, we'll want to move away
            if (decisionData.DistanceToTarget < Service.Grid.GetTileScale * 1.2f)
            {
                AddScore(ScoreAmount.Important);
            }

            // If the lane ahead of us is blocked
            if (decisionData.ImmediateLanesBlocked[1])
            {
                // If it's terrain we care immediately
                if (decisionData.ImmediateLanesBlockedIsTerrain[1])
                {
                    // Important if further away, critical if closer
                    AddScore(decisionData.ImmediateLanesBlockedDistance[1] < 3 ? ScoreAmount.Critical : ScoreAmount.Important);
                }
                // Otherwise if it's just an obstacle we will deal with it later
                else if(decisionData.ImmediateLanesBlockedDistance[1] < 3)
                {
                    // Score depending on health amount, if we're good on health, then not as important
                    AddScore(health.HealthPercentage > 0.85f ? ScoreAmount.Good : ScoreAmount.Important);
                }
            }

            movementScore = GetFinalScore();
        }

        // Only score attacking if we have abilities for it
        if (decisionData.AvailableAbilities.Any(x => x.ability.category == AbilityType.Offensive))
        {
            //Blend between average and good so that it beats average but not good
            AddScore(ScoreAmount.Average);
            AddScore(ScoreAmount.Good);

            attackScore = GetFinalScore();
        }

        // Only score support if we have abilities for it
        if (decisionData.AvailableAbilities.Any(x => x.ability.category == AbilityType.Support))
        {
            var per = health.HealthPercentage;
            if (per < 0.75f)
            {
                // If we're under 75% health and we have a heal ability only score it at this point
                if (decisionData.AvailableAbilities.Any(x => x.ability is HealAbility))
                {
                    if (per < 0.25f)
                    {
                        AddScore(ScoreAmount.Critical);
                    }
                    else if (per < 0.5f)
                    {
                        AddScore(ScoreAmount.Important);
                    }
                    else if (per < 0.75f)
                    {
                        AddScore(ScoreAmount.Good);
                    }
                }
            }

            //TODO Add shield ability

            supportScore = GetFinalScore();
        }

        // Modify final scores depending on if the last ability used was of that type
        switch (decisionData.LastAbilityTypeUsed)
        {
            case AbilityType.Offensive:
                attackScore *= 0.6f;
                break;
            case AbilityType.Support:
                supportScore *= 0.7f;
                break;
            case AbilityType.Movement:
                movementScore *= 0.9f;
                break;
            case AbilityType.Upgrade:
                break;
        }

        AbilityType idealType;

        List<Pair<float, AbilityType>> abilityPriority = new List<Pair<float, AbilityType>>();

        var sorter = new float[3];
        sorter[0] = movementScore;
        sorter[1] = attackScore;
        sorter[2] = supportScore;
        var sorterTypes = new AbilityType[3];
        sorterTypes[0] = AbilityType.Movement;
        sorterTypes[1] = AbilityType.Offensive;
        sorterTypes[2] = AbilityType.Support;
        
        ReverseComparer rc = new ReverseComparer();
        System.Array.Sort(sorter, sorterTypes, rc);

        for (var index = 0; index < sorter.Length; index++)
        {
            abilityPriority.Add(new Pair<float, AbilityType>(sorter[index], sorterTypes[index]));
        }
        
        #endregion

        #region SECOND: Now we have a goal type, figure out what ability to use within that category


        foreach (var abilityType in abilityPriority)
        {
            if (abilityType.First <= 0.0f)
            {
                continue;
            }

            var AbilitiesForType = decisionData.AvailableAbilities.FindAll(x => x.ability.category == abilityType.Second);

            if (AbilitiesForType.Count == 0)
            {
                Debug.LogError("Something messed up. Got to scoring ability type actions but have no slots.");
                continue;
            }

            if (abilityType.Second == AbilityType.Movement)
            {
                /* The ideal position:
                 * 1. Is at the ideal distance
                 * 2. Not too close to other enemies
                 * 3. Out of the way from obstacles and terrain
                 */

                // First step, find good position within the above.
                //      Use distance to target from the position, influence map, terrain collisions, and obstacle paths

                var pathToPlayer = Service.Grid.GetPath(CurrentTile, combatTarget.TargetPosition);
                var extraTiles = new List<Vector2Int>();

                // Bleed out the path to widen it a little
                foreach (var tile in pathToPlayer)
                {
                    if (tile.x > 0)
                    {
                        var newTile = tile - new Vector2Int(-1, 0);
                        if (!pathToPlayer.Contains(newTile))
                        {
                            extraTiles.Add(newTile);
                        }
                    }
                    if (tile.x < Service.Grid.Columns-1)
                    {
                        var newTile = tile - new Vector2Int(1, 0);
                        if (!pathToPlayer.Contains(newTile))
                        {
                            extraTiles.Add(newTile);
                        }
                    }

                    if (tile.y > 0)
                    {
                        var newTile = tile - new Vector2Int(0, -1);
                        if (!pathToPlayer.Contains(newTile))
                        {
                            extraTiles.Add(newTile);
                        }
                    }

                    if (tile.y < Service.Grid.Rows - 1)
                    {
                        var newTile = tile - new Vector2Int(0, 1);
                        if (!pathToPlayer.Contains(newTile))
                        {
                            extraTiles.Add(newTile);
                        }
                    }
                }
                pathToPlayer.AddRange(extraTiles);

                float bestScore = 9999999999f;
                Vector2Int bestScoreTile = combatTarget.TargetPosition;

                // Score each tile in the widened path
                foreach (var tile in pathToPlayer)
                {
                    // If terrain don't even score
                    if (Service.Grid.TerrainCollisions[tile.x, tile.y])
                    {
                        continue;
                    }

                    // When better, influence map tile will have a lower score
                    float score = Service.Grid.presenceInfluenceMap[tile.x, tile.y];

                    float distToPlayer = Service.Grid.GetManhattanDistance(tile, combatTarget.TargetPosition) / 10f;

                    // If too close, multiply the score
                    if (distToPlayer < Tune.IdealTileDistanceAway)
                    {
                        distToPlayer *= 3;
                    }

                    score += distToPlayer;

                    // Lots of score if going to be blocked by terrain
                    int distToTerrain = 0;
                    if (!Service.Grid.AreSpacesInDirectionFreeFromTerrain(tile, new Vector2Int(0, 1),
                        Tune.InFrontAvoidanceDistance, out distToTerrain))
                    {
                        score += Tune.InFrontAvoidanceDistance - distToTerrain;
                    }

                    // Not as much score addition to obstacles
                    int distToObstacle = 0;
                    if (IsObstacleBlockingLane(tile, out distToObstacle))
                    {
                        score += (Tune.InFrontAvoidanceDistance - distToObstacle) * 0.25f;
                    }

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestScoreTile = tile;
                    }

                }



                // Second step, check what directions we can go in.

                //Direction to the target
                var heading = combatTarget.gameObject.transform.position - new Vector3(transform.position.x, transform.position.y, 0);
                heading.z = 0;

                var dist = heading.magnitude;
                var dir = heading / dist;
                

                List<Pair<float, AbilitySlot>> scores = new List<Pair<float, AbilitySlot>>();
               
                foreach (var mover in AbilitiesForType)
                {
                    if (!(mover.ability is MoveAbility mov))
                    {
                        continue;
                    }

                    var diff = dir - new Vector3(mov.direction.x, mov.direction.y, 0);
                    diff.x = Mathf.Abs(diff.x);
                    diff.y = Mathf.Abs(diff.y);


                    scores.Add(new Pair<float, AbilitySlot>(diff.x + diff.y, mover));
                }

                
                // Third step see if using directions would put us in harms way

                foreach (var s in scores)
                {
                    if (!(s.Second.ability is MoveAbility mov))
                    {
                        continue;
                    }

                    var targetTile = CurrentTile + mov.direction;

                    int distToTerrain = 0;
                    if (!Service.Grid.AreSpacesInDirectionFreeFromTerrain(targetTile, new Vector2Int(0, 1),
                        Tune.InFrontAvoidanceDistance, out distToTerrain))
                    {
                        s.First += (Tune.InFrontAvoidanceDistance - distToTerrain) * 0.75f;
                    }

                    int distToObst = 0;
                    if (IsObstacleBlockingLane(targetTile, out distToObst))
                    {
                        s.First += (Tune.IdealTileDistanceAway - distToObst) * 0.25f;
                    }
                }

                bestScore = 99999f;
                AbilitySlot bestAbil = null;

                for (int i = 0; i < scores.Count; i++)
                {
                    if (scores[i].First < bestScore)
                    {
                        bestScore = scores[i].First;
                        bestAbil = scores[i].Second;
                    }
                }

                if (bestAbil == null)
                {
                    bestAbil = AbilitiesForType[0];
                }

                activateSlot = bestAbil;
                return true;

            }
            else if (abilityType.Second == AbilityType.Offensive)
            {
                // For offensive just care about rarity

                int highestRarity = 0;
                int index = 0;

                AbilitiesForType.Shuffle();

                for (var i = 0; i < AbilitiesForType.Count; i++)
                {
                    var a = AbilitiesForType[i];
                    if (a.ability.rarity > highestRarity)
                    {
                        index = i;
                    }
                }

                activateSlot = AbilitiesForType[index];
                return true;
            }
            else if (abilityType.Second == AbilityType.Support)
            {
                // For support, priorise healing if low on health, otherwise use shields if we have them

                AbilitiesForType.Shuffle();
                
                bool hasShield = AbilitiesForType.Any(x => x.ability is ShieldAbility);
                bool hasHeal = AbilitiesForType.Any(x => x.ability is HealAbility);

                if (hasShield && hasHeal)
                {
                    if (health.HealthPercentage < 0.5f)
                    {
                        activateSlot = AbilitiesForType.First(x => x.ability is HealAbility);
                    }
                    else
                    {
                        activateSlot = AbilitiesForType.First(x => x.ability is ShieldAbility);
                    }
                }
                else
                {
                    activateSlot = AbilitiesForType[0];
                }

                return true;
            }
        }

        #endregion

        // If all else fails
        activateSlot = decisionData.AvailableAbilities[0];

        return true;
    }

    

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

        decisionData.LastAbilityTypeUsed = AbilityType.None;
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

                actionTimer += Time.deltaTime * GameplayManager.GlobalTimeMod;

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

                Debug.Log($"Actor at {CurrentTile} decided to use: {activateSlot.ability}");
                
                decisionData.LastAbilityTypeUsed = activateSlot.ability.category;
                abilities.ActivateAbility(activateSlot.slotIndex);

                foreach (var abil in shuffleSlots)
                {
                    abilities.ShuffleAbility(abil.slotIndex);
                }

                currentActionState++;

                break;
            case ActionState.PostAction:

                currentActionState = ActionState.PickWaitingTime;

                break;
        }
    }

    bool IsObstacleBlockingLane(Vector2Int origin, out int dist)
    {
        dist = 0;

        Service.Game.CurrentRace.FlushNullObstacles();

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

    bool StoreWorldState()
    {
        

        decisionData.MainLaneBlocked = new bool[NUM_LANES];
        decisionData.MainLaneBlockedDistance = new int[NUM_LANES];

        // To the left, in front, and right of me
        decisionData.ImmediateLanesBlocked = new bool[NUM_IMMEDIATE_LANES];
        decisionData.ImmediateLanesBlockedDistance = new int[NUM_IMMEDIATE_LANES];

        decisionData.MainLaneBlockedIsTerrain = new bool[NUM_LANES];
        decisionData.ImmediateLanesBlockedIsTerrain = new bool[NUM_IMMEDIATE_LANES];

        decisionData.AvailableAbilities = new List<AbilitySlot>();
        shuffleSlots.Clear();


        // Grab terrain blocking data on main lanes
        for (int i = 0; i < NUM_LANES; i++)
        {
            var tilePoint = new Vector2Int(GetMainLaneTileX(i), TargetPosition.y);

            decisionData.MainLaneBlockedIsTerrain[i] = false;
            decisionData.MainLaneBlocked[i] = !Service.Grid.AreSpacesInDirectionFreeFromTerrain( tilePoint,
                                                                                                new Vector2Int(0, 1), 
                                                                                                Tune.InFrontAvoidanceDistance, 
                                                                                                out decisionData.MainLaneBlockedDistance[i]);

            //If not blocked by terrain, check if an obstacle is blocking it
            if (!decisionData.MainLaneBlocked[i])
            {
                decisionData.MainLaneBlocked[i] = IsObstacleBlockingLane(tilePoint, out decisionData.MainLaneBlockedDistance[i]);
            }
            else
            {
                decisionData.MainLaneBlockedIsTerrain[i] = true;
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
                decisionData.ImmediateLanesBlockedIsTerrain[i] = true;
                continue;
            }

            decisionData.ImmediateLanesBlockedIsTerrain[i] = false;
            decisionData.ImmediateLanesBlocked[i] = !Service.Grid.AreSpacesInDirectionFreeFromTerrain(tilePos,
                new Vector2Int(0, 1),
                Tune.InFrontAvoidanceDistance,
                out decisionData.ImmediateLanesBlockedDistance[i]);

            //If not blocked by terrain, check if an obstacle is blocking it
            if (!decisionData.ImmediateLanesBlocked[i])
            {
                decisionData.ImmediateLanesBlocked[i] = IsObstacleBlockingLane(tilePos, out decisionData.ImmediateLanesBlockedDistance[i]);
            }
            else
            {
                decisionData.ImmediateLanesBlockedIsTerrain[i] = true;
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
