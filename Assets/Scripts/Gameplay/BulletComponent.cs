using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletComponent : MonoBehaviour
{
    public float speed = 2.0f;
    public Vector2 direction;


    public void SetDirection(Vector2Int direction)
    {
        this.direction = direction;
        this.direction.Normalize();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var hitHealth = collision.gameObject.GetComponent<HealthComponent>();

        if (hitHealth)
        {
            hitHealth.Offset(-35);
        }
    }    

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        pos.x += direction.x * speed;
        pos.y += direction.y * speed;

        transform.position = pos;
    }
}
