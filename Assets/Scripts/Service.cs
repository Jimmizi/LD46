﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public static class Service
{
    public static DriversUIManager DriveUI = null;
    public static MusicManager Music = null;
    public static SpeedGaugeController Speed = null;
    public static CheckpointCounter Counter = null;
    public static NewAbilityPoster AbilityPost = null;
    public static FlowManager Flow = null;
    public static OptionsMonobehaviour Options = null;
    public static EndScreen End = null;
    public static PlayableGrid Grid = null;
    public static GameplayManager Game = null;
    public static PrefabRegistry Prefab = null;
    public static StormController Storm = null;
    public static ScoreController Score = null;
}
