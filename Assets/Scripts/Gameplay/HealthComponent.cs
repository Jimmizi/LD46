using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public delegate void HealthEvent(HealthComponent healthComponent, float health, float previousHealth);

    public List<ParticleSystem> ParticlesToStopOnDead = new List<ParticleSystem>();

    public float maxHealth = 100;
    public float currentHealth = 100;

    public event HealthEvent OnHealthDepleted;
    public event HealthEvent OnHealthRestored;
    public event HealthEvent OnHealthChanged;

    public void Offset(float offset)
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

        if (currentHealth <= 0 && previousHealth > 0.0f)
        {
            OnHealthDepleted?.Invoke(this, currentHealth, previousHealth);

            foreach (var ptfx in ParticlesToStopOnDead)
            {
                ptfx.Pause(true);
            }
        }

        if (currentHealth == maxHealth && previousHealth < maxHealth)
        {
            OnHealthRestored?.Invoke(this, currentHealth, previousHealth);
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
