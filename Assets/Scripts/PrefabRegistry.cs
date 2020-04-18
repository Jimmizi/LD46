using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabRegistry : MonoBehaviour
{
    /// <summary>
    /// Title screen UI prefab for start of the game, has a canvas on it to present a start button to the player
    /// </summary>
    [SerializeField]
    public GameObject TitleScreen;

    /// <summary>
    /// The manager for running the game, spawning objects, defining win/lose conditions
    /// </summary>
    [SerializeField]
    public GameObject GameplayManager;

    [SerializeField]
    public GameObject RaceCoordinatorPrefab;

    /// <summary>
    /// End screen UI prefab for when the game is over, allowing the player to restart
    /// </summary>
    [SerializeField]
    public GameObject EndScreen;

    [SerializeField]
    public GameObject PlayerActor;

    void Awake()
    {
        Service.Prefab = this;
    }

}
