// SpearTrap.cs
// Implements trap of spears launched horizontally from wall
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearTrap : TriggerTrap
{
    public float retractTime;
    public float speed;

    protected override SoundId GetTriggerSound()
    {
        return SoundId.FireSpears;
    }

    // Start is called before the first frame update
    void Start()
    {
        OnStart();
    }

    void Update()
    {
        if (triggerStart != 0)
        {
            if ((Time.time - triggerStart) > retractTime)
            {
                //Spear damage disabled after set retraction time
                Retract();
            }
        }
    }

    protected override void EnableMissles(bool enable)
    {
        base.EnableMissles(enable);

        if (enable)
        {
            //Set velocity of each spear based on trap's facing
            Vector2 v = new Vector2((transform.localScale.x > 0) ? -speed : speed, 0.0f);

            for (int i = 0; i < missles.Length; i++)
            {
                missles[i].GetComponent<Rigidbody2D>().velocity = v;
            }
        }
    }
}
