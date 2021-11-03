// PlayerVigor.cs
// Handles player overrides for injury, death, and blocking
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVigor : Vigor
{
    [HideInInspector]
    public bool isDucking;
    
    [HideInInspector]
    public bool isRolling;
    
    private PlayerController controller;

    public void HealFull()
    {
        hits = maxHits;
        GameManager.instance.SetHealth(hits, maxHits);
    }

    protected override void OnDeath()
    {
        SoundManager.instance.SetMute(true);
        GameManager.instance.SetHealth(hits, maxHits);
        GameManager.instance.OnPlayerDeath();
    }

    protected override bool IsBlocked(AttackType attack, float height)
    {
        //Player ducking / rolling blocks any Kobold club attack (though typically too high to hit)
        bool blockIfDuck = (attack == AttackType.Swing);
        bool blockIfRoll = (attack == AttackType.Swing);

        bool block = (isDucking) ? blockIfDuck : (isRolling) ? blockIfRoll : false;

        if (block && isDucking) Debug.Log($"Player Blocked while Ducking:{attack}");
        if (block && isRolling) Debug.Log($"Player Blocked while Rolling:{attack}");

        return block;
    }

    protected override void OnBleed()
    {
        controller.Interrupt();
        GameManager.instance.SetHealth(hits, maxHits);
    }

    void Start()
    {
        OnStart();
        controller = GetComponent<PlayerController>();
        GameManager.instance.SetHealth(hits, maxHits);
    }

    void Update()
    {
        OnUpdate();
    }
}
