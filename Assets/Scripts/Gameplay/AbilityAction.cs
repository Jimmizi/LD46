using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AbilityAction
{
    public enum Category
    {
        Offensive,
        Support,
        Movement,
        Upgrade
    }

    public enum Rarity
    {
        Common,
        Rare,
        Epic
    }

    public float Damage;
    public float Healing;
}
