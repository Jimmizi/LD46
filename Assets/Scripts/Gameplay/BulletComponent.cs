using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletComponent : MonoBehaviour
{
    public float speed = 10.0f;
    public Vector2 direction;
    public GameObject user;


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
                hitHealth.Offset(-35);
            }
        }
    }    

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        pos.x += direction.x * speed * Time.deltaTime;
        pos.y += direction.y * speed * Time.deltaTime;

        transform.position = pos;
    }
}
