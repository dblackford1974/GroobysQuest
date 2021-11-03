// TutorialManager.cs
// Manages specific events for Tutorial level
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : LevelManager
{
    private GameObject progressBoulder;
    private Text tutorialText;
    private Image panelImage;
    private int currentProgress;

    public override void OnGameProgress(int type, int index)
    {
        if ((type == 1) && (index > currentProgress))
        {
            //Load next tutorial message
            tutorialText.text = messages[index];

            //Cycle background colors for each message
            Vector4 color = colors[index % colors.Length];
            panelImage.color = color;

            if (index == 13)
            {
                //Remove boulder blocking progress after freeing the Alchemist
                progressBoulder.SetActive(false);
            }

            currentProgress = index;

            if ((index == 9) || (index == 13))
            {
                //Play 'score points' sound for opening chest and freeing Alchemist
                SoundManager.instance.Play(SoundManager.SoundId.ScorePoints);
            }
            else if (index > 0)
            {
                SoundManager.instance.Play(SoundManager.SoundId.TutorialProgress);
            }
        }
        else
        {
            base.OnGameProgress(type, index);
        }
    }

    void Start()
    {
        progressBoulder = GameObject.Find("boulder_low2");
        tutorialText = GameObject.Find("TutorialText").GetComponent<Text>();
        panelImage = GameObject.Find("TutorialTextPanel").GetComponent<Image>();
        currentProgress = -1;
        OnStartLevel();
        OnGameProgress(1, 0);
    }

    protected override void OnNextLevel()
    {
        UI_Manager.instance.LoadSceneByIndex(0);
    }

    Vector4[] colors = new Vector4[] 
    {
        //Background colors for tutorial messages
        new Vector4(230.0f, 255.0f, 240.0f, 192.0f) / 255.0f,
        new Vector4(255.0f, 240.0f, 230.0f, 192.0f) / 255.0f,
        new Vector4(240.0f, 230.0f, 255.0f, 192.0f) / 255.0f,
    };

    //Tutorial messages for each event
    private string[] messages = new string[] 
    {
        "Welcome to Grooby’s Quest!  I am the Alchemist and I’ll be your guide through this tutorial.  Control Grooby using the keyboard keys 'W', 'A', 'S', and 'D' to move Up, Left, Down, and Right respectively.  Now hold 'D' to run up into that building to the right.",
        "Grooby loves to run – great work!  With your feet touching the ground you can press Up ('W') to jump.  When holding a ladder, you can climb upward or move Down ('S') to climb downward.  Or if the ladder is beneath you, press down to climb down it.  Now jump to reach that ladder, and see if Grooby can climb his way to the top!",
        "Grooby has fun climbing, and it’s great exercise.  But sometimes Grooby needs to fight.  Tell Grooby to attack by pressing the 'E' key.  Press this now to have Grooby swing his favorite pair of Nunchucks, then run left down the hallway and climb the next ladder.",
        "Grooby has many talents.  While jumping or falling through the air, use the movement keys to nudge Grooby in any direction.  Grooby always lands on his feet, but can still be hurt if he falls too far.  Now run or jump off the ledge to the right and try to maneuver his fall.",
        "You landed that like a pro!  Now let’s try Grooby’s favorite attack.  Jump into the air while standing, walking, or running, then press Attack ('E') to send Grooby into a flying kick!  Now use the flying kick to jump over the boulder to the right.",
        "You’re getting fairly dangerous, but a few more moves will help.  To perform a somersault attack, press the 'F' key or double tap the movement key in the direction you are facing.  Grooby calls this “the Rolling Rock”.  Now use this move to slip under the boulder to the right.",
        "Good job!  Remember this move to keep your enemies off their feet.  Another ladder to climb?  No, this is a net.  Nets are like ladders but with room to climb from left to right.  Are you still wondering what to do?  Jump up and climb that net!  Then head over to the bridge on the right.",
        "We’re almost done, but let’s learn some defense.  To evade damage press down to duck, or move opposite your facing to back away.  Hold the movement key to start running in that direction, or press the 'R' key to quickly change your facing.  Here’s plenty of space to practice.  Head to the right across the bridge when you’re finished.",
        "What’s in that chest?  Too bad it’s locked.  See if you can break it open with those nunchucks!",
        "Nice!  This will help to fund the resistance, with some set aside for a refreshing beverage at the Tavern.  There’s more to learn, keep travelling to the right!",
        "What’s the deal with the funny symbol?  This is the totem of your monastic Order.  If you die on your adventure, your Order will reincarnate you to the furthest totem you’ve reached bearing this symbol.  Other totem symbols will summon enemies, or provide quick escape for heroes freed from prison.",
        "This is embarrassing but I need your help, please climb up the ladder and you’ll find me to the left…",
        "I managed to get myself locked in this blasted cage!  I was going to show you how they worked, but I suppose this will do well enough.  Just break that glowing orb with your nunchucks (you remember how right?).  This will break the binding spell that keeps me in this prison.  The nearby Totem of Heroes will do the rest!",
        "Thank you so much!  That was quite a bind I found myself in!  I removed the boulder from your path to the right.  Head right to the next downward ladder, climb down to the lower level, and keep travelling right to exit this shrine.  Thanks again!",
        "You’ve reached the end of this Tutorial!  I’ve shown you the basics, but the strongholds of King Tombuk will be more dangerous.  Watch out for fearsome monsters and deadly traps!  Remember what I’ve taught you.  Now head right or press 'Esc' to exit this level, and go play the game! (I mean 'spark the resistance'…  what game?)",
    };
}
