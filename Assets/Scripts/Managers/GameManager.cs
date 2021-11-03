// GameManager.cs
// Singleton to manage overall game state and global events
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager instance {get; private set;}

    public float respawnPlayerDelay = 1.0f;
    public float respawnKoboldDelay = 10.0f;
    public int startLives = 2;

    public GameObject playerPrefab;
    public GameObject koboldPrefab;

    [HideInInspector]
    public bool isVictory = false;

    private LevelManager levelManager;
    private int lives; 
    private int currentScore = 0;
    private int highScore;

    [HideInInspector]
    public PlayerController player {get; private set;}
    public KoboldController kobold {get; private set;}

    const float playerDepth = 10.0f;  //Z depth when spawning player
    const float koboldDepth = 10.0f;  //Z depth when spawning kobold

    private KoboldSpawner activeSpawner = null;
    private KoboldSpawner lastSpawner = null;

    private bool allowSpawnKobold = true;

    private bool isPaused = false;

    public (int, int, bool) GetScores()
    {
        return (currentScore, highScore, isVictory);
    }

    public void UpdateHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }
    }

    public void HealPlayer()
    {
        if (player != null)
        {
            player.HealFull();
        }
    }

    public void QuitLevel()
    {
        //Unpause and unmute in case paused when quit
        Time.timeScale = 1.0f;
        SoundManager.instance.SetMute(false);
        UI_Manager.instance.LoadSceneByIndex(0);
    }

    //Spawn Kobold and return true, unless in penalty time for defeated Kobold
    public bool CanSpawnKobold(KoboldSpawner spawner)
    {
        if (!allowSpawnKobold)
        {
            return false;
        }

        //Deactivate prior spawner
        if (activeSpawner != null) activeSpawner.SetActive(false);        
        activeSpawner = spawner;

        //Don't retrigger the same spawner
        if ((lastSpawner != spawner) || (kobold = null))
        {
            if (kobold == null)
            {
                //Spawn new
                SpawnKobold(spawner.transform.position);
            }
            else
            {
                //Teleport existing Kobold to spawn point
                kobold.transform.position = spawner.transform.position;
                SoundManager.instance.Play(SoundManager.SoundId.SpawnKobold);
            }

            lastSpawner = spawner;
        }

        return true;
    }

    public void AddBonusLife()
    {
        lives++;
        UI_Manager.instance.SetLivesCount(lives);
    }

    public void OnGameProgress(int type, int index)
    {
        levelManager.OnGameProgress(type, index);
    }

    private IEnumerator RespawnPlayer()
    {
        //Wait for short respawn delay
        yield return new WaitForSeconds(respawnPlayerDelay);
        levelManager.RespawnPlayer();
    }

    private IEnumerator RespawnKobold()
    {
        //Disable Kobold spawning for penalty delay
        allowSpawnKobold = false;
        yield return new WaitForSeconds(respawnKoboldDelay);
        if (activeSpawner != null)
        {
            SpawnKobold(activeSpawner.transform.position);
        }
        allowSpawnKobold = true;
    }

    public void OnPlayerDeath()
    {
        if (lives > 0)
        {
            lives--;
            UI_Manager.instance.SetLivesCount(lives);
            StartCoroutine(RespawnPlayer());
        }
        else
        {
            //Out of lives, load game over screen
            isVictory = false;
            SoundManager.instance.SetMute(false);
            UI_Manager.instance.LoadSceneByIndex(2);
        }
    }

    public void OnKoboldDeath()
    {
        //Set progress trigger for killing Kobold
        levelManager.OnGameProgress(0, 0);

        //Deactivate current spawner
        if (activeSpawner != null) activeSpawner.SetActive(false);        
        activeSpawner = null;
        lastSpawner = null;

        //Start penalty coroutine
        StartCoroutine(RespawnKobold());
    }

    public void OnNewGame()
    {
        currentScore = 0;
        lives = startLives;
    }

    //Called at start of level
    public void OnLoadLevel()
    {
        //Find Level Manager
        GameObject lm = GameObject.FindGameObjectWithTag("LevelManager");

        if (lm != null)
        {
            //Init Level Manager
            levelManager = (lm != null) ? lm.GetComponent<LevelManager>() : null;

            UI_Manager.instance.SetLivesCount(lives);
            UI_Manager.instance.SetScore(currentScore);
        }
    }

    public void IncreaseScore(int score)
    {
        currentScore += score;
        UI_Manager.instance.SetScore(currentScore);
    }

    public void SetHealth(float current, float max)
    {
        float level = (float)current / (float)max;
        UI_Manager.instance.SetHealthLevel(level);
    }

    public void ClearCheckpoints()
    {
        levelManager.ClearCheckpoints();
    }

    public void SpawnPlayer(Vector2 spawnPoint, float damageMultiplier, float damageResistance)
    {
        GameObject prefab = Instantiate(playerPrefab, new Vector3(spawnPoint.x, spawnPoint.y, playerDepth), Quaternion.identity);
        player = prefab.GetComponent<PlayerController>();
        player.name = "Player1";
        player.Initialize(damageMultiplier, damageResistance);
        SoundManager.instance.SetMute(false);
        SoundManager.instance.Play(SoundManager.SoundId.SpawnPlayer);
    }

    public void SpawnKobold(Vector2 spawnPoint)
    {
        GameObject prefab = Instantiate(koboldPrefab, new Vector3(spawnPoint.x, spawnPoint.y, koboldDepth), Quaternion.identity);
        kobold = prefab.GetComponent<KoboldController>();
        kobold.name = "Enemy1";
        kobold.Initialize(1.0f, 1.0f);
        SoundManager.instance.Play(SoundManager.SoundId.SpawnKobold);
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

    void Update()
    {
        if (levelManager != null)
        {
            //Handle 'pause' button during level
            if (Input.GetKeyDown("space"))
            {
                isPaused = !isPaused;

                Time.timeScale = (isPaused) ? 0.0f : 1.0f;
                
                if (player)
                {
                    SoundManager.instance.SetMute(isPaused);
                }
            }
        }
    }

    private void Initialize()
    {
        highScore = 0;
    }
}
