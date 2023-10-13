using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : AnimatedEntity
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
    private string currentAnimation;

    [Header("State")]
    public bool IsPushing = false;
    public bool isRunning = false;
    public bool isHiding = false;
    private bool wiggleRight = true;
    public bool hasFlashlight = false;

    [Header("Other Objects")]
    public GameObject Logic;
    private LogicScript logicScript;
    private Light lightCone;

    private int keyCount = 0;

    void Start()
    {
        logicScript = Logic.GetComponent<LogicScript>();
        rb = GetComponent<Rigidbody2D>();
        lightCone = flashlight.GetComponent<Light>();
        AnimationSetup();
    }

    // Update is called once per frame
    void Update()
    {
        checkMovement();
        checkAudio();
        CheckPushing();
        CheckRunning();
        CheckTrapped();
        checkFlashlight();
        AnimationUpdate();
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

    void CheckTrapped()
    {
        if (logicScript.isTrapped)
        {
            speed = 0;  // Player cannot move while trapped
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
        } else if (!(isRunning || isWalking || isHiding)) {
            transform.eulerAngles = new Vector3(0, 0, 0);  // Reset rotation
            speed = defaultSpeed;  // let the man walk
        }
    }

    void checkAudio()
    {
        if (AudioSource != null)
        {
            if (!logicScript.IsPaused)
            {
                playFootfall();
            }
            else
            {
                AudioSource?.Stop();
            }
        }
    }   

    void playFootfall()
    {
        if(AudioSource != null && !AudioSource.isPlaying)
        {
            if(isWalking) 
            {
                 if (Input.GetKey(KeyCode.LeftControl) && isRunning)
                {
                    AudioSource.PlayOneShot(footstepsRun[UnityEngine.Random.Range(0, footstepsWalk.Capacity)]);
                } else {
                    AudioSource.PlayOneShot(footstepsWalk[UnityEngine.Random.Range(0, footstepsWalk.Capacity)]);

                }

           }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bad" && !isHiding)
        {
            // This is when the monster sees you and you are not behind the box
            // Gameover can go here! For now I just freeze them
            speed = 0f;
            //BadGuy.speed = 0f;
            logicScript.Death();
        }
        else if (collision.gameObject.tag == "TrapArmed")
        {
            logicScript.isTrapped = true;
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
        ResetAnimationCycle();
    }

    private void CheckHiding(GameObject Hideable)
    {
        if (Input.GetKey(KeyCode.Space))
        {
            speed = sneakSpeed;
            isHiding = true;    
            var hideablePosition = Hideable.transform.position;
            Hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, 1);
            if (currentAnimation != "hiding")
            {
                InterruptAnimation(hiding, true);
            }
            transform.position += Vector3.right * 0.0001f;
            currentAnimation = "hiding";
        }
        else if (isHiding)  // unhides only if you were hiding
        {
            Unhide(Hideable);
            ResetAnimationCycle();
            currentAnimation = "default";
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
            if (currentAnimation != "pushing")
            {
                InterruptAnimation(PushingCycle, true);
            }
            currentAnimation = "pushing";
        }
        else if (!isHiding)  // can't push if hiding, but also won't switch speed back to default while player is hiding
        {
            speed = defaultSpeed;
            ResetAnimationCycle();
            currentAnimation = "default";
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