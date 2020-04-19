using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class FuelGaugeRotator : MonoBehaviour
{
    public Gradient LowFuelGradient;
    public AnimationCurve GradientBlendMode;
    public Image FuelGaugeSprite;

    public float Under20Percent_GradientDuration = 3;
    public float Under10Percent_GradientDuration = 2;
    public float Under5Percent_GradientDuration = 1;

    public float MinFuelRotation;
    public float MaxFuelRotation;

    private HealthComponent playerHealthComponent;

    private float timer;

    public float targetRotation;

    private float maxDifference;

    private bool doingHealthLostFlash;

    // Start is called before the first frame update
    void Start()
    {
        targetRotation = MaxFuelRotation;
        maxDifference = Math.Abs(MaxFuelRotation - MinFuelRotation);

        var rot = transform.eulerAngles;
        rot.z = targetRotation;
        transform.eulerAngles = rot;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerHealthComponent == null)
        {
            if (Service.Game?.CurrentRace?.PlayerGameObject != null)
            {
                playerHealthComponent = Service.Game.CurrentRace.PlayerGameObject.GetComponent<HealthComponent>();

                playerHealthComponent.OnHealthChanged += (component, health, previousHealth) =>
                {
                    if (playerHealthComponent.currentHealth > 20 && previousHealth - health >= 7) // 7 is arbitrary 
                    {
                        Service.Grid?.PlayerActor?.SetJustHitObstacle();
                        StartCoroutine(DoHealthLostFlash());
                    }
                };
            }
        }
        else
        {
            targetRotation = MaxFuelRotation + (maxDifference * (1 - (playerHealthComponent.currentHealth / 100)));

            var rot = transform.eulerAngles;
            rot.z = Mathf.LerpAngle(rot.z, targetRotation, 3 * Time.deltaTime);
            transform.eulerAngles = rot;
        }

        if (!doingHealthLostFlash && FuelGaugeSprite && playerHealthComponent)
        {
            if (playerHealthComponent.currentHealth > 20)
            {
                FuelGaugeSprite.color = Color.white;
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;

                var durationToUse = 0f;

                if (playerHealthComponent.currentHealth <= 20 && playerHealthComponent.currentHealth > 10)
                {
                    durationToUse = Under20Percent_GradientDuration;
                }
                else if (playerHealthComponent.currentHealth <= 10 && playerHealthComponent.currentHealth > 5)
                {
                    durationToUse = Under10Percent_GradientDuration;
                }
                else //under/equal 5
                {
                    durationToUse = Under5Percent_GradientDuration;
                }

                var mod = timer / durationToUse;

                FuelGaugeSprite.color = LowFuelGradient.Evaluate(1 * GradientBlendMode.Evaluate(mod));

                if (timer >= durationToUse)
                {
                    timer = 0;
                }
            }
        }
    }

    IEnumerator DoHealthLostFlash()
    {
        doingHealthLostFlash = true;
        int flashesDone = 0;
        float tim = 0;

        const float flashTimeInterval = 0.3f;

        while (flashesDone < 4)
        {
            tim += Time.deltaTime;

            var mod = tim / flashTimeInterval;

            FuelGaugeSprite.color = LowFuelGradient.Evaluate(1 * GradientBlendMode.Evaluate(mod));

            if (tim > flashTimeInterval)
            {
                flashesDone++;
                tim = 0;
            }

            yield return null;
        }

        doingHealthLostFlash = false;
    }
}
