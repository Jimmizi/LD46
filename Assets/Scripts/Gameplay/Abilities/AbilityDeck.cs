using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDeck
{
    /// <summary> Adds an ability to the deck </summary>
    public void Add(AbilityBase newAbility, int quantity = 1)
    {
        if (newAbility != null)
        {
            for (int i = 0; i < quantity; i++)
            {
                abilities.Add(newAbility);
                combinedWeightTotal += newAbility.drawWeight;
            }
        }
    }

    private float GetAbilityWeightedScore(AbilityBase ability, int actorX, int actorY)
    {
        float score = ability.drawWeight;

        // Movement card scoring depending on actor location
        if (ability.category == AbilityType.Movement
            && ability is MoveAbility move)
        {
            if (move.direction.x != 0)
            {
                //actor on right side of board
                if (actorX > (Service.Grid.Columns / 2) + 1)
                {
                    if (move.direction.x > 0) 
                    {
                        // We don't want to go right when on the right side
                        score /= 3;
                    }
                    else
                    {
                        // We want to go left while on the right side
                        score *= 1.5f;
                    }
                }
                //left side of board
                else if (actorX < (Service.Grid.Columns / 2) - 1)
                {
                    if (move.direction.x > 0)
                    {
                        // we want to go right while on the left
                        score *= 1.5f; 
                    }
                    else
                    {
                        //we don't want to go left while on the left
                        score /= 3;
                    }
                }
            }
            else if (move.direction.y != 0)
            {
                //actor on top side of board
                if (actorY > (Service.Grid.Rows / 2) + 1)
                {
                    if (move.direction.y > 0)
                    {
                        // We don't want to go forward when on the top side
                        score /= 3;
                    }
                    else
                    {
                        // We want to go backward while on the top side
                        score *= 1.5f;
                    }
                }
                //bottom side of board
                else if (actorY < (Service.Grid.Rows / 2) - 1)
                {
                    if (move.direction.y > 0)
                    {
                        // we want to go forward while on the bottom
                        score *= 1.5f;
                    }
                    else
                    {
                        //we don't want to go backward while on the bottom
                        score /= 3;
                    }
                }
            }
        }

        return score;
    }

    private void CalculatedCombinedWeight(int actorX, int actorY)
    {
        combinedWeightTotal = 0;

        foreach (AbilityBase ability in abilities)
        {
            combinedWeightTotal += GetAbilityWeightedScore(ability, actorX, actorY);
        }
    }

    /// <summary> Returns a random ability </summary>
    public AbilityBase Draw(AbilitySlot[] currentAbilities, int actorX, int actorY)
    {
        CalculatedCombinedWeight(actorX, actorY);

        float randomWeight = Random.value * combinedWeightTotal;
        float runningWeight = 0.0f;

        abilities.Shuffle();

        foreach (AbilityBase ability in abilities)
        {
            runningWeight += GetAbilityWeightedScore(ability, actorX, actorY);

            // Don't add too many of the same ability
            int numSameAbility = 0;
            foreach (var abil in currentAbilities)
            {
                if(ability.IsIdentical(abil.ability))
                {
                    numSameAbility++;
                }
            }

            if (numSameAbility >= 2)
            {
                continue;
            }

            if (runningWeight >= randomWeight)
            {
                return ability.Clone();
            }
        }

        foreach (var abil in abilities)
        {
            if (abil is MoveAbility mov
                && mov.direction.x != 0)
            {
                return abil.Clone();
            }
        }

        return null;
    }

    private List<AbilityBase> abilities = new List<AbilityBase>();
    private float combinedWeightTotal = 0.0f;
}
