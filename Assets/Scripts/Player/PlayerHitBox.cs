// PlayerHitBox.cs
// Hit box trigger used to detect weapon hits on player, as workaround from 
//   using the primary collider, to allow traps to hit the player when climbing
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    public PlayerVigor vigor;
    public Collider2D hitStand;
    public Collider2D hitDuck;

    //Toggle internal colliders between duck/roll and stand/climb/fly modes
    public void SetDuck(bool duck)
    {
        hitStand.enabled = !duck;
        hitDuck.enabled = duck;
    }

    void Start()
    {
        SetDuck(false);
    }
}
