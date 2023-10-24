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

    // TRAP
    public GameObject trappedText;
    public bool isTrapped = false;
    public float mashTimer = 1.5f;  // If you don't mash for 1 seconds you die


    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log(1);
            Destroy(gameObject);
        }

        Debug.Log(2);
        Debug.Log(DeathScreen.name);
        DeathScreen.SetActive(false);  // Don't want to be dead at the start lol
        PauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsPaused)
        {
            // PUT ALL LOGIC HERE
            if (isTrapped)
            {
                MashTrap();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))  // Pauses game when player hits esc
        {
            TogglePause();
        }
    }

    public void MashTrap()
    {
        //trappedText.SetActive(true);
        mashTimer -= Time.deltaTime;
        if (mashTimer <= 0)
        {
            // If the player does not mash fast enough they die :(
            Death();
        }
        else if (mashTimer >= 3)
        {
            // The player escapes!
            isTrapped = false;
            trappedText.SetActive(false);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                mashTimer += 0.3f;  // Add 0.2 seconds to the timer
            }
        }
    }

    public void Death(bool val = true)
    {   
        // Activates death screen
        IsPaused = val;  // will prevent player from moving after death
        DeathScreen.SetActive(val);
        AudioListener.pause = IsPaused;
    }
    public void TogglePause()
    {
        // Pauses game
        // Note: When we add enemy script, enemy movement should also be stopped if paused
        PauseMenu.SetActive(!IsPaused);
        IsPaused = !IsPaused;  // will prevent player from moving while paused

        AudioListener.pause = IsPaused;
    }

    public void RestartGame()
    {
        // Restarts the game by resetting scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        isTrapped = false;
        mashTimer = 1.5f;
        System.Threading.Thread.Sleep(100);
        Death(false);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
        System.Threading.Thread.Sleep(100);
        TogglePause();
    }

}
