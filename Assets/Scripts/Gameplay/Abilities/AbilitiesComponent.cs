using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesComponent : MonoBehaviour
{
    /// <summary> Activates the ability at the given slot </summary>
    /// <returns> The required targeting of the ability </returns>
    public AbilityTargeting ActivateAbility(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            return AbilityTargeting.None;
        }

        AbilitySlot slot = abilitySlots[slotIndex];
        slot.Activate();
        currentTargetingSlot = slot.isTargeting ? slot : null;

        return slot.targeting;
    }
    
    /// <returns> The required targeting of the currently active ability </returns>
    public AbilityTargeting GetCurrentTargeting()
    {
        return (currentTargetingSlot != null) ? currentTargetingSlot.targeting : AbilityTargeting.None;
    }

    /// <summary> Sets the target for the current active ability (as direction or area) </summary>
    public void SetTarget(Vector2Int target)
    {
        if(currentTargetingSlot != null)
        {
            currentTargetingSlot.SetTarget(target);
        }
    }

    /// <summary> Sets the target for the current active ability (as object) </summary>
    public void SetTarget(GameObject target)
    {
        if (currentTargetingSlot != null)
        {
            currentTargetingSlot.SetTarget(target);
        }
    }

    /// <summary> Draws a random ability from the deck into the given slot </summary>
    public bool DrawAbility(int slotIndex)
    {
        return DrawAbility(GetSlot(slotIndex));
    }

    /// <summary> Draws a random ability from the deck into the given slot </summary>
    public bool DrawAbility(AbilitySlot slot)
    {
        if (slot!=null)
        {
            if (slot.ability == null)
            {
                slot.ability = abilityDeck.Draw();
                return true;
            }
        }

        return false;
    }

    /// <returns> The the ability sprite </returns>
    public Sprite GetAbilitySprite(int slotIndex)
    {
        return GetAbility(slotIndex)?.sprite;
    }

    /// <returns> The the ability name </returns>
    public string GetAbilityName(int slotIndex)
    {
        return GetAbility(slotIndex)?.name;
    }

    /// <returns> The the ability cooldown progress (from 0 just started to 1 not on cooldown)  </returns>
    public float GetCooldownProgress(int slotIndex)
    {
        var slot = GetSlot(slotIndex);
        if (slot != null)
        {
            return 1.0f - slot.cooldownTimer / AbilitySlot.COOLDOWN_TIME;
        }

        return 1.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupDeck();
        SetupSlots();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (AbilitySlot slot in abilitySlots)
        {
            slot.Update(Time.deltaTime);
        }

        if (Input.GetButtonUp("Ability1"))
        {
            ActivateAbility(0);
        }

        if (Input.GetButtonUp("Ability2"))
        {
            ActivateAbility(1);
        }

        if (Input.GetButtonUp("Ability3"))
        {
            ActivateAbility(2);
        }

        if (Input.GetButtonUp("Ability4"))
        {
            ActivateAbility(3);
        }
    }

    void SetupSlots()
    {
        for (int i = 0; i < NUM_SLOTS; i++)
        {
            abilitySlots[i] = new AbilitySlot(gameObject, i);
            abilitySlots[i].OnCooldownEnded += slot => DrawAbility(slot);
            abilitySlots[i].Clear(true);
            //DrawAbility(abilitySlots[i]);
        };
    }

    void SetupDeck()
    {
        // Add abilities
        // abilityDeck.Add( new Ability...() );        

        abilityDeck.Add(new MoveAbility("Move Left", "AbilityMoveLeft.png", AbilityTargeting.None, -1, 0));
        abilityDeck.Add(new MoveAbility("Move Right", "AbilityMoveRight.png", AbilityTargeting.None, 1, 0));
        abilityDeck.Add(new MoveAbility("Move Forward", "AbilityMoveForward.png", AbilityTargeting.None, 0, -1));
        abilityDeck.Add(new MoveAbility("Move Back", "AbilityMoveBack.png", AbilityTargeting.None, 0, 1));
    }

    bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < abilitySlots.Length;
    }

    AbilitySlot GetSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            return abilitySlots[slotIndex];
        }

        return null;
    }

    AbilityBase GetAbility(int slotIndex)
    {
        return GetSlot(slotIndex)?.ability;
    }

    private const int NUM_SLOTS = 4;

    AbilitySlot[] abilitySlots      = new AbilitySlot[NUM_SLOTS];
    AbilityDeck abilityDeck         = new AbilityDeck();

    AbilitySlot currentTargetingSlot = null;
}
