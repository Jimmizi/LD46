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

    public CanvasGroup GameUICanvasGroup;

    public float InitialFadeInTime = 1;

    /// <summary>
    /// Canvas group for the initial fade in on game start
    /// </summary>
    private CanvasGroup gameStartFadeInGroup;

    private EndScreen endScreenRef = null;

    void Awake()
    {
        Service.Flow = this;
        gameStartFadeInGroup = GetComponentInChildren<CanvasGroup>();
        Assert.IsNotNull(gameStartFadeInGroup, "Flow manager did not find a canvas group component in a child. ERROR.");
    }

    // Start is called before the first frame update
    void Start()
    {
        Service.Flow = this;
        gameStartFadeInGroup.alpha = 1f;
        GameUICanvasGroup.alpha = 0f;

#if DEBUG && UNITY_EDITOR
        if (SkipTitleScreen)
        {
            GameUICanvasGroup.alpha = 1f;
            gameStartFadeInGroup.alpha = 0f;
            SetupGameStart();
            return;
        }
#endif
        var title = (GameObject)Instantiate(Service.Prefab.TitleScreen);
        var titleScreen = title.GetComponent<TitleScreen>();

        Assert.IsNotNull(titleScreen);

        // Set up the start of the game and alpha out the title screen fader
        titleScreen.OnTitleFadedOut += (sender, args) => SetupGameStart();
        StartCoroutine(FadeIntoGameAtStart());
    }

    // Update is called once per frame
    void Update()
    {
        if (endScreenRef && endScreenRef.ReadyToFadeBackIn)
        {
            //Destroy gameplay manager so SetupGameStart can make a new one
            
            Service.Game.Shutdown();
            Destroy(Service.Game.gameObject);

            Service.Game = null;

            endScreenRef.FadeBackIn();
            SetupGameStart();
        }
    }


    private void SetupGameStart()
    {
        endScreenRef = null;

        var gameplay = (GameObject)Instantiate(Service.Prefab.GameplayManager);
        var gameplayRef = gameplay.GetComponent<GameplayManager>();

        Assert.IsNotNull(gameplayRef);

        gameplayRef.OnGameFinished += (o, eventArgs) =>
        {
            var end = (GameObject)Instantiate(Service.Prefab.EndScreen);
            var endScreen = end.GetComponent<EndScreen>();

            Assert.IsNotNull(endScreen);

            endScreenRef = endScreen;
        };
    }

    private IEnumerator FadeIntoGameAtStart()
    {
        yield return new WaitForSeconds(1f);

        while (gameStartFadeInGroup.alpha > 0)
        {
            gameStartFadeInGroup.alpha -= InitialFadeInTime * Time.deltaTime;
            yield return null;
        }
    }
}
