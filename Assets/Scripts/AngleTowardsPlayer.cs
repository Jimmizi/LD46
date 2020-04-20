using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleTowardsPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Service.Game?.CurrentRace?.PlayerGameObject)
        {
            transform.up = Service.Game.CurrentRace.PlayerGameObject.transform.position - transform.position;
        }
    }
}
