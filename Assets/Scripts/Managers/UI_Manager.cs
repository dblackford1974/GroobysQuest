// UI_Manager.cs
// Singleton for top level user interface management across all scenes
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
    // Messages for Game Over screen
    private const string gameOverVictory = "You have survived your great adventure!  The hills and forests sing with the news of your heroic accomplishments, and the hearts of the oppressed stir with the valiant seeds of resistance…  or perhaps revolution!";
    private const string gameOverDeath = "You have died one too many times, and your Monestary couldn't resurrect you.  Surely you will be remembered for your heroic deeds, and as an inspiration for the impending resistance!  Too bad you didn't live to see it...";
    private const string scoreNewHighScore = "You have achieved a new high score of {0:n0}!  Your last highest score was {1:n0}.  Great job! (Can you do better?)";
    private const string scorePriorHighScore = "You have achieved a score of {0:n0} with this adventure, but your highest score was {1:n0}…  keep trying!";
    private const string scoreNoHighScore = "You have achieved a total score of {0:n0}!  This has been recorded as your highest score.  Can you do better?";
    private const string scoreNoPoints = "You did not score any points!  Try playing the Tutorial, and remember to score points by slaying enemies, freeing prisoners, or breaking open chests.";

    static public UI_Manager instance {get; private set;}

    public Dictionary<string, UnityAction> navButtonMap;

    public GameObject musicManagerPrefab;
    public GameObject gameCanvasPrefab;

    HealthDisplay healthDisplay;
    ScoreDisplay scoreDisplay;

    //Update Score display HUD
    public void SetScore(int score)
    {
        if (scoreDisplay != null)
            scoreDisplay.SetScore(score);
    }

    //Update Health Level HUD
    public void SetHealthLevel(float level)
    {
        if (healthDisplay != null)
            healthDisplay.SetHealth(level);
    }

    //Update Free Lives HUD
    public void SetLivesCount(int count)
    {
        if (healthDisplay != null)
            healthDisplay.SetLives(count);
    }

    //Initialize HUD display for playable level
    public void InitGameUI()
    {
        GameObject canvas = Instantiate(gameCanvasPrefab, Vector3.zero, Quaternion.identity);
        GameObject health = canvas.transform.Find("Health Panel").gameObject;
        GameObject score = canvas.transform.Find("Score Panel").gameObject;

        healthDisplay = health.GetComponent<HealthDisplay>();
        scoreDisplay = score.GetComponent<ScoreDisplay>();
    }

    //Initialize Game Over screen
    private void InitGameOverUI((int, int, bool) scores)
    {
        var (score, highScore, isVictory) = scores;
        Image imageVictory = GameObject.Find("Image_Victory").GetComponent<Image>();
        Image imageDeath = GameObject.Find("Image_Death").GetComponent<Image>();
        Text textMessage = GameObject.Find("Text_Message").GetComponent<Text>();
        Text textScore = GameObject.Find("Text_Score").GetComponent<Text>();

        imageVictory.enabled = isVictory;
        imageDeath.enabled = !isVictory;

        string messageText = (isVictory) ? gameOverVictory : gameOverDeath;
        string scoreFormat = (score == 0) ? scoreNoPoints : 
            (highScore == 0) ? scoreNoHighScore :
            (score <= highScore) ? scorePriorHighScore :
            scoreNewHighScore;
        string scoreText = string.Format(scoreFormat, score, highScore);

        textMessage.text = messageText;
        textScore.text = scoreText;

        GameManager.instance.UpdateHighScore();
    }

    //Load a scene by the load order index
    public void LoadSceneByIndex(int sceneIndex)
    {
        //Load and switch to the specified scene
        SceneManager.LoadScene(sceneIndex);
    }

    //Exit the game
    public void ExitGame()
    {
#if UNITY_EDITOR
        //In editor, so end the play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //Running full application, so quit
        Application.Quit();
#endif
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    //Initialize the singleton
    private void Initialize()
    {
        //Add callback on scene changed
        SceneManager.activeSceneChanged += ChangedActiveScene;

        //Create map for all nav buttons from name to desired action
        navButtonMap = new Dictionary<string, UnityAction>();
        navButtonMap.Add("Button_Tutorial", () => LoadSceneByIndex(6));
        navButtonMap.Add("Button_PlayGame", () => LoadSceneByIndex(4));
        navButtonMap.Add("Button_Credits", () => LoadSceneByIndex(1));
        navButtonMap.Add("Button_QuitGame", () => ExitGame());
        navButtonMap.Add("Button_MainMenu", () => LoadSceneByIndex(0));
        navButtonMap.Add("Button_StartTutorial", () => LoadSceneByIndex(3));
        navButtonMap.Add("Button_NextTutorial", () => UI_Intro.NextPage());
    }

    //Handle scene changed event
    private void ChangedActiveScene(Scene current, Scene next)    
    {
        //Find all navigation buttons
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("NavButton");

        //Set the click action for each button from the dictionary entry
        foreach(GameObject g in buttons)
        {
            UnityAction action = navButtonMap[g.name];
            Button button = g.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        //Update Game Music
        SetSceneMusic(next.name);

        if (next.name == "GameOver")
        {
            InitGameOverUI(GameManager.instance.GetScores());
        }
        else
        {
            //Update Game Manager
            GameManager.instance.OnLoadLevel();
        }
    }

    //Set scene music by scene name (and Victory status for Game Over screen)
    private void SetSceneMusic(string name)
    {
        if (name == "MainMenu")
        {
            CreateMusic().songIndex = 1;
        }
        else if (name == "GameOver")
        {
            if (GameManager.instance.isVictory)
            {
                CreateMusic().songIndex = 3;
            }
            else
            {
                CreateMusic().songIndex = 2;
            }
        }
        else if (name == "Credits")
        {
            CreateMusic().songIndex = 4;
        }
    }

    private MusicManager CreateMusic()
    {
        GameObject g = Instantiate(musicManagerPrefab, Vector3.zero, Quaternion.identity);

        return g.GetComponent<MusicManager>();
    }
}


