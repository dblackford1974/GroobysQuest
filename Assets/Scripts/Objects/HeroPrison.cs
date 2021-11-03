// HeroPrison.cs
// Manages the level objects to free an imprisoned hero 
// Author:  Dan Blackford
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HeroPrison : MonoBehaviour
{
    public GameObject breakLock;     //This lock faces the player and can be broken
    public GameObject passiveLock;   //This lock can't be reached by player
    public GameObject prisonerLock;  //Displays the hero in the cage
    public GameObject prisonerFree;  //Displays the freed hero at the totem
    public GameObject totemLock;     //Displays the deactivated hero totem
    public GameObject totemFree;     //Displays the activated hero totem

    private bool isFree = false;
 
    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("ScoreTrigger") && !isFree)
        {
            BreakLock();
            isFree = true;
        }
    }

    void Start()
    {
        //Initialize the hero to be imprisoned
        prisonerLock.SetActive(true);
        prisonerFree.SetActive(false);
        totemLock.SetActive(true);
        totemFree.SetActive(false);
    }

    private void BreakLock()
    {
        //Change states so hero displays freed and equiped beneath the hero totem
        breakLock.GetComponent<Animator>().SetInteger("LockState", 2);
        passiveLock.GetComponent<Animator>().SetInteger("LockState", 1);
        prisonerLock.SetActive(false);
        prisonerFree.SetActive(true);
        totemLock.SetActive(false);
        totemFree.SetActive(true);

        //Invoke the ProgressTrigger component to handle score / tutorial events
        ProgressTrigger trigger = gameObject.GetComponent<ProgressTrigger>();
        if (trigger != null) trigger.TriggerProgress();
    }
}
