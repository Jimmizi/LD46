using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDelete : MonoBehaviour
{
    public float TimeToDestruct;

    private float timer;
    
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= TimeToDestruct)
        {
            Destroy(this.gameObject);
        }
    }
}
