// SecondLevelManager.cs
// Manages specific events for 2nd playable level
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondLevelManager : LevelManager
{
    private int currentProgress;

    public override void OnGameProgress(int type, int index)
    {
        //Score event
        if (type == 1)
        {
            if (index > currentProgress)
            {
                if (index == 1)
                {
                    //Completion bonus for level 1 is at the start of level 2
                    AwardPoints(3, 1);
                    GameManager.instance.AddBonusLife();
                }
                else if (index == 2)
                {
                    //Completion bonus for level 2 is currently "end of game" bonus
                    AwardPoints(4, 1);
                }

                currentProgress = index;
            }
        }
        else
        {
            base.OnGameProgress(type, index);
        }
    }

    void Awake()
    {
        UI_Manager.instance.InitGameUI();
        currentProgress = 0;
    }

    void Start()
    {
        OnStartLevel();
    }

   protected override void OnNextLevel()
    {
        //Load End Game screen in Victory mode
        GameManager.instance.isVictory = true;
        UI_Manager.instance.LoadSceneByIndex(2);
    }
}
