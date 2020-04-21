using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilitySpriteDB", menuName = "Abilities/Sprite Database", order = 1)]
public class AbilitySpritesDB : ScriptableObject
{
    public Sprite Empty;
    public Sprite MoveLeft;
    public Sprite MoveRight;
    public Sprite MoveForward;
    public Sprite MoveBack;
    public Sprite Heal;
    public Sprite Shield;
    public Sprite SpreadShot;
    public Sprite RapidFire;
    public Sprite NovaShotBlast;
    public Sprite FlameBoost;
}
