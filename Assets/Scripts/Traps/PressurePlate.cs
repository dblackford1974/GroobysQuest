// PressurePlate.cs
// Pressure plate to trigger traps when activated
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public TriggerTrap linkTrap;
    public float delay;

    private float triggerTime;

    void OnTriggerEnter2D(Collider2D hit)
    {
        if (triggerTime == 0)
        {
            if (delay != 0)
            {
                //Wait for delay
                triggerTime = Time.time;
            }
            else
            {
                //Immediate trigger
                linkTrap.Trigger();
            }
        }
    }

    void Start()
    {
        triggerTime = 0;
    }

    void Update()
    {
        //Process delay
        if ((triggerTime > 0) && ((Time.time - triggerTime) > delay))
        {
            //Delayed trigger
            linkTrap.Trigger();
            triggerTime = 0;
        }
    }
}
