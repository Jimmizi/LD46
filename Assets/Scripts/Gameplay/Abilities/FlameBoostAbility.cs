using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBoostAbility : AbilityBase
{
    FlameComponent falmePrefab;
    Vector3 spawnLocation;
    int numFlamesToSpawn = 3;
    int boost = 3;
    float timeToSpawn = 0.0f;
    float spawnDelay = 0.1f;

    public FlameBoostAbility(string name, Sprite sprite, FlameComponent falmePrefab, int boost = 3)
    {
        this.name = name;
        this.category = AbilityType.Movement;
        this.sprite = sprite;
        this.falmePrefab = falmePrefab;
    }

    public FlameBoostAbility(FlameBoostAbility other)
        : base(other)
    {
        falmePrefab = other.falmePrefab;
    }

    public override bool IsIdentical(AbilityBase other)
    {
        return other is FlameBoostAbility;
    }

    public override AbilityBase Clone()
    {
        return new FlameBoostAbility(this);
    }

    public override bool Activate(AbilitySlot userSlot)
    {
        spawnLocation = userSlot.owner.transform.position;

        numFlamesToSpawn = boost*2;

        for (int i = 0; i < boost; i++)
        {
            Move(userSlot.owner, new Vector2(0, 1));
        }

        return true;
    }

    public override bool Update(AbilitySlot userSlot, float DeltaTime)
    {
        timeToSpawn -= DeltaTime;
        if (timeToSpawn <= 0)
        {
            timeToSpawn = spawnDelay;
            numFlamesToSpawn--;

            spawnLocation.y += 0.25f;
            var flame = GameObject.Instantiate<FlameComponent>(falmePrefab, spawnLocation, Quaternion.identity);
            flame.SetUser(userSlot.owner);
        }

        return numFlamesToSpawn > 0;
    }

    public void Move(GameObject target, Vector2 direction)
    {
        var gridActor = target.GetComponentInChildren<GridActor>();
        gridActor?.MoveInDirection((int)direction.x, (int)direction.y);
    }
}
