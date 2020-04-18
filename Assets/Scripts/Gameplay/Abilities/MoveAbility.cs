using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAbility : AbilityBase
{
    public Vector2Int direction;

    public MoveAbility(string name, string spritePath, AbilityTargeting targeting, int x, int y)
    {
        this.name = name;
        this.category = AbilityType.Movement;
        this.sprite = Resources.Load<Sprite>(spritePath);
        this.targeting = targeting;
        this.direction = new Vector2Int(x,y);
        this.drawWeight = 1.0f;
    }

    public MoveAbility(MoveAbility other)
        : base(other)
    {
        direction = other.direction;
    }

    public override AbilityBase Clone()
    {
        return new MoveAbility(this);
    }

    public override bool Activate(AbilitySlot userSlot)
    {
        Move(userSlot.owner, direction);
        return false;
    }

    public override bool Activate(AbilitySlot userSlot, Vector2Int target)
    {
        Move(userSlot.owner, direction);
        return false;
    }

    public override bool Activate(AbilitySlot userSlot, GameObject target)
    {
        Move(target, direction);
        return false;
    }

    public void Move(GameObject target, Vector2Int direction)
    {
        var gridActor = target.GetComponentInChildren<GridActor>();
        gridActor?.MoveInDirection(direction.x, direction.y);
    }
}
