using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public delegate void HealthEvent(HealthComponent healthComponent, float health, float previousHealth);


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
            OnHealthChanged(this, currentHealth, previousHealth);
        }

        if (currentHealth == 0 && previousHealth > 0.0f)
        {
            OnHealthDepleted(this, currentHealth, previousHealth);
        }

        if (currentHealth == maxHealth && previousHealth < maxHealth)
        {
            OnHealthRestored(this, currentHealth, previousHealth);
        }
    }
}
