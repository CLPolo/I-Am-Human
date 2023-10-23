using System;
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
    private string currentAnimation;

    [Header("State")]
    public PlayerState state;
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
        CheckTrapped();
        checkMovement();
        checkAudio();
        CheckPushing();
        CheckRunning();
        checkFlashlight();
        AnimationUpdate();
        //print(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    void checkMovement()
    {   
        // Move left
        if (Input.GetKey(KeyCode.A) && !logic.IsPaused)
        {
            rb.velocity = Vector2.left * speed;
            if (state != PlayerState.Hiding) {
                state = PlayerState.Walking;
            }
            movingRight = false;
            CheckFlip();
        }

        // Move right
        if (Input.GetKey(KeyCode.D) && !logic.IsPaused)
        {
            rb.velocity = Vector2.right * speed;
            if (state != PlayerState.Hiding)
            {
                state = PlayerState.Walking;
            }
            movingRight = true;
            CheckFlip();
        }
        if((!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) || logic.IsPaused)
        {
            if (state != PlayerState.Hiding)
            {
                state = PlayerState.Idle;
            }
            rb.velocity *= Vector2.zero;
        }
    }

    void CheckTrapped()
    {
        if (logic.isTrapped)
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
        } else if (state != PlayerState.Running && state != PlayerState.Walking && state != PlayerState.Hiding) {
            transform.eulerAngles = new Vector3(0, 0, 0);  // Reset rotation
            speed = defaultSpeed;  // let the man walk
        }
    }

    void checkAudio()
    {
        if (AudioSource != null)
        {
            if (!logic.IsPaused)
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
            if(state == PlayerState.Walking) 
            {
                 if (Input.GetKey(KeyCode.LeftControl) && state == PlayerState.Running)
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
            logic.isTrapped = true;
            state = PlayerState.Trapped;
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
        speed = defaultSpeed;
        var hideablePosition = Hideable.transform.position;
        Hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, -1);
        state = PlayerState.Idle;
        transform.position += Vector3.left * 0.0001f;
        ResetAnimationCycle();
    }

    private void CheckHiding(GameObject Hideable)
    {
        if (Input.GetKey(KeyCode.Space))
        {
            speed = sneakSpeed;
            state = PlayerState.Hiding;    
            var hideablePosition = Hideable.transform.position;
            Hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, 1);
            if (currentAnimation != "hiding")
            {
                InterruptAnimation(hiding, true);
            }
            transform.position += Vector3.right * 0.0001f;
            currentAnimation = "hiding";
        }
        else if (state == PlayerState.Hiding)  // unhides only if you were hiding
        {
            Unhide(Hideable);
            ResetAnimationCycle();
            currentAnimation = "default";
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

    private void CheckPushing()
    {
        // Checks if player is pushing objects and adjusts speed and animations / sprites

        if (state == PlayerState.Pushing)
        {
            speed = pushSpeed;
            if (currentAnimation != "pushing")
            {
                InterruptAnimation(PushingCycle, true);
            }
            currentAnimation = "pushing";
        }
        else if (state != PlayerState.Hiding && state != PlayerState.Walking && state != PlayerState.Idle)  // can't push if hiding, but also won't switch speed back to default while player is hiding
        {
            speed = defaultSpeed;
            ResetAnimationCycle();
            currentAnimation = "default";
        }
    }

    private void CheckRunning()
    {
        // Checks if player is running, and makes them run

        if (state != PlayerState.Pushing && state != PlayerState.Hiding)  // First checks if player is pushing an object or hiding, in which case they can't run
        {
            if (state == PlayerState.Running && Input.GetKey(KeyCode.LeftControl))  // if they're pressing run button and should be running (isRunning)
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