using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilitySpriteDB", menuName = "Abilities/Sprite Database", order = 1)]
public class AbilitySpritesDB : ScriptableObject
{
    public Sprite MoveLeft;
    public Sprite MoveRight;
    public Sprite MoveForward;
    public Sprite MoveBack;    
}
