using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySlot
{
    public enum State
    {
        Default,
        Targeting,
        Active,
        Clearing
    }

    /// <summary> The owner game object </summary>
    public GameObject owner { get; set; }

    /// <summary> Current state of the slot </summary>
    public State state { get; set; }

    /// <summary> The currently assigned ability </summary>
    public AbilityBase ability
    {
        get
        {
            return _ability;
        }

        set
        {
            _ability = value;
            state = State.Default;
        }
    }
    private AbilityBase _ability;

    /// <summary> Activates the current ability </summary>
    public void Activate()
    {
        if (ability == null)
            return;

        if (ability.targeting != AbilityTargeting.None)
        {
            state = State.Targeting;
        }
        else
        {
            state = State.Active;
            if(!ability.Activate(this)) { ability = null; }
        }
    }

    /// <summary> Sets the target of the ability if it requires a target. </summary>
    public void SetTarget(Vector2 Target)
    {
        if (ability == null)
            return;
        
        switch (ability.targeting)
        {
            case AbilityTargeting.Area:
            case AbilityTargeting.Cone:
            case AbilityTargeting.Line:
                state = State.Active;
                if (!ability.Activate(this, Target)) { ability = null; }
                break;
        }
    }

    /// <summary> Sets the target of the ability if it requires a target. </summary>
    public void SetTarget(GameObject Target)
    {
        if (ability == null)
            return;

        switch (ability.targeting)
        {
            case AbilityTargeting.Unit:
                state = State.Active;
                if (!ability.Activate(this, Target)) { ability = null; }
                break;
        }
    }

    /// <summary> Updates the ability slot </summary>
    public bool Update(float DeltaTime)
    {
        if (ability == null)
            return false;

        bool abilityActive = false;
        if (state == State.Active)
        {
            abilityActive = ability.Update(this, DeltaTime);
        }

        if (!abilityActive)
        {            
            ability = null;
        }

        return abilityActive;
    }
}