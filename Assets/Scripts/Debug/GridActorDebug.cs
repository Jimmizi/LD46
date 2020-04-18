using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridActor))]
public class GridActorDebug : MonoBehaviour
{
#if UNITY_EDITOR

    public bool EnableArrowKeyMovement = true;
    private GridActor gridActor = null;
    
    void Start()
    {
        gridActor = GetComponent<GridActor>();

    }

    
    void Update()
    {
        if (EnableArrowKeyMovement)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                gridActor.AddDirection(0, 1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                gridActor.AddDirection(0, -1);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gridActor.AddDirection(-1, 0);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gridActor.AddDirection(1, 0);
            }
        }
    }

#endif
}
