using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AbilityBase
{
    public string name = "Unknown";
    public AbilityType category;
    public AbilityTargeting targeting;
    public Sprite sprite;
    public float drawWeight = 1.0f;

    /// <summary> Base construct </summary>
    public AbilityBase() { }

    /// <summary> Copy construct </summary>
    public AbilityBase(AbilityBase other)
    {
        name = other.name;
        category = other.category;
        targeting = other.targeting;
        sprite = other.sprite;
        drawWeight = other.drawWeight;
    }

    /// <summary> Creates a clone of this object </summary>
    public abstract AbilityBase Clone();

    /// <summary> Override for non-targeting abilities </summary>
    /// <returns> 'true' if the ability is still active or 'false' otherwise. </returns>
    public virtual bool Activate(AbilitySlot userSlot) { return false; }

    /// <summary> Override for positional / directional abilities </summary>
    /// <returns> 'true' if the ability is still active or 'false' otherwise. </returns>
    public virtual bool Activate(AbilitySlot userSlot, Vector2 target) { return false; }

    /// <summary> Override for unit targeting abilities </summary>
    /// <returns> 'true' if the ability is still active or 'false' otherwise. </returns>
    public virtual bool Activate(AbilitySlot userSlot, GameObject target) { return false; }

    /// <summary> Implement for unit targeting abilities </summary>
    /// <param name="DeltaTime"></param>
    /// <returns> 'true' if the ability is still active or 'false' otherwise. </returns>
    public virtual bool Update(AbilitySlot userSlot, float DeltaTime) { return false; }        
}


public enum AbilityType
{
    Offensive,
    Support,
    Movement,
    Upgrade
}

public enum AbilityTargeting
{
    None,
    Area,
    Line,
    Cone,
    Unit
}
