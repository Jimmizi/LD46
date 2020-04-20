using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadshotAbility : AbilityBase
{
    public static float SPAWN_DELAY = 0.0f;

    public int NumberOfShots = 3;

    BulletComponent bulletPrefab;
    float spawnTimer = SPAWN_DELAY;
    Vector2 direction;

    private float bulletSpawnTimer = 0.1f;
    private int numBulletsToSpawn;

    public SpreadshotAbility(string name, Sprite sprite, BulletComponent bulletPrefab, AbilityTargeting target = AbilityTargeting.Line)
    {
        this.name = name;
        this.sprite = sprite;
        this.bulletPrefab = bulletPrefab;
        this.category = AbilityType.Offensive;
        this.targeting = target;
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
        numBulletsToSpawn = NumberOfShots;
        return true;
    }

    public override bool Activate(AbilitySlot userSlot, GameObject target)
    {
        this.spawnTimer = SPAWN_DELAY;
        
        var heading = new Vector2(target.transform.position.x, target.transform.position.y) 
                      - new Vector2(userSlot.owner.transform.position.x, userSlot.owner.transform.position.y);

        var dist = heading.magnitude;
        this.direction = heading / dist;

        numBulletsToSpawn = NumberOfShots;
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

        bulletSpawnTimer -= DeltaTime;

        if (bulletSpawnTimer < 0)
        {
            bulletSpawnTimer = 0.1f;
            numBulletsToSpawn--;

            var bullet = GameObject.Instantiate<BulletComponent>(bulletPrefab, spawnLocation, Quaternion.identity);
            bullet.SetUser(userSlot.owner);

            var bulletComp = bullet.GetComponent<BulletComponent>();

            bulletComp.speed += Random.Range(-(bulletComp.speed / 10), (bulletComp.speed / 10));

            var dirPercentageX = direction.x / 10;
            var dirPercentageY = direction.y / 10;
            var rndDir = new Vector2(Random.Range(-dirPercentageX, dirPercentageX),
                Random.Range(-dirPercentageY, dirPercentageY));

            bullet.SetDirection(direction + rndDir);
        }
        
        return numBulletsToSpawn > 0;
    }
}
