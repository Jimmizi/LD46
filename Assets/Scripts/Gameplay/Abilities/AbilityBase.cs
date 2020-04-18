using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AbilityBase
{
    public virtual string name { get; set; }
    public virtual AbilityType category { get; set; }
    public virtual AbilityTargeting targeting { get; set; }
    public virtual Sprite sprite { get; set; }
    public virtual float drawWeight { get; set; }

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
    public virtual bool Activate(AbilitySlot userSlot, Vector2Int target) { return false; }

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
