using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAbility : AbilityBase
{
    float healAmount;

    public HealAbility(string name, Sprite sprite, float healAmount)
    {
        this.name = name;
        this.sprite = sprite;
        this.healAmount = healAmount;
        this.category = AbilityType.Support;        
    }

    public HealAbility(HealAbility other)
       : base(other)
    {
        healAmount = other.healAmount;
    }

    public override bool IsIdentical(AbilityBase other)
    {
        return other is HealAbility;
    }

    public override AbilityBase Clone()
    {
        return new HealAbility(this);
    }

    public override bool Activate(AbilitySlot userSlot)
    {
        var health = userSlot.owner.GetComponentInChildren<HealthComponent>();
        health?.Offset(healAmount);
        return false;
    }
}
