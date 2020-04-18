using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPrinting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Value of music is {Service.Options.Get().MusicVolume}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
