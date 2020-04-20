using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : MonoBehaviour
{
    public delegate void TargetDelegate(TargetObject targetObject);

    public TargetDelegate OnTargetReady;  

    public bool isReady = false;
    public GameObject targetUser;

    public bool TargetPlayer;

    public virtual Vector2 GetVector() { return new Vector2(); }

    public virtual GameObject GetUnit()
    {
        if (TargetPlayer)
        {
            return Service.Game?.CurrentRace?.PlayerGameObject;
        }

        return null;
    }

    public virtual TargetObject CreateFor(GameObject targetUser)
    {
        this.targetUser = targetUser;
        return GameObject.Instantiate<TargetObject>(this);
    }

    protected Vector3 GetMouseLocation()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0.0f;
        return mousePosition;
    }
}
