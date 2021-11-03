// ScoreDisplay.cs
// Displays current score as canvas overlay on camera view.
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Text[] textStack;
    public int maxScore;

    //Update score display for main text and shadow layers
    public void SetScore(int setScore)
    {
        if (setScore > maxScore) setScore = maxScore;

        string score = $"{setScore:n0}";

        for (int i = 0; i < textStack.Length; i++)
        {
            textStack[i].text = score;
        }
    }
}
