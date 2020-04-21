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
        public RuntimeAnimatorController controller;

        [SerializeField]
        public int damageOnCollision;

        [SerializeField]
        public bool moveBackOne;
    }

    public List<ObstaclePreset> Obstacles = new List<ObstaclePreset>();
    public bool HasPushBack => Obstacles.Count > 0 && Obstacles[0].moveBackOne;

    // Start is called before the first frame update
    void Start()
    {
        Assert.IsTrue(Obstacles.Count > 0);
        Obstacles.Shuffle();

        GetComponent<SpriteRenderer>().sprite = Obstacles[0].sprite;

        if (Obstacles[0].controller)
        {
            var anim = gameObject.AddComponent<Animator>();
            anim.runtimeAnimatorController = Obstacles[0].controller as RuntimeAnimatorController;
        }
    }

    public float GetCollisionDamage => Obstacles[0].damageOnCollision;

    // Update is called once per frame
    void Update()
    {
        
    }
}
