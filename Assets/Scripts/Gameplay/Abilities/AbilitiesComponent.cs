using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesComponent : MonoBehaviour
{
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

    public AbilityTargeting GetCurrentTargeting()
    {
        return (currentTargetingSlot != null) ? currentTargetingSlot.targeting : AbilityTargeting.None;
    }

    public void SetTarget(Vector2Int target)
    {
        if(currentTargetingSlot != null)
        {
            currentTargetingSlot.SetTarget(target);
        }
    }

    public void SetTarget(GameObject target)
    {
        if (currentTargetingSlot != null)
        {
            currentTargetingSlot.SetTarget(target);
        }
    }

    public bool DrawAbility(int slotIndex)
    {
        return DrawAbility(GetSlot(slotIndex));
    }

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

    // Start is called before the first frame update
    void Start()
    {
        SetupSlots();
        SetupDeck();        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (AbilitySlot slot in abilitySlots)
        {
            slot.Update(Time.deltaTime);
        }
    }

    void SetupSlots()
    {
        for (int i = 0; i < NUM_SLOTS; i++)
        {
            abilitySlots[i] = new AbilitySlot(gameObject, i);
            abilitySlots[i].OnCooldownEnded += slot => DrawAbility(slot);
            DrawAbility(abilitySlots[i]);
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

    private const int NUM_SLOTS = 3;

    AbilitySlot[] abilitySlots      = new AbilitySlot[NUM_SLOTS];
    AbilityDeck abilityDeck         = new AbilityDeck();

    AbilitySlot currentTargetingSlot = null;
}
