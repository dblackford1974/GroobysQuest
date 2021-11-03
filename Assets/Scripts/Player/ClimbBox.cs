// ClimbBox.cs
// Trigger collider attached to player for ladder / net climbing and transitions
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbBox : MonoBehaviour
{
    public PlayerController player;
    public Collider2D grab;
    public Collider2D netFix;

    public void SetNetFix(bool setFix)
    {
        //Scale of collider is widened while on a ladder or net
        //  Optimal size is smaller for grabbing ladders/nets, and wider for horizontal net traversal 
        transform.localScale = (setFix) ? new Vector3(5.0f, 1.0f, 1.0f) : new Vector3(1.0f, 1.0f, 1.0f);
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        // Set climb mode when entering 'climb' collider
        player.SetClimbMode(true, false);
    }

    void OnTriggerExit2D(Collider2D hit)
    {
        // Exit climb mode when leaving 'climb' collider
        player.SetClimbMode(false, false);
    }

    void Start()
    {
        SetNetFix(false);
    }
}
