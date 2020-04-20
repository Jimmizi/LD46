using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriversUIManager : MonoBehaviour {

    public Material SpeedDialMaterial;
    public Material FuelDialMaterial;

    public GameObject TirePressureLight;
    public GameObject HazardLight;
    public GameObject LowFuelLight;
    public GameObject EngineFaultLight;
    
    private float currentSpeed;
    private float currentFuel;
    private bool tirePressureLightOn = true;
    private bool hazardLightOn = true;
    private bool lowFuelLightOn = true;
    private bool engineFaultLightOn = true;

    public bool TirePressureLightOn => tirePressureLightOn;
    public bool HazardLightOn => hazardLightOn;
    public bool LowFuelLightOn => lowFuelLightOn;
    public bool EngineFaultLightOn => engineFaultLightOn;

    void Start()
    {
        Service.DriveUI = this;
    }

    void Awake()
    {
        Service.DriveUI = this;
    }

    void Update() {
        
    }

    public void SetSpeedFactor(float speed) {
        currentSpeed = Mathf.Abs(speed);
        currentSpeed = (currentSpeed > 1.0f) ? 1.0f : currentSpeed;

        if (SpeedDialMaterial != null) {
            SpeedDialMaterial.SetFloat("_Turn", currentSpeed);
        }
    }
    
    public void SetFuelFactor(float fuel) {
        currentFuel = Mathf.Abs(fuel);
        currentFuel = (currentFuel > 1.0f) ? 1.0f : currentFuel;

        if (FuelDialMaterial != null) {
            FuelDialMaterial.SetFloat("_Turn", currentFuel);
        }
    }

    
    public void SetTirePressureLight(bool onOff) {
        tirePressureLightOn = onOff;
        if (TirePressureLight != null) {
            TirePressureLight.SetActive(onOff);
        }
    }
    
    public void SetHazardLight(bool onOff) {
        hazardLightOn = onOff;
        if (HazardLight != null) {
            HazardLight.SetActive(onOff);
        }
    }
    
    public void SetLowFuelLight(bool onOff) {
        lowFuelLightOn = onOff;
        if (LowFuelLight != null) {
            LowFuelLight.SetActive(onOff);
        }
    }
    
    public void SetEngineFaultLight(bool onOff) {
        engineFaultLightOn = onOff;
        if (EngineFaultLight != null) {
            EngineFaultLight.SetActive(onOff);
        }
    }

    
}
