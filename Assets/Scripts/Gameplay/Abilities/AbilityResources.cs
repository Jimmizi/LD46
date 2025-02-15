﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilitySpriteDB", menuName = "Abilities/Resources", order = 1)]
public class AbilityResources : ScriptableObject
{
    public BulletComponent BulletPrefab;
    public BulletComponent RapidBulletPrefab;
    public BulletComponent NovaBulletPrefab;
    public FlameComponent FlamePrefab;
}
