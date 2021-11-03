// LevelManager.cs
// Base class for managing specific game level and related events
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelManager : MonoBehaviour
{
    public int scoreMultiplier;
    public float damageMultiplier;
    public float damageResistance;
    public PlayerCheckpoint[] checkpoints;

    //Standard point values for various events
    //  Events={Kill Kobold, Open Chest, Free Hero, Complete Level, Complete Game}
    static readonly int[] eventPoints = new int[] {100, 500, 1000, 1500, 4000};

    //Handle progress event
    public virtual void OnGameProgress(int type, int index)
    {
        if (type == 0)
        {
            //Standard score event
            AwardPoints(index, scoreMultiplier);
        }
        else if (type == -1)
        {
            //Event to trigger next level
            OnNextLevel();
        }
    }

    protected void AwardPoints(int eventIndex, int multiplier)
    {
        GameManager.instance.IncreaseScore(eventPoints[eventIndex] * multiplier);
        
        if (eventIndex > 0)
        {
            SoundManager.instance.Play(SoundManager.SoundId.ScorePoints);
        }
    }

    public void RespawnPlayer()
    {
        Vector2 useSpawnPoint = transform.localPosition;

        //Find an active checkpoint, otherwise use level entry position
        for(int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i].isEnabled)
            {
                useSpawnPoint = checkpoints[i].transform.position;
                break;
            }
        }

        GameManager.instance.SpawnPlayer(useSpawnPoint, damageMultiplier, damageResistance);        
    }

    public void ClearCheckpoints()
    {
        //Clear all active checkpoints (only one should be active at a time)
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].Reset();
        }
    }

    //Spawn player at the start of the level
    public void OnStartLevel()
    {
        GameManager.instance.SpawnPlayer(transform.localPosition, damageMultiplier, damageResistance);        
    }

    //Override to load the next level (or scene) in sequence
    protected abstract void OnNextLevel();
}
