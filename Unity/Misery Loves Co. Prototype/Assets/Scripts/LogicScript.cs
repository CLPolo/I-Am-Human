using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicScript : MonoBehaviour
{

    public GameObject DeathScreen;
    public GameObject PauseMenu;
    public bool IsPaused = false;  // true if game is paused
    public bool IsDead = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDead)  // gets IsDead value from player's movement script
        {
            Death();
        }
        if (Input.GetKeyDown(KeyCode.Escape))  // Pauses game when player hits esc
        {
            TogglePause();
        }
    }

    public void Death()
    {   
        // Activates death screen

        IsPaused = true;  // will prevent player from moving after death
        IsDead = false;
        DeathScreen.SetActive(true);
        Debug.Log("did it change in death?");

        AudioListener.pause = IsPaused;
    }
    public void TogglePause()
    {
        // Pauses game
        // Note: When we add enemy script, enemy movement should also be stopped if paused

        PauseMenu.SetActive(!IsPaused);
        IsPaused = !IsPaused;  // will prevent player from moving while paused
        Debug.Log("did it change in toggle pause?");

        AudioListener.pause = IsPaused;
    }

    public void RestartGame()
    {
        // Restarts the game by resetting scene
        Debug.Log("did it change in restart?");
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
