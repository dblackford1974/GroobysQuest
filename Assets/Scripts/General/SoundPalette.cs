// SoundPalette.cs
// Manages a collection of audio clips and related parameters
// Author:  Dan Blackford
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPalette : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public AudioClip clip = null;
        public float volume = 1.0f;
        public float pitch = 1.0f;
        public bool loop = false;
    }

    public Entry[] sounds;

    private AudioSource source;
    private Entry loopEntry;
    private bool isLooping;
    private float lastClipEnd;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void Silence()
    {
        Play(-1);
    }

    //Play clip at selected index
    public void Play(int index)
    {
        Entry entry = null;

        if ((index >= 0) && (index < sounds.Length))
        {
            entry = sounds[index];
        }

        if ((entry == null) || entry.loop)
        {
            //Don't interrupt non-looped clip with silence or a loop
            if (Time.time < lastClipEnd)
            {
                return;
            }
        }

        if (isLooping)
        {
            if ((entry != null) && (loopEntry == entry))
            {
                //Already looping
                return;
            }

            source.Stop();
            isLooping = false;
        }

        if ((source != null) && (entry != null) && (entry.clip != null))
        {
            //Get clip parameters
            source.clip = entry.clip;
            source.loop = entry.loop;
            source.volume = entry.volume;
            source.pitch = entry.pitch;
            isLooping = entry.loop;

            if (isLooping)
            {
                //Set up looping mode (low priority)
                loopEntry = entry;
                lastClipEnd = 0;
            }
            else
            {
                //Set up single fire clip mode (high priority)
                loopEntry = null;
                lastClipEnd = Time.time + entry.clip.length;
            }

            source.Play();
        }
    }
}
