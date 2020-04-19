using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSlotButtonPress : MonoBehaviour
{
    public int Slot;
    private Button myButton;

    private AbilitiesComponent playerAbil = null;

    // Start is called before the first frame update
    void Start()
    {
        myButton = GetComponent<Button>();
    }

    bool ShouldBeInteractable()
    {
        if (playerAbil == null)
        {
            if (Service.Game?.CurrentRace?.PlayerGameObject == null)
            {
                return false;
            }

            playerAbil = Service.Game.CurrentRace.PlayerGameObject.GetComponent<AbilitiesComponent>();

            
        }

        if (playerAbil == null)
        {
            return false;
        }

        return playerAbil.NeedsToLiftKey(Slot);
    }

    // Update is called once per frame
    void Update()
    {
        if (myButton)
        {

            

            if (Input.GetButton(AbilitiesComponent.GetInputKey(Slot)))
            {
                myButton.interactable = ShouldBeInteractable();
            }
            else
            {
                myButton.interactable = true;
            }
        }
        
    }
}
