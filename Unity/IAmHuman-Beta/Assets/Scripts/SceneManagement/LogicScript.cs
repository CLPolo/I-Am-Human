using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    // TRAP
    public GameObject trappedText;
    public float defaultMashTimer = 1.5f;  // If you don't mash for 1 seconds you die
    private float mashTimer;
    public bool trapKills;


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
            // PUT ALL LOGIC HERE
            if (player.GetState() == PlayerState.Trapped)
            {   
                MashTrap();
            }

        if (Input.GetKeyDown(Controls.Pause))  // Pauses game when player hits esc
        {
            TogglePause();
        }
    }

    public void MashTrap()
    {
        if(trappedText != null)
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
            if (trappedText != null)
            {
                trappedText.SetActive(false);
            }
        }
        else
        {
            if (Input.GetKeyDown(Controls.Mash))
            {
                if (!audioSource.isPlaying){
                    //audioSource.clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/mud-trap-struggle-" + UnityEngine.Random.Range(0,5).ToString());
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/mud-trap-struggle-" + UnityEngine.Random.Range(0,5).ToString()), 0.5f);
                }
                mashTimer += 0.3f;  // Add 0.2 seconds to the timer
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
    }
    public void TogglePause()
    {
        // Pauses game
        // Note: When we add enemy script, enemy movement should also be stopped if paused
        PauseMenu.SetActive(!IsPaused);
        IsPaused = !IsPaused;  // will prevent player from moving while paused
        Time.timeScale = IsPaused?0:1F;

        AudioListener.pause = IsPaused;
    }

    public void RestartGame()
    {
        // Restarts the game by resetting scene
        LevelLoader.Instance.loadScene(SceneManager.GetActiveScene().name);
        mashTimer = 1.5f;
        System.Threading.Thread.Sleep(100);
        Death(false);
    }

    public void ReturnToMainMenu()
    {
        LevelLoader.Instance.loadScene(0);
        TogglePause();
    }
}
