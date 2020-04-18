using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDeck
{
    /// <summary> Adds an ability to the deck </summary>
    public void Add(AbilityBase newAbility)
    {
        if (newAbility != null)
        {
            abilities.Add(newAbility);
            combinedWeightTotal += newAbility.drawWeight;
        }
    }

    /// <summary> Returns a random ability </summary>
    public AbilityBase Draw()
    {
        float randomWeight = Random.value * combinedWeightTotal;
        float runningWeight = 0.0f;

        foreach (AbilityBase ability in abilities)
        {
            runningWeight += ability.drawWeight;
            if(runningWeight >= randomWeight)
            {
                return ability.Clone();
            }
        }

        return null;
    }

    private List<AbilityBase> abilities;
    private float combinedWeightTotal;
}
