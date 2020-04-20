using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowFuelIndicator : MonoBehaviour
{
    public Image FuelImage;
    // Start is called before the first frame update
    void Start()
    {
        FuelImage.enabled = false;
    }

    private HealthComponent playerHealth = null;

    // Update is called once per frame
    void Update()
    {
        if (Service.Game?.CurrentRace?.PlayerGameObject == null)
        {
            FuelImage.enabled = false;
            playerHealth = null;
            return;
        }

        if (playerHealth == null)
        {
            playerHealth = Service.Game.CurrentRace.PlayerGameObject.GetComponent<HealthComponent>();
        }
        else
        {
            FuelImage.enabled = playerHealth.HealthPercentage < 0.35f;
        }
    }
}
