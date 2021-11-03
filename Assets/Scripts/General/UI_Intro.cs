using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Intro : MonoBehaviour
{
    public Text message;
    public Button start;
    public Button next;

    static public int currentPage;
    public int showPage;

    const string msg_ = "  ";
    const string msgN = "\n\n";

    const string msg01 = "Your name is Grooby Flusbim – a simple Gnome from the Deep Forest who hopes one day to be a Hero.";
    const string msg02 = "After several long years at the Monestary, you’ve completed your training in Martial Arts and are ready for your first Adventure!";
    const string msg03 = "In the present events, King Tombuk has been corrupted by the dark warlock Lord Protisk, and has spread his grip of tyranny and conquest throughout the surrounding lands.";
    const string msg04 = "What better place than the Deep Forest to serve as a headquarters to organize a Resistance against this new evil?";
    const string msg05 = "To prepare for this Resistance, you must first infiltrate a deadly Stronghold belonging to King Tombuk!";
    const string msg06 = "This Stronghold is being used to store an abundance of treasure seized by the conquests and taxes of the newly corrupted kingdom.";
    const string msg07 = "Reclaiming some of this treasure will surely help to fund the Resistance with supplies, weapons, and even mercenaries!";
    const string msg08 = "Further, Lord Protisk has kidnapped and imprisoned powerful heros into magical prisons.";
    const string msg09 = "If you can free some of these prisoners, they will become powerful allies to your cause.";
    const string msg10 = "If all else fails, the tales of your bravery and skill against the evil enforcers of Tombuk’s reign will inspire similar courage for others to rise in rebellion against the corrupted King, and to bring an end to the magical corruption of the dastardly Lord Protisk!";
    const string msg11 = "An infamous Dwarf Wizard, known only as \"The Alchemist\", awaits you at a nearby Shrine to prepare you for your Mission...";

    static public void NextPage()
    {
        currentPage++;
    }

    void Start()
    {
        currentPage = 0;
        showPage = 0;
        start.gameObject.SetActive(false);
        next.gameObject.SetActive(true);

        message.text = msg01 + msgN + msg02;        
    }

    void FixedUpdate()
    {
        //Debug.Log($"{showPage}=={currentPage}");

        if (showPage != currentPage)
        {
            Debug.Log(showPage);

            //Update page and buttons
            if (currentPage == 1)
            {
                message.text = msg03 + msgN + msg04 + msg_ + msg05;
            }
            else if (currentPage == 2)
            {
                message.text = msg06 + msg_ + msg07 + msgN + msg08 + msg_ + msg09;
            }
            else if (currentPage == 3)
            {
                message.text = msg10 + msgN + msg11;
                start.gameObject.SetActive(true);
                next.gameObject.SetActive(false);

                showPage = 0;
            }

            showPage = currentPage;
        }
    }

}
