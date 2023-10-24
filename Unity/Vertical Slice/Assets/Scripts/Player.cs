﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum PlayerState
{
    Idle = 0,
    Walking = 1,
    Running = 2,
    Hiding = 3,
    Trapped = 4,
    Pushing = 5,
}


public class Player : AnimatedEntity
{
    private static Player _instance;
    public static Player Instance { get { return _instance; } }

    [Header("Movement")]
    public float speed = 5f;
    public const float sneakSpeed = 0.8f;
    public const float pushSpeed = 2.5f;
    public const float walkSpeed = 5f;
    public const float runSpeed = 9f;
    private Rigidbody2D rb; // ------------------> RigidBody2D. Will be used to access it's velocity property to check for movement.

    [Header("Animation Cycles")]
    public List<Sprite> walkCycle;  
    public List<Sprite> runCycle;
    public List<Sprite> hideCycle;
    public List<Sprite> PushingCycle;

    [Header("Animation Variables")]
    public Sprite DefaultSprite;

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
    private string currentAnimation;

    [Header("State")]
    private PlayerState state;
    private bool wiggleRight = true;
    public bool hasFlashlight = false;

    [Header("Other Objects")]
    private LogicScript logic;
    private Light lightCone;

    private int keyCount = 0;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        } else {
            _instance = this;
        }

        logic = LogicScript.Instance;

        rb = GetComponent<Rigidbody2D>();
        lightCone = flashlight.GetComponent<Light>();
        AnimationSetup();
    }

    // Update is called once per frame
    void Update()
    {
        if (!logic.IsPaused)
        {
            CheckTrapped();
            checkMovement();
            playFootfall();
            checkFlashlight();
            AnimationUpdate();
        } else {
            AudioSource?.Stop();

        }
        //print(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    public PlayerState GetState()
    {
        return state;
    }

    public void SetState(PlayerState _state)
    {
        if (_state == state)
        {
            return;
        }

        bool allowStateChange = true;

        // reset state specific changes
        transform.eulerAngles = new Vector3(0, 0, 0);  // Reset rotation
        transform.position += Vector3.left * 0.0001f;
        speed = walkSpeed;
        currentAnimation = "default";
        ResetAnimationCycle();

        switch (_state)
        {
            case PlayerState.Idle:
                // set animation
                break;
            case PlayerState.Walking:
                break;
            case PlayerState.Hiding:
                speed = sneakSpeed;
                if (currentAnimation != "hiding")
                {
                    InterruptAnimation(hideCycle, true);
                }
                transform.position += Vector3.right * 0.0001f;
                currentAnimation = "hiding";
                break;
            case PlayerState.Running:
                speed = runSpeed;
                break;
            case PlayerState.Pushing:
                speed = pushSpeed;
                if (currentAnimation != "pushing")
                {
                    InterruptAnimation(PushingCycle, true);
                }
                currentAnimation = "pushing";
                break;
            case PlayerState.Trapped:
                speed = 0;  // Player cannot move while trapped
                rb.velocity = Vector2.zero;
                transform.eulerAngles = new Vector3(0, -135, 0); // indicate player is trapped somehow
                break;
        }

        if (allowStateChange)
        {
            state = _state;
        }
        Debug.Log(state);
    }

    void checkMovement()
    {   
        // Move left
        if (Input.GetKey(KeyCode.A))
        {
            if (!state.isOneOf(PlayerState.Hiding, PlayerState.Pushing, PlayerState.Trapped)) {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    SetState(PlayerState.Running);
                }
                else
                {
                    SetState(PlayerState.Walking);
                }
            }
            rb.velocity = Vector2.left * speed;
            movingRight = false;
            CheckFlip();
        }

        // Move right
        if (Input.GetKey(KeyCode.D))
        {
            if (!state.isOneOf(PlayerState.Hiding, PlayerState.Pushing, PlayerState.Trapped))
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    SetState(PlayerState.Running);
                } else {
                    SetState(PlayerState.Walking);
                }
            }
            rb.velocity = Vector2.right * speed;
            movingRight = true;
            CheckFlip();
        }

        if(!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
        {
            if (state.isOneOf(PlayerState.Walking, PlayerState.Running))
            {
                SetState(PlayerState.Idle);
            }
            rb.velocity = Vector2.zero;
        }
    }

    void CheckTrapped()
    {
        if (state == PlayerState.Trapped)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                wiggleRight = !wiggleRight;
                if (wiggleRight)
                {
                    transform.eulerAngles = new Vector3(0, -45, 0);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, -135, 0);
                }
            }
        }
    }

    void playFootfall()
    {
        if(AudioSource != null && !AudioSource.isPlaying)
        {
            if (state == PlayerState.Walking) 
            {
                AudioSource.PlayOneShot(footstepsRun[UnityEngine.Random.Range(0, footstepsWalk.Capacity)]);
            } else if (state == PlayerState.Running) {
                AudioSource.PlayOneShot(footstepsWalk[UnityEngine.Random.Range(0, footstepsWalk.Capacity)]);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bad" && state != PlayerState.Hiding)
        {
            // This is when the monster sees you and you are not behind the box
            // Gameover can go here! For now I just freeze them
            speed = 0f;
            //BadGuy.speed = 0f;
            logic.Death();
        }
        else if (collision.gameObject.tag == "TrapArmed")
        {
            SetState(PlayerState.Trapped);
            collision.gameObject.tag = "TrapDisarmed";
        }
        else if (collision.gameObject.tag == "Key")
        {
            Destroy(collision.gameObject);
            keyCount++;
        }
        else if (collision.gameObject.tag == "Door")
        {
            Door door = collision.gameObject.GetComponent<Door>();
            if (door.isLocked && keyCount > 0)
            {
                door.isLocked = false;
                keyCount--;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var Hideable = collision.gameObject;
        if (collision.gameObject.tag == "Hideable" || collision.gameObject.tag == "Box")
        {
            CheckHiding(Hideable);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Hideable" || collision.gameObject.tag == "Box")
        {
            var hideable = collision.gameObject;
            if (state == PlayerState.Hiding)
            {
                // remove fog
                Unhide(hideable);
            }
        }
    }

    private void Unhide(GameObject Hideable)
    {
        var hideablePosition = Hideable.transform.position;
        Hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, -1);
        SetState(PlayerState.Idle);
    }

    private void CheckHiding(GameObject Hideable)
    {
        if (Input.GetKey(KeyCode.Space))
        {
            SetState(PlayerState.Hiding);    
            var hideablePosition = Hideable.transform.position;
            Hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, 1);
        }
        else if (state == PlayerState.Hiding)  // unhides only if you were hiding
        {
            Unhide(Hideable);
            ResetAnimationCycle();
        }
    }

    private void CheckFlip()
    {
        // Checks if player needs to be flipped (i.e. if the player is not facing the direction they are moving)
        if (state != PlayerState.Pushing) // if not pushing and facing right, flips
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

    private void checkFlashlight()
    {
        //turn light on and off
        if (hasFlashlight && Input.GetKeyDown(KeyCode.F))
        {
            if (lightCone.range == 50) {
                lightCone.range = 0;
            } else {
                lightCone.range = 50;
            }
        }
        float angle;
        //update rotation of flashlight
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - flashlight.transform.position;
        
        if(facingRight)
        {
            angle = Vector2.SignedAngle(dir, Vector2.right);
            flashlight.transform.eulerAngles = new Vector3(angle, 90, 0);
        } else {
            angle = Vector2.SignedAngle(Vector2.left, dir);
            flashlight.transform.eulerAngles = new Vector3(angle, -90, 0);
        }
    }
}