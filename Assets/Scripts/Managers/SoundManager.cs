// SoundManager.cs
// Singleton to play global game sounds not tied to a specific game object
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum SoundId
    {
        ScorePoints = 0,
        SetCheckpoint = 1,
        SpawnPlayer = 2,
        SpawnKobold = 3,
        TutorialProgress = 4,
    }

    static public SoundManager instance {get; private set;}

    private SoundPalette sounds;

    public void SetMute(bool isMute)
    {
        AudioListener.pause = isMute;
        AudioListener.volume = (isMute) ? 0.0f : 1.0f;
    }

    //Call to play the specified sound
    public void Play(SoundId sound)
    {
        sounds.Play((int)sound);
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        sounds = gameObject.GetComponent<SoundPalette>();
    }
}
