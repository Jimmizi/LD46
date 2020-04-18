using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesComponent : MonoBehaviour
{    
    // Start is called before the first frame update
    void Start()
    {
        SetupSlots();
        SetupDeck();        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetupSlots()
    {
        foreach (AbilitySlot slot in abilitySlots)
        {
            slot.owner = gameObject;
        }
    }

    void SetupDeck()
    {
        // Add abilities
        // abilityDeck.Add( new Ability...() );        
    }

    private const int NUM_SLOTS = 3;

    AbilitySlot[] abilitySlots      = new AbilitySlot[NUM_SLOTS];
    AbilityDeck abilityDeck         = new AbilityDeck();
}
