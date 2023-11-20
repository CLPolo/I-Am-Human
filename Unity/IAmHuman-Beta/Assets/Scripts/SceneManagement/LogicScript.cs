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

    public Player player;
    private AudioSource audioSource;
    public string currentScene => SceneManager.GetActiveScene().name;

    // TRAP
    public GameObject trappedText;
    public float defaultMashTimer = 1.5f;  // If you don't mash for 1 seconds you die
    private float mashTimer;
    public bool trapKills;
    public bool inGore = false;
    private float playerYStart = 1000f;


    // Start is called before the first frame update
    void Start()
    {
        mashTimer = defaultMashTimer;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
        DeathScreen.SetActive(false);  // Don't want to be dead at the start lol
        PauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.Instance;
        }
        if (playerYStart == 1000f)
        {
            playerYStart = player.transform.position.y;
        }
        // PUT ALL LOGIC HERE
        if (player.GetState() == PlayerState.Trapped)
        {   
            MashTrap();
        }

        if (Input.GetKeyDown(Controls.Pause))  // Pauses game when player hits esc
        {
            TogglePause();
        }
        CheckCutscenes();
    }

    public void MashTrap()
    {
        if (trappedText != null)
        {
            trappedText.SetActive(true);
        }
        mashTimer -= Time.deltaTime;
        if (inGore)
        {
            TilemapCollider2D tilemapCollider = GameObject.Find("Tilemap").GetComponent<TilemapCollider2D>();
            tilemapCollider.enabled = false;  // Disable the collider so the player can fall in the goop
            player.transform.position = new Vector3(player.transform.position.x,
                        player.transform.position.y - Time.deltaTime * (1.5f / 2.5f),
                        player.transform.position.z);
        }
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
            if (inGore)
            {
                TilemapCollider2D tilemapCollider = GameObject.Find("Tilemap").GetComponent<TilemapCollider2D>();
                tilemapCollider.enabled = true;  // Enable the collider so the player no fall in goop
            }
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
                    mashTimer += 3;  // This allows player to insta escape if in gore
                    player.transform.position = new Vector3(player.transform.position.x,
                        playerYStart, player.transform.position.z);
                }
                else
                {
                    mashTimer += 0.3f;  // Add 0.3 seconds to the timer
                }
            }
        }
    }

    public void Death(bool val = true)
    {   
        // Activates death screen
        IsPaused = val;  // will prevent player from moving after death
        DeathScreen.SetActive(val);
        //audioSource.PlayOneShot(audioSource.clip, 0.5f)
        //AudioListener.pause = IsPaused;
        player.SetState(PlayerState.Frozen);
        if (currentScene == "Bed room")
        {
            PlayerPrefs.SetInt("Crowbar", 0);  // if you die in the bedroom after grabbing the crowbar, it will respawn on load
        }
        PlayerPrefs.SetInt("Dead", 1);
    }

    // disable menu is used so certain methods of closing pause menu such as
    // clicking the continue button can pass 'false' since THEY will deal with
    // disabling the menu (i.e. need to play click sound before disabling but
    // click audio source is on button and cant be played if it gets disabled first
    public void TogglePause(bool disableMenu = true)
    {
        // Pauses game
        // Note: When we add enemy script, enemy movement should also be stopped if paused
        if (disableMenu)
        {
            PauseMenu.SetActive(!IsPaused);
        }
        IsPaused = !IsPaused;  // will prevent player from moving while paused
        if (IsPaused)
        {
            PlayerPrefs.SetInt("Paused", 1);
        }
        else { PlayerPrefs.SetInt("Paused", 0); }
        Time.timeScale = IsPaused?0:1F;

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
        LevelLoader.Instance.loadScene(0);
        TogglePause(false);
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
        }
    }
}
