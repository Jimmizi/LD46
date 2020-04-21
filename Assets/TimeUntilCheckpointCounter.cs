using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeUntilCheckpointCounter : MonoBehaviour
{
    public DialController Slot3;
    public DialController Slot2;
    public DialController Slot1;

    private int lastCheckpointValue;

    public void SetCounter(int value)
    {
        if (value < 10)
        {
            Slot3.SetToEmpty();
            Slot2.SetToEmpty();
            Slot1.SetTo(value);
        }
        else
        {
            int[] digits = ScoreController.GetDigits(value).ToArray();

            if (digits.Length == 2)
            {
                Slot3.SetToEmpty();
                Slot2.SetTo(digits[0]);
                Slot1.SetTo(digits[1]);
            }

            else if (digits.Length == 3)
            {
                Slot3.SetTo(digits[0]);
                Slot2.SetTo(digits[1]);
                Slot1.SetTo(digits[2]);
            }
        }
    }

    void Start()
    {
        
    }

    void resetCounter()
    {
        if (lastCheckpointValue != -1)
        {
            SetCounter(666);
            lastCheckpointValue = -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Service.Game?.CurrentRace)
        {
            float timeList = Service.Game.CurrentRace.RaceLengthTimer - Service.Game.CurrentRace.RaceTime;
            var counter = Mathf.FloorToInt(timeList);

            counter = Mathf.Clamp(counter, 0, 999);

            if (counter != lastCheckpointValue)
            {
                SetCounter(counter);
            }

            lastCheckpointValue = counter;
        }
        else resetCounter();
    }
}
