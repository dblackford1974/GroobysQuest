// Vigor.cs
// Base class for player / enemy hit points and combat
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Vigor : MonoBehaviour
{
    public enum AttackType
    {
        None = 0,   //Environmental or calculated damage, can't be blocked
        Kick = 1,   //Player flying kick, Kobold jump kick, or flying Spear
        Swing = 2,  //Player Nunchuck or Kobold Club
        Sweep = 3,  //Player Rolling Rock or Kobold Foot Counter
    }

    protected enum SoundId
    {
        Blocked = 0,
        Hit = 1,
        Killed = 2,
    }

    public float maxHits = 100.0f;
    public float deathTime = 0.5f;
    public float bleedTime = 0.25f;
    public Vector4 deathColor = new Vector4(1.0f, 0.5f, 0.0f, 0.5f);
    public Vector4 bleedColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);

    [HideInInspector]
    public bool faceRight;

    [HideInInspector]
    public float resistDamage;

    [HideInInspector]
    public float resistKnock;

    protected float hits;
    
    private Rigidbody2D body;
    private SpriteRenderer render;
    private SoundPalette sounds;

    private float deathStartTime;
    private float bleedStartTime;

    protected void OnStart()
    {
        body = gameObject.GetComponent<Rigidbody2D>();
        render = gameObject.GetComponent<SpriteRenderer>();
        sounds = gameObject.GetComponent<SoundPalette>();
        hits = maxHits;
    }

    protected void OnUpdate()
    {
        Vector3 mine = transform.position;
        if (deathStartTime > 0)
        {
            //Process death fade out
            float delta = Time.time - deathStartTime;

            if (delta > deathTime)
            {
                GameObject.Destroy(gameObject);
                OnDeath();
            }
            else
            {
                render.color = Vector4.Lerp(Vector4.one, deathColor, (delta / deathTime));
            }
        }
        else if (bleedStartTime > 0)
        {
            //Process bleed coloring
            float delta = Time.time - bleedStartTime;

            if (delta > bleedTime)
            {
                bleedStartTime = 0;
            }
            else
            {
                float f = Mathf.Sin((delta / bleedTime) * Mathf.PI);
                render.color = Vector4.Lerp(Vector4.one, bleedColor, f);
            }
        }
        else
        {
            render.color = Vector4.one;
        }
    }

    //Called on death
    protected abstract void OnDeath();

    //Called on non-blocked hit
    protected abstract void OnBleed();

    //Check if an attack was blocked
    protected abstract bool IsBlocked(AttackType attackType, float height);

    public void OnHit(float damage, float knockback, AttackType attackType, float height)
    {
        if (hits > 0)
        {
            //Calculate damage and knockback
            damage *= resistDamage;
            knockback *= resistKnock;

            //Check if attack was blocked.
            if ((attackType == AttackType.None) || !IsBlocked(attackType, height))
            {
                hits -= damage;

                if ((hits <= 0) && (deathStartTime == 0))
                {
                    //Handle death
                    deathStartTime = Time.time;
                    sounds.Play((int)SoundId.Killed);
                }
                else if (bleedStartTime == 0)
                {
                    //Handle injury
                    bleedStartTime = Time.time;
                    sounds.Play((int)SoundId.Hit);
                }

                if (knockback != 0)
                {
                    //Handle knockback
                    body.AddForce(new Vector2(knockback * 10.0f, 0.0f), ForceMode2D.Impulse);
                }

                OnBleed();
            }
            else
            {
                sounds.Play((int)SoundId.Blocked);
            }
        }
    }
}
