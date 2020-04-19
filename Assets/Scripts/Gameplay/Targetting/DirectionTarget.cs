using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionTarget : TargetObject
{
    public Vector3 direction;
    public float renderDistance = 100;

    public LineRenderer lineRenderer;

    // Update is called once per frame
    void Update()
    {
        if (!targetUser)
            return;

        if(!isReady)
        {
            direction = GetMouseLocation() - targetUser.transform.position;
            direction.Normalize();

            //if(Input.GetMouseButtonDown(0))
            {
                isReady = true;
                OnTargetReady?.Invoke(this);
            }
        }

       // lineRenderer.SetPosition(0, targetUser.transform.position);
       // lineRenderer.SetPosition(1, targetUser.transform.position + direction * renderDistance);
    }

    public override Vector2 GetVector()
    {
        return new Vector2(direction.x, direction.y);
    }
}
