using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldAbility : AbilityBase
{
    public ShieldAbility(string name, Sprite sprite)
    {
        this.name = name;
        this.sprite = sprite;
        this.category = AbilityType.Support;        
    }

    public ShieldAbility(ShieldAbility other)
       : base(other)
    {
        
    }

    public override bool IsIdentical(AbilityBase other)
    {
        return other is ShieldAbility;
    }

    public override AbilityBase Clone()
    {
        return new ShieldAbility(this);
    }

    public override bool Activate(AbilitySlot userSlot)
    {
        var health = userSlot.owner.GetComponentInChildren<HealthComponent>();
        health?.AddShield();
        return false;
    }
}
