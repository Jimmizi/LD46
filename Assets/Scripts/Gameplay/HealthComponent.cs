using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthComponent : MonoBehaviour
{
    public delegate void HealthEvent(HealthComponent healthComponent, float health, float previousHealth);

    public List<ParticleSystem> ParticlesToStopOnDead = new List<ParticleSystem>();

    public bool DebugKill;

    public float maxHealth = 100;
    public float currentHealth = 100;

    /// <summary>
    /// 0.0 to 1.0 percentage of health
    /// </summary>
    public float HealthPercentage => currentHealth / maxHealth;

    [Range(0, 50)]
    public float HealthSubtractAmountPerInterval = 1;

    public float HealthSubtractInterval = 1;

    private float healthTimer;

    private int shieldCount = 0;

    public event HealthEvent OnHealthDepleted;
    public event HealthEvent OnHealthRestored;
    public event HealthEvent OnHealthChanged;

    public bool MoveOffscreenOnDeath;

    public GameObject SpawnOnDeath = null;

    public ShieldComponent ShieldFxPrefab = null;

    ShieldComponent ShieldFx = null;

    public void Offset(float offset, bool backupCall = false)
    {
        float previousHealth = currentHealth;

        if (offset<-HealthSubtractAmountPerInterval)
        {
            if(ReduceShield())
            {
                return;
            }
        }

        currentHealth += offset;

        if (currentHealth <= 0)
        {
            currentHealth = 0.0f;            
        }

        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;            
        }

        if (currentHealth != previousHealth)
        {
            OnHealthChanged?.Invoke(this, currentHealth, previousHealth);
        }

        if (currentHealth <= 0 && (previousHealth > 0.0f || backupCall))
        {
            OnHealthDepleted?.Invoke(this, currentHealth, previousHealth);

            foreach (var ptfx in ParticlesToStopOnDead)
            {
                if (ptfx)
                {
                    ptfx.Pause(true);
                }
            }

            if (SpawnOnDeath)
            {
                GameObject.Instantiate(SpawnOnDeath, transform);
                SpawnOnDeath = null;

                var animator = GetComponent<Animator>();
                if (animator)
                {
                    Destroy(animator);
                }
            }

            if (MoveOffscreenOnDeath)
            {
                var grid = GetComponent<GridActor>();
                var ptfx = GetComponentsInChildren<ParticleSystem>();

                foreach (var ps in ptfx)
                {
                    var main = ps.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.Local;
                }


                if (grid)
                {
                    grid.TargetPosition = new Vector2Int(grid.TargetPosition.x, -4);
                    grid.MoveSpeed = 4;
                    grid.LockTargetPosition = true;
                    grid.SelfDestroyOnTargetReached = true;
                    grid.UseLinearMovementSpeed = true;
                }
            }
        }

        if (currentHealth == maxHealth && previousHealth < maxHealth)
        {
            OnHealthRestored?.Invoke(this, currentHealth, previousHealth);
        }
    }

    public void AddShield()
    {
        if( shieldCount == 0 )
        {
            ShieldFx = GameObject.Instantiate<ShieldComponent>(ShieldFxPrefab, transform);
            ShieldFx.transform.parent = transform;
        }

        shieldCount++;
    }

    public bool ReduceShield()
    {
        // If we have shields, use one of them
        if (shieldCount > 0)
        {
            shieldCount--;

            if (shieldCount == 0 && ShieldFx)
            {
                ShieldFx.DestroyShield();
                ShieldFx = null;
            }

            return true;
        }

        return false;
    }

    void Update()
    {
        if (Service.Game && !Service.Game.IsRaceInProgress())
        {
            return;
        }

        if (HealthSubtractInterval > 0)
        {
            healthTimer += Time.deltaTime * GameplayManager.GlobalTimeMod;

            if (healthTimer >= HealthSubtractInterval)
            {
                if (CompareTag("Player"))
                {
                    Service.Score.AddScore(ScoreController.ScoreType.PerHealthTick);
                }

                Offset(-HealthSubtractAmountPerInterval);
                healthTimer = 0;
            }
        }


        // Backup
        //if (currentHealth <= 0 || DebugKill)
        //{
        //    DebugKill = false;
        //    Offset(-100, true);
        //}
    }

    List<GameObject> ObstaclesHit = new List<GameObject>();
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Obstacle")
        {
            for (int i = ObstaclesHit.Count - 1; i >= 0; i--)
            {
                if (ObstaclesHit[i] == null)
                {
                    ObstaclesHit.RemoveAt(i);
                }
            }

            if (ObstaclesHit.Contains(other.gameObject))
            {
                return;
            }

            var obstacleComp = other.GetComponent<ObstaclePicker>();
            if (obstacleComp)
            {
                ObstaclesHit.Add(other.gameObject);

                if (ReduceShield())
                {
                    return;
                }                

                if (obstacleComp.HasPushBack)
                {
                    var grid = GetComponent<GridActor>();
                    if (grid)
                    {
                        grid.PushDown();
                    }
                }

                var carSounds = GetComponent<CarSounds>();
                if (carSounds)
                {
                    carSounds.PlayCrash();
                }

                Offset(-obstacleComp.GetCollisionDamage);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        
    }
}
