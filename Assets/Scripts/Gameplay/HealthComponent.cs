using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public delegate void HealthEvent(HealthComponent healthComponent, float health, float previousHealth);

    public List<ParticleSystem> ParticlesToStopOnDead = new List<ParticleSystem>();

    public float maxHealth = 100;
    public float currentHealth = 100;

    [Range(0, 50)]
    public float HealthSubtractAmountPerInterval = 1;

    public float HealthSubtractInterval = 1;

    private float healthTimer;

    public event HealthEvent OnHealthDepleted;
    public event HealthEvent OnHealthRestored;
    public event HealthEvent OnHealthChanged;

    public GameObject SpawnOnDeath = null;

    public void Offset(float offset, bool backupCall = false)
    {
        float previousHealth = currentHealth;

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
                ptfx.Pause(true);
            }

            if (SpawnOnDeath)
            {
                Instantiate(SpawnOnDeath, transform);
            }
        }

        if (currentHealth == maxHealth && previousHealth < maxHealth)
        {
            OnHealthRestored?.Invoke(this, currentHealth, previousHealth);
        }
    }

    void Update()
    {
        if (HealthSubtractInterval > 0)
        {
            healthTimer += Time.deltaTime;

            if (healthTimer >= HealthSubtractInterval)
            {
                Offset(-HealthSubtractAmountPerInterval);
                healthTimer = 0;
            }
        }


        // Backup
        if (currentHealth <= 0)
        {
            Offset(0);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Instadeath")
        {
            Offset(-maxHealth);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        
    }
}
