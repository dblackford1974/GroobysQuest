// KoboldSpawner.cs
// Manages Kobold enemy spawn points
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoboldSpawner : MonoBehaviour
{
    public GameObject totemEnable;  //Displays activated enemy spawn totem
    public GameObject totemDisable; //Displays deactivated enemy spawn totem
    
    [HideInInspector]
    public bool isActive {get; private set;} = false; 

    public void SetActive(bool active)
    {
        isActive = active;
        totemEnable.SetActive(active);
        totemDisable.SetActive(!active);
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        OnTrigger(hit);
    }

    void OnTriggerStay2D(Collider2D hit)
    {
        OnTrigger(hit);
    }

    void OnTrigger(Collider2D hit)
    {
        if (hit.gameObject.name == "Player1")
        {
            if (!isActive)
            {
                //Try to spawn Kobold
                if (GameManager.instance.CanSpawnKobold(this))
                {
                    //Activate totem
                    SetActive(true);
                }
            }
        }
    }

    void Start()
    {
        SetActive(false);
        isActive = false;
    }
}
