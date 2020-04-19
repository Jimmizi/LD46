using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : MonoBehaviour
{
    public delegate void TargetDelegate(TargetObject targetObject);

    public TargetDelegate OnTargetReady;  

    public bool isReady = false;
    public GameObject targetUser;

    public virtual Vector2Int GetVector() { return new Vector2Int(); }
    public virtual GameObject GetUnit() { return null; }

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
