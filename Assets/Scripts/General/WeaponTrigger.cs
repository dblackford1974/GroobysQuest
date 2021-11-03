// WeaponTrigger.cs
// Generic, configurable handler class for weapon triggers and colliders
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    public Vigor parent;
    public Vigor.AttackType attackType;
    public float damage;
    public float knockback;

    //For timed deployment (handled by Update)
    public bool useTimeFactor;
    public float timeStartFactor;
    public float timeStopFactor;
    
    [HideInInspector]
    public float damageBonus;

    private float enableStartTime;
    private bool isEnabled;
    private bool isDeployed;
    private new Collider2D collider;
    private float maxEnableTime;

    void Start()
    {
        collider = gameObject.GetComponent<Collider2D>();
        collider.enabled = false;
    }

    void Update()
    {
        //Process timed deployment
        if (enableStartTime != 0)
        {
            float delta = Time.time - enableStartTime;
            float factor = delta / maxEnableTime;
            
            //Check if weapon is deployed
            bool deploy = ((factor >= timeStartFactor) && (factor <= timeStopFactor));

            //Handle change in deployment status
            if (deploy != isDeployed)
            {
                isDeployed = deploy;
                collider.enabled = deploy;
            }
        }
    }

    //Enable or disable the weapon collider
    public void Enable(bool enable, float maxTime)
    {
        isEnabled = enable;
        
        if (!useTimeFactor)
        {
            isDeployed = enable;
            collider.enabled = enable;
        }
        else
        {
            //Set up parameters for timed deployment
            enableStartTime = (isEnabled) ? Time.time : 0;
            maxEnableTime = maxTime;
            if (!enable && isDeployed) collider.enabled = false;
            isDeployed = false;
        }
    }

    void OnCollisionEnter2D(Collision2D hit)
    {
        OnCollision(hit.gameObject);
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        OnCollision(hit.gameObject);
    }

    void OnCollision(GameObject hit)
    {
        if (hit.CompareTag("Player"))
        {
            //Pass damage, knockback, and attack details to Vigor hit by weapon
            Vigor v = hit.GetComponent<Vigor>();
            float dir = (parent.faceRight) ? 1.0f : -1.0f;
            float heightDelta = gameObject.transform.position.y - hit.transform.position.y;
            v.OnHit(damage * damageBonus, knockback * dir, attackType, heightDelta);
        }
    }
}
