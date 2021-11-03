// ProgressTrigger.cs
// Manages progress colliders and progress events
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressTrigger : MonoBehaviour
{
    public int progressType;    //'Type' (category) parameter passed to GameManager when triggered
    public int progressIndex;   //'Index' (specific event) parameter passed to GameManager when triggered
    public bool triggerOnCollide = true;

    //Trigger progress event on collision
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && triggerOnCollide)
        {
            TriggerProgress();
        }
    }

    //Call from component to manually trigger progress event
    public void TriggerProgress()
    {
        GameManager.instance.OnGameProgress(progressType, progressIndex);
    }
}
