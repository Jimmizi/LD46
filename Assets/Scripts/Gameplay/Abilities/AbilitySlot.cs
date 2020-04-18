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

    public GameObject owner { get; set; }
    public State state { get; set; }
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
            ability.Activate(this);
        }
    }

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
                ability.Activate(this, Target);
                break;

        }
    }

    public void SetTarget(GameObject Target)
    {
        if (ability == null)
            return;

        switch (ability.targeting)
        {
            case AbilityTargeting.Unit:
                state = State.Active;
                ability.Activate(this, Target);
                break;
        }
    }

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