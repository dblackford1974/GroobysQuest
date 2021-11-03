// KoboldVigor.cs
// Handles Kobold enemy overrides for injury, death, and blocking
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoboldVigor : Vigor
{
    public float clubBlockHeight = 2.0f;

    [HideInInspector]
    public bool isBlocking;

    [HideInInspector]
    public bool isCounter;

    private KoboldController controller;

    protected override void OnDeath()
    {
        GameManager.instance.OnKoboldDeath();
    }

    protected override bool IsBlocked(AttackType attack, float height)
    {
        //All player "rolling rock" and nunchuck attacks blocked by Kobold club defense,
        //  also blocks kicks coming in at a low height
        bool canBlock = ((attack != AttackType.Kick) || (height < clubBlockHeight));

        //Player "rolling rock" attack is blocked by Kobold's specific counterattack
        bool canCounter = (attack == AttackType.Sweep);

        bool block = (isBlocking) ? canBlock : (isCounter) ? canCounter : false;

        if (block && isBlocking) Debug.Log($"Kobold Blocked:{attack},{height}");
        if (block && isCounter) Debug.Log($"Kobold Countered:{attack}");

        return block;
    }

    protected override void OnBleed()
    {
        controller.Interrupt();
    }

    void Start()
    {
        OnStart();
        controller = GetComponent<KoboldController>();
    }

    void Update()
    {
        OnUpdate();
    }
}
