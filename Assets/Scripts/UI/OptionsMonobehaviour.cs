using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMonobehaviour : MonoBehaviour
{
    [SerializeField]
    private float musicPercentage;

    [SerializeField]
    private float sfxPercentage;

    [SerializeField]
    private bool dummyBooleanOption;

    public void Awake()
    {
        Service.Options = this;
    }

    public float MusicVolume
    {
        get => musicPercentage;
        set
        {
            musicPercentage = value;
        }
    }
    public float SfxVolume
    {
        get => sfxPercentage;
        set
        {
            sfxPercentage = value;
        }
    }

    public bool DummyBoolean
    {
        get => dummyBooleanOption;
        set
        {
            dummyBooleanOption = value;
        }
    }

    public void QuitGame()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
