// MusicManager.cs
// Manages playing of a specific background music track
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme;
    public AudioClip deathSong;
    public AudioClip victorySong;
    public AudioClip creditsSong;

    public int songIndex;

    private AudioClip currentClip;

    void Start()
    {
        if (songIndex > 0)
        {
            if (songIndex == 1)
            {
                Play(mainTheme);
            }
            else if (songIndex == 2)
            {
                Play(deathSong);
            }
            else if (songIndex == 3)
            {
                Play(victorySong);
            }
            else if (songIndex == 4)
            {
                Play(creditsSong);
            }
        }
    }

    private void Play(AudioClip clip)
    {
        currentClip = clip;

        StartCoroutine(PlayClip());
    }

    IEnumerator PlayClip()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = currentClip;

        //Continuously loop the music track based on track length
        while(gameObject.activeSelf)
        {
            audio.Play();
            yield return new WaitForSeconds(audio.clip.length);
        }
    }
}
