using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AbilityBase
{
    public float drawWeight { get; }
    public AbilityType category { get; }
    public AbilityTargeting targeting { get; }

    /// <summary> Override for non-targeting abilities </summary>
    public void Activate(AbilitySlot userSlot) { }

    /// <summary> Override for positional / directional abilities </summary>
    public void Activate(AbilitySlot userSlot, Vector2 target) { }

    /// <summary> Override for unit targeting abilities </summary>
    public void Activate(AbilitySlot userSlot, GameObject target) { }

    /// <summary> Implement for unit targeting abilities </summary>
    /// <param name="DeltaTime"></param>
    /// <returns> 'true' if the ability has finished or 'false' otherwise. </returns>
    public bool Update(AbilitySlot userSlot, float DeltaTime) { return false; }
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
