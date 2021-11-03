// PlayerCheckpoint.cs
// Manages player checkpoints to heal and respawn player
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckpoint : MonoBehaviour
{
    public GameObject totemEnable;   //Displays activated player checkpoint totem
    public GameObject totemDisable;  //Displays deactivated player checkpoint totem
    
    [HideInInspector]
    public bool isEnabled {get; private set;} = false;

    private LevelManager levelManager;
    private bool hasHealed = false;

    //Display totem as disabled
    public void Reset()
    {
        totemEnable.SetActive(false);
        totemDisable.SetActive(true);
        isEnabled = false;
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        if (!isEnabled)
        {
            if (hit.gameObject.name == "Player1")
            {
                //Activate checkpoint
                GameManager.instance.ClearCheckpoints();
                SoundManager.instance.Play(SoundManager.SoundId.SetCheckpoint);
                isEnabled = true;
                totemEnable.SetActive(true);
                totemDisable.SetActive(false);

                if (!hasHealed)
                {
                    //Heal player on first activation
                    GameManager.instance.HealPlayer();
                    hasHealed = true;
                }
            }
        }
    }

    void Start()
    {
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        Reset();
    }
}
