﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicScript : MonoBehaviour
{

    public GameObject DeathScreen;
    public GameObject PauseMenu;
    
    public bool IsPaused = false;  // true if game is paused

    // TRAP
    public GameObject trappedText;
    public bool isTrapped = false;
    public float mashTimer = 1.5f;  // If you don't mash for 1 seconds you die


    // Start is called before the first frame update
    void Start()
    {
        if (DeathScreen != null)
        {
            DeathScreen?.SetActive(false);  // Don't want to be dead at the start lol
        }
        if (trappedText != null)
        {
            trappedText?.SetActive(false);
        }
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
        trappedText.SetActive(true);
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

    public void Death()
    {   
        // Activates death screen
        IsPaused = true;  // will prevent player from moving after death
        DeathScreen.SetActive(true);

        AudioListener.pause = IsPaused;
    }
    public void TogglePause()
    {
        // Pauses game
        // Note: When we add enemy script, enemy movement should also be stopped if paused
        PauseMenu.SetActive(IsPaused);
        IsPaused = !IsPaused;  // will prevent player from moving while paused

        AudioListener.pause = IsPaused;
    }

    public void RestartGame()
    {
        // Restarts the game by resetting scene
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
