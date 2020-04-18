using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUtil : MonoBehaviour
{
    public bool CenterCamera = true;

    // Start is called before the first frame update
    void Start()
    {
        if (CenterCamera)
        {
            var padding = new Vector3(Service.Grid.GetTotalPaddingX, Service.Grid.GetTotalPaddingY, 0) / 2;

            transform.position = new Vector3(0,0, transform.position.z) +
                new Vector3(Service.Grid.Columns * Service.Grid.GetTileScale, Service.Grid.Rows * Service.Grid.GetTileScale) / 2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
