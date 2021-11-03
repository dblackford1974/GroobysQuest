// KoboldTrigger.cs
// The Kobold AI is controlled by Player interaction with triggers in different zones
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoboldTrigger : MonoBehaviour
{
    public KoboldController kobold;
    public int triggerId;

    void OnTriggerStay2D(Collider2D hit)
    {
        if (hit.CompareTag("Player"))
        {
            //Pass the trigger ID to the kobold controller
            kobold.OnStayTrigger(triggerId);
        }
    }
}
