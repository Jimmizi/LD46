using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedGaugeController : MonoBehaviour
{
    public float MaxRot;
    public float MinRot;

    private float originalRot;

    private float timer;

    public enum SpeedType
    {
        Low, Normal, High, SlightlyLower
    }

    public float TargetRotation;

    private float tempTarget;
    private float tempVariance;

    void Awake()
    {
        Service.Speed = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        Service.Speed = this;
        TargetRotation = transform.eulerAngles.z;
        originalRot = TargetRotation;
    }

    public void SetSpeedAfterMove(SpeedType st)
    {
        if (st == SpeedType.Normal)
        {
            TargetRotation = originalRot;
        }
        else if (st == SpeedType.Low)
        {
            TargetRotation = MinRot;
        }
        else if (st == SpeedType.High)
        {
            TargetRotation = MaxRot;
        }
        else if (st == SpeedType.SlightlyLower)
        {
            TargetRotation = originalRot + 30;
        }

        timer = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        tempTarget = TargetRotation + tempVariance;

        if (timer <= 0)
        {
            TargetRotation = originalRot;
            tempVariance = Random.Range(-15, 15);
            timer = 0.5f;
        }
        else timer -= Time.deltaTime;

        var rot = transform.eulerAngles;
        rot.z = Mathf.LerpAngle(rot.z, tempTarget, 3 * Time.deltaTime);
        transform.eulerAngles = rot;

        
    }
}
