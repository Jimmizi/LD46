using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySlot
{
    public delegate void SlotEvent(AbilitySlot slot);

    public static float COOLDOWN_TIME = 5.0f;

    public enum State
    {
        Default,
        Targeting,
        Active,
        Clearing
    }

    /// <summary> The owner game object </summary>
    public int slotIndex { get; set; }

    /// <summary> The owner game object </summary>
    public GameObject owner { get; set; }

    /// <summary> Current state of the slot </summary>
    public State state { get; set; }

    /// <summary> Event fired when the cooldown ends </summary>
    public event SlotEvent OnCooldownEnded;

    /// <summary> The current cooldown time (counts down to 0) </summary>
    public float cooldownTimer { get; set; }

    /// <summary> True if the ability is on cooldown (cannot be set) </summary>
    public bool isOnCooldown
    {
        get { return cooldownTimer > 0.0f; }
    }    

    /// <summary> True if the ability is currently targeting </summary>
    public bool isTargeting
    {
        get { return state == State.Targeting && ability != null; }
    }

    /// <summary> Returns the current targeting state </summary>
    public AbilityTargeting targeting
    {
        get { return isTargeting ? ability.targeting : AbilityTargeting.None; }
    }

    /// <summary> The currently assigned ability </summary>
    public AbilityBase ability
    {
        get
        {
            return _ability;
        }

        set
        {
            if (!isOnCooldown)
            {
                _ability = value;
                state = State.Default;
            }
        }
    }
    private AbilityBase _ability;

    public AbilitySlot(GameObject owner, int index)
    {
        this.owner = owner;
        this.slotIndex = index;        
    }

    /// <summary> Activates the current ability </summary>
    public void Activate()
    {
        if (ability == null)
            return;

        if (state != State.Default)
            return;

        if (ability.targeting != AbilityTargeting.None)
        {
            state = State.Targeting;
        }
        else
        {
            DoActivate();            
        }
    }

    /// <summary> Sets the target of the ability if it requires a target. </summary>
    public void SetTarget(Vector2Int Target)
    {
        if (ability == null)
            return;

        if (state != State.Targeting)
            return;

        switch (ability.targeting)
        {
            case AbilityTargeting.Area:
            case AbilityTargeting.Cone:
            case AbilityTargeting.Line:
                DoActivate();
                break;
        }
    }

    /// <summary> Sets the target of the ability if it requires a target. </summary>
    public void SetTarget(GameObject Target)
    {
        if (ability == null)
            return;

        if (state != State.Targeting)
            return;

        switch (ability.targeting)
        {
            case AbilityTargeting.Unit:
                DoActivate();
                break;
        }
    }

    /// <summary> Updates the ability slot </summary>
    public bool Update(float DeltaTime)
    {        
        if (ability == null)
        {
            // No ability
            if (cooldownTimer > 0.0f)
            {
                cooldownTimer -= DeltaTime;
                if (cooldownTimer <= 0.0f)
                {
                    cooldownTimer = 0.0f;
                    OnCooldownEnded(this);
                }
            }

            return false;
        }
        else 
        {
            // Yes ability
            if (state == State.Active)
            {
                if(!ability.Update(this, DeltaTime))
                {
                    Clear(true);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }

    public void Clear(bool setOnCooldown)
    {
        ability = null;

        if (setOnCooldown)
        {
            cooldownTimer = COOLDOWN_TIME;
        }
    }

    private void DoActivate()
    {
        state = State.Active;
        if (!ability.Activate(this))
        {
            Clear(true);
        }
    }
}