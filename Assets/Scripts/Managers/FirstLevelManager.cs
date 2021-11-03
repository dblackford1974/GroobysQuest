// FirstLevelManager.cs
// Manages specific events for 1st playable level
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstLevelManager : LevelManager
{
    void Awake()
    {
        GameManager.instance.OnNewGame();
        UI_Manager.instance.InitGameUI();
    }

    void Start()
    {
        OnStartLevel();
    }

    protected override void OnNextLevel()
    {
        //Load level 2
        UI_Manager.instance.LoadSceneByIndex(5);
    }
}
