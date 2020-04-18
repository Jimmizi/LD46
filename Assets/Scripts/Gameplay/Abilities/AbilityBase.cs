using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AbilityBase
{
    public float drawWeight { get; }
    public AbilityType category { get; }
    public AbilityTargeting targeting { get; }

    /// <summary> Override for non-targeting abilities </summary>
    /// <returns> 'true' if the ability is still active or 'false' otherwise. </returns>
    public bool Activate(AbilitySlot userSlot) { return false; }

    /// <summary> Override for positional / directional abilities </summary>
    /// <returns> 'true' if the ability is still active or 'false' otherwise. </returns>
    public bool Activate(AbilitySlot userSlot, Vector2 target) { return false; }

    /// <summary> Override for unit targeting abilities </summary>
    /// <returns> 'true' if the ability is still active or 'false' otherwise. </returns>
    public bool Activate(AbilitySlot userSlot, GameObject target) { return false; }

    /// <summary> Implement for unit targeting abilities </summary>
    /// <param name="DeltaTime"></param>
    /// <returns> 'true' if the ability is still active or 'false' otherwise. </returns>
    public bool Update(AbilitySlot userSlot, float DeltaTime) { return false; }    

    /// <summary> Creates a clone of this object </summary>
    public abstract AbilityBase Clone();
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
