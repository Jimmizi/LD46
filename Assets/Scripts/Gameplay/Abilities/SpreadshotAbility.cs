using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadshotAbility : AbilityBase
{
    public static float SPAWN_DELAY = 3;

    BulletComponent bulletPrefab;
    float spawnTimer = SPAWN_DELAY;
    Vector2 direction;

    public SpreadshotAbility(string name, Sprite sprite, BulletComponent bulletPrefab)
    {
        this.name = name;
        this.sprite = sprite;
        this.bulletPrefab = bulletPrefab;
        this.category = AbilityType.Offensive;
        this.targeting = AbilityTargeting.Line;
    }

    public SpreadshotAbility(SpreadshotAbility other)
        : base(other)
    {
        bulletPrefab = other.bulletPrefab;
    }

    public override bool IsIdentical(AbilityBase other)
    {
        return other is SpreadshotAbility;
    }

    public override AbilityBase Clone()
    {
        return new SpreadshotAbility(this);
    }

    public override bool Activate(AbilitySlot userSlot, Vector2 direction)
    {
        this.spawnTimer = SPAWN_DELAY;
        this.direction = direction;        
        return true;
    }

    public override bool Update(AbilitySlot userSlot, float DeltaTime)
    {
        spawnTimer -= DeltaTime;
        if(spawnTimer > 0)
        {
            return true;
        }

        Vector3 spawnLocation = userSlot.owner.transform.position;

        var bullet = GameObject.Instantiate<BulletComponent>(bulletPrefab, spawnLocation, Quaternion.identity);
        bullet.SetUser(userSlot.owner);
        bullet.SetDirection(direction);

        return false;
    }
}
