using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointCounter : MonoBehaviour
{
    public DialController Slot2;
    public DialController Slot1;

    void Awake()
    {
        Service.Counter = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Service.Counter = this;
        SetRaceCounter(0);
    }

    public void SetRaceCounter(int value)
    {
        if (value < 10)
        {
            Slot2.SetToEmpty();
            Slot1.SetTo(value);
        }
        else
        {
            int[] digits = ScoreController.GetDigits(value).ToArray();

            if (digits.Length == 2)
            {
                Slot2.SetTo(digits[0]);
                Slot1.SetTo(digits[1]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
