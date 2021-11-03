// TriggerTrap.cs
// Base class for traps activated by a trigger
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerTrap : MonoBehaviour
{
    protected enum SoundId
    {
        DeploySpikes = 0,
        FireSpears = 1
    }

    public Missle[] missles;     //Missles attached to this trap
    public bool silent = false;  //Use to silence a single trap, useful to limit volume for linked traps

    protected float triggerStart;
    private TrapTimer trapTimer;
    private SoundPalette sounds;

    protected abstract SoundId GetTriggerSound();

    public void Trigger()
    {
        if (triggerStart == 0)
        {
            trapTimer.EnableNext(true);

            //Activate / launch missles
            EnableMissles(true);
            triggerStart = Time.time;

            if (!silent)
            {
                sounds.Play((int)GetTriggerSound());
            }
        }
    }

    //Deactivate all missles
    protected void Retract()
    {
        triggerStart = 0;
        EnableMissles(false);
    }

    protected void OnStart()
    {
        trapTimer = gameObject.GetComponent<TrapTimer>();
        sounds = gameObject.GetComponent<SoundPalette>();
        EnableMissles(false);
    }

    //Enable or disable all missles.  Override to customize.
    protected virtual void EnableMissles(bool enable)
    {
        for (int i = 0; i < missles.Length; i++)
        {
            missles[i].Enable(enable);
        }
    }
}
