﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float sneakSpeed = 0.8f;
    
    [Header("Animation")]
    public List<Sprite> walkCycle;  
    public List<Sprite> runCycle;
    public List<Sprite> hiding;

    [Header("Sound")]
    public AudioSource AudioSource;
    public List<AudioClip> footstepsWalk;
    public List<AudioClip> footstepsRun;


    //boolean state based variables
    //private bool isHiding = false;
    private bool isWalking = false;
    private bool isRunning = false;

    public LogicScript logicScript;
    public GameObject Logic;

    void Start()
    {
        logicScript = Logic.GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        checkMovement();
        checkAudio();


    }
    void checkMovement()
    {   


        // Move left
        if (Input.GetKey(KeyCode.A) && !logicScript.IsPaused)
        {
            transform.position += Vector3.left * Time.deltaTime * speed;
            isWalking = true;
            
        }

        // Move right
        if (Input.GetKey(KeyCode.D) && !logicScript.IsPaused)
        {
            transform.position += Vector3.right * Time.deltaTime * speed;
            isWalking = true;
            
        }
        if((!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) || logicScript.IsPaused)
        {
            isWalking = false;
        }
    }
    void checkAudio()
    {
        if(!logicScript.IsPaused)
        {
            playFootfall();
        } else 
        {
            AudioSource.Stop();
        }
    }   

    void playFootfall()
    {
        if(!AudioSource.isPlaying)
        {
            if(isWalking) 
            {
                AudioSource.PlayOneShot(footstepsWalk[Random.Range(0, footstepsWalk.Capacity)]);
            }
    }       if(isRunning){}
    }

}