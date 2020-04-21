using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameComponent : MonoBehaviour
{    
    public GameObject user;        
    public float Damage = 10;

    public void SetUser(GameObject user)
    {
        this.user = user;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != user)
        {
            var hitHealth = collision.gameObject.GetComponent<HealthComponent>();

            if (hitHealth)
            {
                hitHealth.Offset(-Damage);
            }
        }
    }

    void Update()
    {
        if (!Service.Game?.CurrentRace?.RaceInProgress ?? false)
        {
            return;
        }

        Vector3 pos = transform.position;

        pos.y -= 3 * Time.deltaTime * GameplayManager.GlobalTimeMod;

        transform.position = pos;
    }
}
