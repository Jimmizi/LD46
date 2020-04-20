using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFontLerper : MonoBehaviour
{
    public float Duration;

    public Text FontText;
    public AnimationCurve Curve;

    public int MaxSize;
    public int MinSize;

    private float timer;
    private CanvasGroup group;

    // Start is called before the first frame update
    void Start()
    {
        group = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (group && group.alpha <= 0)
        {
            return;
        }

        timer += Time.deltaTime;

        var mod = timer / Duration;
        var curveAmount = Curve.Evaluate(1f * mod);

        FontText.fontSize = MaxSize - Mathf.FloorToInt((MaxSize - MinSize) * curveAmount);

        if (timer >= Duration)
        {
            timer = 0;
        }
    }
}
