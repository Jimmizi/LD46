using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletComponent : MonoBehaviour
{
    public float speed = 10.0f;
    public Vector2 direction;
    public GameObject user;
    public float FalloffPerSecond = 1;

    public float Damage = 10;

    private float bulletFalloff;

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
        this.direction.Normalize();
    }

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
                Destroy(this.gameObject);
            }
        }
    }

    bool ShouldDelete()
    {
        if (Service.Game?.CurrentRace?.PlayerGameObject == null)
        {
            return true;
        }

        return Vector3.Distance(transform.position,
            Service.Game.CurrentRace.PlayerGameObject.gameObject.transform.position) > 20;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Service.Game?.CurrentRace?.RaceInProgress ?? false)
        {
            return;
        }

        Vector3 pos = transform.position;

        pos.x += direction.x * speed * Time.deltaTime;
        pos.y += direction.y * speed * Time.deltaTime;

        pos.y -= bulletFalloff * Time.deltaTime;

        bulletFalloff += FalloffPerSecond * Time.deltaTime;

        transform.position = pos;

        if (ShouldDelete())
        {
            Destroy(this.gameObject);
        }
    }
}
