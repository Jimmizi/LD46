using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAbility : AbilityBase
{
    public Vector2Int direction;

    public const float VERTICAL_WEIGHT = 0.5f;
    public const float HORIZONTAL_WEIGHT = 2f;

    public MoveAbility(string name, Sprite sprite, AbilityTargeting targeting, int x, int y)
    {
        this.name = name;
        this.category = AbilityType.Movement;
        this.sprite = sprite;
        this.targeting = targeting;
        this.direction = new Vector2Int(x,y);

        if (x != 0)
        {
            this.drawWeight = VERTICAL_WEIGHT;
        }
        else if (y != 0)
        {
            this.drawWeight = HORIZONTAL_WEIGHT;
        }
    }

    public MoveAbility(MoveAbility other)
        : base(other)
    {
        direction = other.direction;
    }

    public override bool IsIdentical(AbilityBase other)
    {
        if (!(other is MoveAbility move))
        {
            return false;
        }

        return direction == move.direction;
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

    public override bool Activate(AbilitySlot userSlot, Vector2 target)
    {
        Move(userSlot.owner, direction);
        return false;
    }

    public override bool Activate(AbilitySlot userSlot, GameObject target)
    {
        Move(target, direction);
        return false;
    }

    public void Move(GameObject target, Vector2 direction)
    {
        var gridActor = target.GetComponentInChildren<GridActor>();
        gridActor?.MoveInDirection((int)direction.x, (int)direction.y);
    }
}
