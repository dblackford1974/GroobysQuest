// TrapTimer.cs
// Handles both periodic trap triggering and chained triggering with optional delay
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTimer : MonoBehaviour
{
    //For periodic traps
    public bool periodic;
    public float period;
    public float offset;  //Offset the period by x seconds
    
    //For chained traps
    public TriggerTrap nextTrap;
    public float nextDelay;

    private TriggerTrap triggerTrap;
    private float periodTime;
    private float nextTime;

    public void EnablePeriod(bool enable)
    {
        if (enable && periodic)
        {
            periodTime = Time.time;

            //Fix for time=0 at Start
            periodTime += 0.0001f;
        }
        else 
        {
            periodTime = 0;
        }
    }

    //Called by TriggerTrap when this trap is activated
    public void EnableNext(bool enable)
    {
        if (enable && (nextTrap != null))
        {
            //Start timing for chained trap trigger
            nextTime = Time.time;
        }
        else 
        {
            nextTime = 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        triggerTrap = gameObject.GetComponent<TriggerTrap>();

        EnablePeriod(true);

        //Handle offset
        periodTime -= offset;
    }

    void Update()
    {
        if (nextTime != 0)
        {
            float elapse = Time.time - nextTime;
            
            if ((triggerTrap != null) && (elapse > nextDelay))
            {
                //Trigger chained trap
                nextTrap.Trigger();
                EnableNext(false);
            }
        }

        if (periodTime != 0)
        {
            float elapse = Time.time - periodTime;
            
            if (periodic && (elapse > period))
            {
                //Trigger this trap, then reset period
                triggerTrap.Trigger();
                EnablePeriod(false);
                EnablePeriod(true);
            }
        }
    }
}
