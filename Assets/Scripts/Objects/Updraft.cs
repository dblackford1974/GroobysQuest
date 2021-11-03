// Updraft.cs
// Manages 'updraft' effect to propel player upward while in contact with collider
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Updraft : MonoBehaviour
{
    public float forceY = 1000.0f;

    public void OnTriggerStay2D(Collider2D hit)
    {
        Rigidbody2D body = hit.gameObject.GetComponent<Rigidbody2D>();

        //Add 'forceY' as continuous force over time, not as an impulse
        body.AddForce(new Vector2(0, forceY), ForceMode2D.Force);
    }
}
