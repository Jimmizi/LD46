using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour {

    /// <summary>
    /// ScoreController reference (UI part)
    /// </summary>
    public ScoreController UIScoreController;

    private int score;
    
    void Start() {
        
    }

    void Update() {
        
    }

    public void AddScore(int score) {
        score = Math.Abs(this.score);
        UIScoreController.SetScore(score);
    }

    public void ResetScore() {
        score = 0;
        UIScoreController.SetScore(score);
    }
}
