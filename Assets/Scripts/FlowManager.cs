using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using Time = UnityEngine.Time;

public class FlowManager : MonoBehaviour
{
#if DEBUG && UNITY_EDITOR
    public bool SkipTitleScreen;
#endif

    /// <summary>
    /// Title screen UI prefab for start of the game, has a canvas on it to present a start button to the player
    /// </summary>
    public GameObject TitleScreenPrefab;

    /// <summary>
    /// The manager for running the game, spawning objects, defining win/lose conditions
    /// </summary>
    public GameObject GameplayManagerPrefab;

    /// <summary>
    /// End screen UI prefab for when the game is over, allowing the player to restart
    /// </summary>
    public GameObject EndScreenPrefab;

    public float InitialFadeInTime = 1;

    /// <summary>
    /// Canvas group for the initial fade in on game start
    /// </summary>
    private CanvasGroup gameStartFadeInGroup;

    private EndScreen endScreenRef = null;

    void Awake()
    {
        
        gameStartFadeInGroup = GetComponentInChildren<CanvasGroup>();
        Assert.IsNotNull(gameStartFadeInGroup, "Flow manager did not find a canvas group component in a child. ERROR.");
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStartFadeInGroup.alpha = 1f;

#if DEBUG && UNITY_EDITOR
        if (SkipTitleScreen)
        {
            gameStartFadeInGroup.alpha = 0f;
            SetupGameStart();
            return;
        }
#endif
        var title = (GameObject)Instantiate(TitleScreenPrefab);
        var titleScreen = title.GetComponent<TitleScreen>();

        Assert.IsNotNull(titleScreen);

        // Set up the start of the game and alpha out the title screen fader
        titleScreen.OnPlayPressed += (sender, args) => SetupGameStart();
        StartCoroutine(FadeIntoGameAtStart());
    }

    // Update is called once per frame
    void Update()
    {
        if (endScreenRef && endScreenRef.ReadyToFadeBackIn)
        {
            //Destroy gameplay manager so SetupGameStart can make a new one
            Destroy(Service.Game.gameObject);

            endScreenRef.FadeBackIn();
            SetupGameStart();
        }
    }


    private void SetupGameStart()
    {
        endScreenRef = null;

        var gameplay = (GameObject)Instantiate(GameplayManagerPrefab);
        var gameplayRef = gameplay.GetComponent<GameplayManager>();

        Assert.IsNotNull(gameplayRef);

        gameplayRef.OnGameFinished += (o, eventArgs) =>
        {
            var end = (GameObject)Instantiate(EndScreenPrefab);
            var endScreen = end.GetComponent<EndScreen>();

            Assert.IsNotNull(endScreen);

            endScreenRef = endScreen;
        };
    }

    private IEnumerator FadeIntoGameAtStart()
    {
        while (gameStartFadeInGroup.alpha > 0)
        {
            gameStartFadeInGroup.alpha -= (1f / InitialFadeInTime) * Time.deltaTime;
            yield return null;
        }
    }
}
