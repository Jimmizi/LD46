using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class EnemyPicker : MonoBehaviour
{
    private static bool HasSpawnedBossMan = false;

    [Serializable]
    public class EnemyPreset
    {
        [SerializeField]
        public GameObject Prefab;

        [SerializeField]
        public int health;

        [SerializeField]
        public EnemyController.EnemyPersonality personality = EnemyController.EnemyPersonality.Random;
    }

    public List<EnemyPreset> Rank1Presets = new List<EnemyPreset>();
    public List<EnemyPreset> Rank2Presets = new List<EnemyPreset>();
    public List<EnemyPreset> Rank3Presets = new List<EnemyPreset>();
    public List<EnemyPreset> Rank4Presets = new List<EnemyPreset>();

    // Start is called before the first frame update
    void Awake()
    {
        Rank1Presets.Shuffle();
        Rank2Presets.Shuffle();
        Rank3Presets.Shuffle();
        Rank4Presets.Shuffle();

        int[] rankChance = new int[4];

        if (Service.Game.RaceCount <= 2)
        {
            rankChance[0] = 90;
            rankChance[1] = 10;
            rankChance[2] = 0;
            rankChance[3] = 0;
        }
        else if (Service.Game.RaceCount > 2 && Service.Game.RaceCount <= 4)
        {
            rankChance[0] = 60;
            rankChance[1] = 30;
            rankChance[2] = 10;
            rankChance[3] = 0;
        }
        else if (Service.Game.RaceCount > 5 && Service.Game.RaceCount <= 7)
        {
            rankChance[0] = 45;
            rankChance[1] = 25;
            rankChance[2] = 30;
            rankChance[3] = 0;
        }
        else
        {
            rankChance[0] = 20;
            rankChance[1] = 30;
            rankChance[2] = 40;
            rankChance[3] = 10;
        }

        var controller = GetComponent<EnemyController>();
        var health = GetComponent<HealthComponent>();
       
        Assert.IsNotNull(controller);
        Assert.IsNotNull(health);

        var generatedChance = Mathf.FloorToInt(Random.Range(0f, 10001f) / 100f);
        int rollingTotal = 0;

        int rankToSpawn = 0;

        for (int i = 0; i < 4; i++)
        {
            rollingTotal += rankChance[0];
            if (generatedChance <= rollingTotal)
            {
                rankToSpawn = i;
                break;
            }
        }

        EnemyPreset preset;

        switch (rankToSpawn)
        {
            
            case 1:
                preset = Rank2Presets[0];
                break;
            case 2:
                preset = Rank3Presets[0];
                break;
            case 3:
                preset = Rank4Presets[0];

                var collider = GetComponent<BoxCollider2D>();
                if (collider)
                {
                    //special collider increase for the boss
                    collider.size = new Vector2(collider.size.x, 2);
                }

                break;
            case 0:
            default:
                preset = Rank1Presets[0];
                break;
        }

        var spriteGo = (GameObject) Instantiate(preset.Prefab);
        spriteGo.transform.position = Vector3.zero;
        spriteGo.transform.SetParent(this.gameObject.transform);
        spriteGo.transform.position = Vector3.zero;

        if (health)
        {
            health.ParticlesToStopOnDead = GetComponentsInChildren<ParticleSystem>().ToList();
        }

        controller.Personality = preset.personality;

        health.maxHealth = preset.health;
        health.currentHealth = preset.health;
    }

}
