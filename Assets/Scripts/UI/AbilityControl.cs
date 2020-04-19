using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityControl : MonoBehaviour
{
    /// <summary> The ability slot index </summary>
    public int slotIndex;    

    public UnityEngine.UI.Image abilityImage;
    public UnityEngine.UI.Image cooldownImage;

    GameObject _playerGameObject;
    AbilitiesComponent _abilitiesComponent;
    AbilitySlot _abilitySlot;
    UnityEngine.UI.Button _abilityButton;

    GameObject playerGameObject
    {
        get
        {
            if (!_playerGameObject)
            {
                _playerGameObject = GameObject.FindWithTag("Player");
            }
            return _playerGameObject;
        }
    }

    AbilitiesComponent abilitiesComponent
    {
        get
        {
            if(!_abilitiesComponent && playerGameObject)
            {
                _abilitiesComponent = playerGameObject.GetComponentInChildren<AbilitiesComponent>();
            }
            return _abilitiesComponent;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cooldownImage.material = Instantiate(cooldownImage.material);
    }

    // Update is called once per frame
    void Update()
    {
        if (!abilitiesComponent)
            return;

        if (abilityImage)
        {
            abilityImage.sprite = abilitiesComponent.GetAbilitySprite(slotIndex);
        }

        if(cooldownImage && cooldownImage.material)
        {
            float cooldown = abilitiesComponent.GetCooldownProgress(slotIndex);
            cooldownImage.material.SetFloat("_Cooldown", cooldown);
            cooldownImage.SetMaterialDirty();
        }
    }

    public void ActivateAbility()
    {
        if(abilitiesComponent)
        {
            abilitiesComponent.ActivateAbility(slotIndex);
        }
    }
}
