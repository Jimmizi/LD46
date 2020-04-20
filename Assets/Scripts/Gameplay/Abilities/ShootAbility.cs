using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAbility : AbilityBase
{
    public static float SPAWN_DELAY = 0.0f;

    public int numberOfShots = 3;
    public float spread = 10;
    public float shootRate = 0.1f;
    public float speedVariation = 0.1f;

    BulletComponent bulletPrefab;
    float spawnTimer = SPAWN_DELAY;
    Vector2 direction;

    private float bulletSpawnTimer = 0.1f;
    private int numBulletsToSpawn;

    public ShootAbility(string name, Sprite sprite, BulletComponent bulletPrefab, int numberOfShots, float spread, float speedVariation, float shootRate,  AbilityTargeting target = AbilityTargeting.Line)
    {
        this.name = name;
        this.sprite = sprite;
        this.bulletPrefab = bulletPrefab;
        this.category = AbilityType.Offensive;
        this.targeting = target;
        this.numberOfShots = numberOfShots;
        this.spread = spread;
        this.shootRate = shootRate;
        this.speedVariation = speedVariation;
    }

    public ShootAbility(ShootAbility other)
        : base(other)
    {
        bulletPrefab = other.bulletPrefab;
        numberOfShots = other.numberOfShots;
        spread = other.spread;
        shootRate = other.shootRate;
        speedVariation = other.speedVariation;
    }

    public override bool IsIdentical(AbilityBase other)
    {
        return other is ShootAbility;
    }

    public override AbilityBase Clone()
    {
        return new ShootAbility(this);
    }

    public override bool Activate(AbilitySlot userSlot, Vector2 direction)
    {
        this.spawnTimer = SPAWN_DELAY;
        this.direction = direction;
        numBulletsToSpawn = numberOfShots;
        return true;
    }

    public override bool Activate(AbilitySlot userSlot, GameObject target)
    {
        this.spawnTimer = SPAWN_DELAY;
        
        var heading = new Vector2(target.transform.position.x, target.transform.position.y) 
                      - new Vector2(userSlot.owner.transform.position.x, userSlot.owner.transform.position.y);

        var dist = heading.magnitude;
        this.direction = heading / dist;

        numBulletsToSpawn = numberOfShots;
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
            bulletSpawnTimer = shootRate;
            numBulletsToSpawn--;

            var bullet = GameObject.Instantiate<BulletComponent>(bulletPrefab, spawnLocation, Quaternion.identity);
            bullet.SetUser(userSlot.owner);

            var bulletComp = bullet.GetComponent<BulletComponent>();

            bulletComp.speed *= 1 + Random.Range(-speedVariation, speedVariation);

            var newRandomDirection = Rotate(direction, Random.Range(-spread, spread));

            bullet.SetDirection(newRandomDirection);
        }
        
        return numBulletsToSpawn > 0;
    }

    public static Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}
