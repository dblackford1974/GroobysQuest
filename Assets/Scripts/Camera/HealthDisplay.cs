// HealthDisplay.cs
// Displays health and current spare lives as canvas overlay on camera view.
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public Image[] lives;
    public Sprite lifeOn;
    public Sprite lifeOff;
    public Color colorOn;
    public Color colorOff;
    public Image healthBar;
    public float healthMaxWidth;

    //Scale health bar to current
    public void SetHealth(float level)
    {
        float width = Mathf.Lerp(0, healthMaxWidth, level);

        Vector3 scale = healthBar.gameObject.transform.localScale;
        scale.y = width;
        healthBar.gameObject.transform.localScale = scale;
    }

    //Set enabled state on life slots based on count of free lives
    public void SetLives(int count)
    {
        for(int i = 0; i < lives.Length; i++)
        {
            lives[i].sprite = (i < count) ? lifeOn : lifeOff;
            lives[i].color = (i < count) ? colorOn : colorOff;
        }
    }
}
