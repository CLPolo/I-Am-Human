﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float sneakSpeed = 0.8f;
    public const float pushSpeed = 2.5f;
    public const float defaultSpeed = 5f;
    public const float runSpeed = 9f;
    private Rigidbody2D rb; // ------------------> RigidBody2D. Will be used to access it's velocity property to check for movement.

    [Header("Animation Cycles")]
    public List<Sprite> walkCycle;  
    public List<Sprite> runCycle;
    public List<Sprite> hiding;
    public List<Sprite> PushingCycle;

    [Header("Animation Variables")]
    public Sprite DefaultSprite;
    public SpriteRenderer SpriteRenderer;
    // Notably, the three below variables (as well as the corresponding function AnimationUpdate() below) are ripped from lab 4, I was mostly using for testing purposes
    private float animationTimer;  // current number of seconds since last animation frame update
    private float animationTimerMax = 1.0f / 12f;  // max number of seconds for each frame, defined by Framerate
    private int AnimationIndex = 0;  // current index in the DefaultAnimationCycle

    [Header("Sound")]
    public AudioSource AudioSource;
    public List<AudioClip> footstepsWalk;
    public List<AudioClip> footstepsRun;

    [Header("Items")]
    public GameObject flashlight;


    //boolean state based variables
    //private bool isHiding = false;
    private bool facingRight = true;
    private bool movingRight = true;
    private bool isWalking = false;

    [Header("State")]
    public bool IsPushing = false;
    public bool isRunning = false;
    public bool isHiding = false;
    public bool hasFlashlight = false;

    private GameObject Logic;
    private LogicScript logicScript;
    private GameObject self;
    private GameObject lightSource;
    private Light lightCone;

    void Start()
    {   
        self = GameObject.Find("Player");
        Logic = GameObject.Find("Logic Manager");
        logicScript = Logic.GetComponent<LogicScript>();
        rb = self.GetComponent<Rigidbody2D>();
        
        flashlight = GameObject.Find("Flash Light");
        lightCone = self.GetComponentInChildren<Light>();
        lightSource = GameObject.Find("Flashlight Light");
    }

    // Update is called once per frame
    void Update()
    {

        checkMovement();
        checkAudio();
        CheckPushing();
        CheckRunning();
        checkFlashlight();
        //print(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    void checkMovement()
    {   
        // Move left
        if (Input.GetKey(KeyCode.A) && !logicScript.IsPaused)
        {
            rb.velocity = Vector2.left * speed;
            isWalking = true;
            movingRight = false;
            CheckFlip();
        }

        // Move right
        if (Input.GetKey(KeyCode.D) && !logicScript.IsPaused)
        {
            rb.velocity = Vector2.right * speed;
            isWalking = true;
            movingRight = true;
            CheckFlip();
        }
        if((!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) || logicScript.IsPaused)
        {
            isWalking = false;
            rb.velocity *= Vector2.zero;
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
            AudioSource.PlayOneShot(footstepsWalk[UnityEngine.Random.Range(0, footstepsWalk.Capacity)]);
            }
                   
            if(isRunning){}
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var Hideable = collision.gameObject;
        if (collision.gameObject.tag == "Hideable" || collision.gameObject.tag == "Box")
        {
            CheckHiding(Hideable);
        }
        if (collision.gameObject.tag == "Bad" && !isHiding)
        {
            // This is when the monster sees you and you are not behind the box
            // Gameover can go here! For now I just freeze them
            speed = 0f;
            //BadGuy.speed = 0f;
            logicScript.Death();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Hideable" || collision.gameObject.tag == "Box")
        {
            var hideable = collision.gameObject;
            if (isHiding)
            {
                // remove fog
                Unhide(hideable);
            }
        }
    }

    private void Unhide(GameObject Hideable)
    {
        speed = defaultSpeed;
        var hideablePosition = Hideable.transform.position;
        Hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, -1);
        isHiding = false;
        transform.position += Vector3.left * 0.0001f;
        SpriteRenderer.sprite = DefaultSprite;
    }

    private void CheckHiding(GameObject Hideable)
    {
        if (Input.GetKey(KeyCode.Space))
        {
            speed = sneakSpeed;
            isHiding = true;    
            var hideablePosition = Hideable.transform.position;
            Hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, 1);
            AnimationUpdate(hiding);
            transform.position += Vector3.right * 0.0001f;
        }
        else if (isHiding)  // unhides only if you were hiding
        {
            Unhide(Hideable);
        }
    }

    private void CheckFlip()
    {
        // Checks if player needs to be flipped (i.e. if the player is not facing the direction they are moving)

        if (!IsPushing) // if not pushing and facing right, flips
        {
            if (facingRight != movingRight) // if facing right and moving left OR facing left and moving right, flips
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        // Flips the character

        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingRight = !facingRight;  // reflects which way player is currently facing
    }

    private void CheckPushing()
    {
        // Checks if player is pushing objects and adjusts speed and animations / sprites

        if (IsPushing)
        {
            speed = pushSpeed;
            AnimationUpdate(PushingCycle);
        }
        else if (!isHiding)  // can't push if hiding, but also won't switch speed back to default while player is hiding
        {
            speed = defaultSpeed;
            SpriteRenderer.sprite = DefaultSprite;
        }
    }

    protected void AnimationUpdate(List<Sprite> AnimationCycle)
    {
        // cycles through / 'plays' given sprites in AnimationCycle
        // Ripped from lab 4, mostly for testing purposes for a pushing animation

        animationTimer += Time.deltaTime;

        if (animationTimer > animationTimerMax)
        {
            animationTimer = 0;
            AnimationIndex++;

            if (AnimationCycle.Count == 0 || AnimationIndex >= AnimationCycle.Count)
            {
                AnimationIndex = 0;
            }
            if (AnimationCycle.Count > 0)
            {
                SpriteRenderer.sprite = AnimationCycle[AnimationIndex];
            }
        }
    }

    private void CheckRunning()
    {
        // Checks if player is running, and makes them run

        if (!IsPushing && !isHiding)  // First checks if player is pushing an object or hiding, in which case they can't run
        {
            if (isRunning && Input.GetKey(KeyCode.LeftControl))  // if they're pressing run button and should be running (isRunning)
            {
                speed = runSpeed;  // updates speed to run speed
            }
            else
            {
                speed = defaultSpeed;  // resets speed to default
            }
        }
    }

    private void checkFlashlight()
    {

        //turn light on and off
        if (hasFlashlight && Input.GetKeyDown(KeyCode.F))
        {

            if(lightCone.range > 0) lightCone.range = 0;
            else{lightCone.range = 50;}

        }
        float angle;
        //update rotation of flashlight
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - lightSource.transform.position;
        
        if(facingRight)
        {
            angle = Vector2.SignedAngle(dir, Vector2.right);
            lightSource.transform.eulerAngles = new Vector3(angle, 90, 0);
        } else {
            angle = Vector2.SignedAngle(Vector2.left, dir);
            lightSource.transform.eulerAngles = new Vector3(angle, -90, 0);
        }
    }
}