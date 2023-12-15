using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LogicScript : MonoBehaviour
{
    public GameObject DeathScreen;
    public GameObject PauseMenu;

    private static LogicScript _instance;
    public static LogicScript Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject LM = Instantiate(Resources.Load("LogicManager", typeof(GameObject))) as GameObject;
                DontDestroyOnLoad(LM);
                _instance = LM.GetComponent<LogicScript>();
            }
            return _instance;
        }
    }

    public bool IsPaused = false;  // true if game is paused
    // used to prevent escape for unpausing when button logic is still being done
    public bool allowPauseKey = true;

    public Player player;

    [Header("Cutscenes")]
    private AudioSource audioSource;
    private AudioClip monsterEmergesAudio;
    public string currentScene => SceneManager.GetActiveScene().name;
    public int currentSceneIndex => SceneManager.GetActiveScene().buildIndex;
    private GameObject monster;

    [Header("Traps")]
    public GameObject trappedText;
    public float defaultMashTimer = 1f;  // If you don't mash for 1 seconds you die
    private float mashTimer;
    public bool trapKills;
    public bool inGore = false;
    public bool doorBoards = false;
    private float playerYStart = 1000f;


    // Start is called before the first frame update
    void Start()
    {
        monsterEmergesAudio = (AudioClip)Resources.Load("Sounds/SoundEffects/Entity/Monster/monster-emerges");
        mashTimer = defaultMashTimer;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
        DeathScreen.SetActive(false);  // Don't want to be dead at the start lol
        PauseMenu.SetActive(false);
        PlayerPrefs.DeleteAll();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentScene == "Forest Chase" && monster == null)
        {
            monster = GameObject.Find("Monster");
            monster.SetActive(false);
            PlayerPrefs.SetInt("MonsterEmerges", 0);
        }
        if (player == null)
        {
            player = Player.Instance;
        } else {
            if (playerYStart == 1000f)
            {
                playerYStart = player.transform.position.y;
            }
            // PUT ALL LOGIC HERE
            if (player.GetState() == PlayerState.Trapped)
            {
                MashTrap();
            }

            if (Input.GetKeyDown(Controls.Pause) && currentSceneIndex > 0 && allowPauseKey)  // Pauses game when player hits esc
            {
                ResetPauseMenu();
                TogglePause();
            }
            CheckCutscenes();
        }
    }

    public void MashTrap()
    {
        if (trappedText != null)
        {
            trappedText.SetActive(true);
        }
        mashTimer -= Time.deltaTime;
        if (mashTimer <= 0 && trapKills)
        {
            // If the player does not mash fast enough they die :(
            Death();
        }
        else if (mashTimer <= 0 && !trapKills)
        {
            // This is for less punishing traps, the trap doesn't kill
            mashTimer = 1;
        }
        else if (mashTimer >= 3)
        {
            // The player escapes!
            mashTimer = defaultMashTimer;
            player.SetState(PlayerState.Idle);
            PlayerPrefs.SetInt("escaped", 1);  // Miriam uses this for the kitchen door trapped interaction
            if (trappedText != null)
            {
                trappedText.SetActive(false);
            }
        }
        else
        {
            if (Input.GetKeyDown(Controls.Mash))
            {

                if (inGore)
                {
                    mashTimer += 1.5f;  // This allows player to escape faster in gore
                }
                else if (doorBoards)
                {
                    mashTimer += 0.4f;  // The door boards are the hardest thing to pull
                }
                else
                {
                    mashTimer += 0.7f;  // Add 0.7 seconds to the timer
                }
            }
        }
    }

    public void Death(bool val = true)
    {   
        //Debug.Log("Called w/ val == " + val);
        // Activates death screen
        IsPaused = val;  // will prevent player from moving after death

        // Miriam commented below out entirely. the reson for audio call is beside it, reason for deathscreen is that it is now being activated from a function (ActivateDeathScreen) that's called by levelloader
        // I left it there in case we find major issues with my implementation.
        //
        //if (val)  
        //{
        //    //DeathScreen.SetActive(val);
        //    //AudioManager.Instance.StopAllSources();   // this call wasn't letting the rest of the function be called, and without it the audio all stops anyways...?
        //}

        //audioSource.PlayOneShot(audioSource.clip, 0.5f)
        //AudioListener.pause = IsPaused;
        player.SetState(PlayerState.Frozen);
        if (currentScene == "Bed room")
        {
            PlayerPrefs.SetInt("Crowbar", 0);  // if you die in the bedroom after grabbing the crowbar, it will respawn on load
        }
        PlayerPrefs.SetInt("Dead", 1);
        PlayerPrefs.SetInt("MonsterEmerges", 0);
    }

    public void ResetPauseMenu()
    {
        foreach (Transform child in PauseMenu.transform)
        {
            child.gameObject.SetActive(child.name == "Pause");
        }
    }

    // disableMenu is used so certain methods of closing pause menu such as
    // clicking the continue button can pass 'false' since THEY will deal with
    // disabling the menu (i.e. need to play click sound before disabling)
    public void TogglePause(bool disableMenu = true)
    {
        // Pauses game
        // Note: When we add enemy script, enemy movement should also be stopped if paused
        
        IsPaused = !IsPaused;  // will prevent player from moving while paused
        if (disableMenu)
        {
            PauseMenu.SetActive(IsPaused);
        }
        PlayerPrefs.SetInt("Paused", IsPaused ? 1 : 0);
        Time.timeScale = IsPaused ? 0 : 1;

        // DONT PAUSE AUDIO LISTENER WHEN PAUSING GAME,
        // NEED TO PROGRAMATICALLY GO THROUGH PLAYING AUDIO SOURCES AND PAUSE THEM INSTEAD
        // AudioListener.pause = IsPaused;
    }

    public void RestartGame()
    {
        // Restarts the game by resetting scene
        LevelLoader.Instance.loadScene(currentScene);
        mashTimer = defaultMashTimer;
        System.Threading.Thread.Sleep(100);
        Death(false);
    }

    public void ReturnToMainMenu()
    {
        PlayerPrefs.SetString("StartScene", currentScene);
        TogglePause(false);
        LevelLoader.Instance.loadScene("Title Screen");
    }

    public void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll(); // might mess w/ box stuff
    }

    public void CheckCutscenes()
    {
        if (currentScene == "Forest Chase")
        {
            if (player.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("StartChaseAnimation"))
            {
                player.SetState(PlayerState.Frozen);  // Freeze when animation playing
                PlayerPrefs.SetInt("InAnimation", 1);
            }
            else if (player.GetState() == PlayerState.Frozen && PlayerPrefs.GetInt("InAnimation") == 1)
            {
                player.gameObject.GetComponent<Animator>().enabled = false;  // Disable the animator to allow moving
                PlayerPrefs.SetInt("InAnimation", 0);
                player.SetState(PlayerState.Idle);
            }

            if (player.monsterEmergesCutscene == true && PlayerPrefs.GetInt("MonsterEmerges") == 2)
            {
                // We can continue the animation here
                monster.SetActive(true);  // Monster appears at the doorway
                playMonsterRoar();
                // START CHASE MUSIC HERE
                player.monsterEmergesCutscene = false;  // The cutscene is over
                player.SetState(PlayerState.Idle);  // THE PLAYER SHOULD RUN NOW
            }
        }


        if (currentScene == "Forest Intro")
        {
            if (PlayerPrefs.GetInt("Fading") == 2)
            {
                player.gameObject.GetComponent<Animator>().SetBool("FadeDone", true);
                player.SetState(PlayerState.Frozen);
            }
            if (player.gameObject.GetComponent<Animator>().GetBool("FadeDone") && player.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Woken"))
            {
                player.gameObject.GetComponent<Animator>().SetBool("FadeDone", false);  // this reset and check prevents constant playerstate updating (only updates to idle once)
                player.gameObject.GetComponent<Animator>().enabled = false;
                player.SetState(PlayerState.Idle);
                PlayerPrefs.SetInt("Fading", 0);
                
            }
        }

    }

    public void ActivateDeathScreen()  // allows the death screen to be activated AFTER the death anim on canvas (called from levelloader)
    {
        DeathScreen.SetActive(true);
    }

    public void playMonsterRoar()
    {
        audioSource.PlayOneShot(monsterEmergesAudio, 0.25f);
    }
}
