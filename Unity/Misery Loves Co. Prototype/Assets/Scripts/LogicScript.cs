using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicScript : MonoBehaviour
{

    public GameObject Player;
    public GameObject DeathScreen;
    public GameObject PauseMenu;
    public bool Is_Paused = false;  // true if game is paused

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.GetComponent<Movement>().IsDead)  // gets IsDead value from player's movement script
        {
            Death();
        }
        if (Input.GetKeyDown(KeyCode.Escape))  // Pauses game when player hits esc
        {
            Paused();
        }
    }

    public void Death()
    {   
        // Activates death screen

        Is_Paused = true;  // will prevent player from moving after death
        DeathScreen.SetActive(true);
    }
    public void Paused()
    {
        // Pauses game
        // Note: When we add enemy script, enemy movement should also be stopped if paused

        Is_Paused = true;  // will prevent player from moving while paused
        PauseMenu.SetActive(true);
    }

    public void Continue()
    {
        // Unpauses game
        PauseMenu.SetActive(false);
        Is_Paused = false;
    }

    public void RestartGame()
    {
        // Restarts the game by resetting scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
