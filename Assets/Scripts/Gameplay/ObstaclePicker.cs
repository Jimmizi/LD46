using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObstaclePicker : MonoBehaviour
{
    [Serializable]
    public struct ObstaclePreset
    {
        [SerializeField]
        public Sprite sprite;

        [SerializeField]
        public int damageOnCollision;
    }

    public List<ObstaclePreset> Obstacles = new List<ObstaclePreset>();

    // Start is called before the first frame update
    void Start()
    {
        Assert.IsTrue(Obstacles.Count > 0);
        Obstacles.Shuffle();

        GetComponent<SpriteRenderer>().sprite = Obstacles[0].sprite;
    }

    public float GetCollisionDamage => Obstacles[0].damageOnCollision;

    // Update is called once per frame
    void Update()
    {
        
    }
}
