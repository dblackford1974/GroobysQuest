// Missle.cs
// Handles collision and damage for trap 'missles' (spikes / spears)
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missle : MonoBehaviour
{
    public TriggerTrap parent;
    public Vigor.AttackType attackType = Vigor.AttackType.None;
    public Vector3 startPos;
    public float damage;
    public float knockback;

    private Rigidbody2D body;

    void Start()
    {
        body = gameObject.GetComponent<Rigidbody2D>();
    }

    public void Enable(bool enable)
    {
        gameObject.GetComponent<PolygonCollider2D>().enabled = enable;
        startPos = gameObject.transform.position;
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        float velX = 0;

        //Get velocity if rigid body attached (true for Spear)
        if (body != null) velX = body.velocity.x;

        if (hit.CompareTag("Player"))
        {
            //Check for direct hit with a Vigor
            Vigor v = hit.GetComponent<Vigor>();

            if (v == null)
            {
                //Retrieve Vigor for indirect Hit Box
                v = hit.GetComponent<PlayerHitBox>().vigor;
            }

            //Pass damage, knockback, and attack details to Vigor hit by missle
            float dir = (velX > 0) ? 1.0f : (velX < 0) ? -1.0f : 0.0f;
            float heightDelta = gameObject.transform.position.y - hit.transform.position.y;
            v.OnHit(damage, knockback * dir, attackType, heightDelta);
        }

        if (body != null)
        {
            //Reset spear to starting position
            body.velocity = Vector2.zero;
            gameObject.transform.position = startPos;
        }
    }
}
