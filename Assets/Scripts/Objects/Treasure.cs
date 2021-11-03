// Treasure.cs
// Manages the level objects to retrieve treasure from locked chests
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    private bool isOpen = false;

    //Update visual state of chest and trigger score event when opened
    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("ScoreTrigger") && !isOpen)
        {
            gameObject.GetComponent<Animator>().SetBool("IsOpen", true);

            ProgressTrigger trigger = gameObject.GetComponent<ProgressTrigger>();

            if (trigger != null) trigger.TriggerProgress();

            isOpen = true;
        }
    }
}
