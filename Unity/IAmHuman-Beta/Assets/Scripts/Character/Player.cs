using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class Controls
{
    // movement
    public static readonly KeyCode Left = KeyCode.A;
    public static readonly KeyCode Right = KeyCode.D;
    // actions
    public static readonly KeyCode Interact = KeyCode.E; // hiding is also interacting
    public static readonly KeyCode Hide = KeyCode.F; // hiding is also interacting
    public static readonly KeyCode Push = KeyCode.Space;
    public static readonly KeyCode Mash = KeyCode.Space;
    public static readonly List<KeyCode> Run = new List<KeyCode> { KeyCode.LeftShift, KeyCode.RightShift };
    // ui
    public static readonly KeyCode Pause = KeyCode.Escape;
}

public enum PlayerState
{
    Idle = 0,
    Walking = 1,
    Running = 2,
    Hiding = 3,
    Trapped = 4,
    Pushing = 5,
    Pulling = 6,
    Frozen = 7,
}


public class Player : AnimatedEntity
{
    private static Player _instance;
    public static Player Instance { get { return _instance; } }

    [Header("Movement")]
    private float speed = 5f;
    public const float sneakSpeed = 0.8f;
    public const float pushSpeed = 2.5f;
    public const float walkSpeed = 5f;
    public const float runSpeed = 9f;
    private Rigidbody2D rb; // ------------------> RigidBody2D. Will be used to access it's velocity property to check for movement.

    [Header("Animation Cycles")]
    private List<Sprite> currentCycle;
    public List<Sprite> walkCycle;  
    public List<Sprite> runCycle;
    public List<Sprite> hideCycle;
    public List<Sprite> pushCycle;
    public List<Sprite> pullCycle;
    [Header("Idle Sprites")]
    public Sprite idleWalk;
    public Sprite idlePush;
    public Sprite idleHide;

    [Header("Sound")]
    public AudioSource AudioSource;
    public List<AudioClip> footstepsWalk;
    public List<AudioClip> footstepsRun;
    public List<AudioClip> monsterFootsteps;
    protected internal bool touchingWall = false;

    [Header("Items")]
    public GameObject flashlight;

    //boolean state based variables
    //private bool isHiding = false;
    private bool moving = false;
    private bool facingRight = true;
    private bool movingRight = true;

    [Header("State")]
    private PlayerState state;
    public bool hasFlashlight = false;

    [Header("Other Objects")]
    private LogicScript logic;
    private Light lightCone;
    public bool monsterEmergesCutscene = false;
    public bool cameraCutscene = false;
    public bool finalCutscene = false;
    public bool shrinkCamera = false;
    private int keyCount = 0;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        } else {
            _instance = this;
        }

        for (int i = hideCycle.Count; i > 0; i--)
        {
            hideCycle.Insert(i, hideCycle.ElementAt(i-1));
        }

        logic = LogicScript.Instance;
        currentCycle = walkCycle;
        rb = GetComponent<Rigidbody2D>();
        lightCone = flashlight.GetComponent<Light>();
        AnimationSetup();
    }

    // Update is called once per frame
    void Update()
    {
        checkMovement();
        checkFlashlight();
        AnimationUpdate();
    }

    public PlayerState GetState()
    {
        return state;
    }

    public void SetFootfalls(string walking, string running){
        footstepsRun  = Resources.LoadAll<AudioClip>(running).ToList();
        footstepsWalk = Resources.LoadAll<AudioClip>(walking).ToList();
    }

    public void SetState(PlayerState _state)
    {
        if (_state == PlayerState.Pushing && movingRight != facingRight)
        {
            _state = PlayerState.Pulling;
        }

        if (moving && !touchingWall && !interruptFlag)
        {
            InterruptAnimation(currentCycle, true);
        } else if ((!moving || touchingWall) && interruptFlag) {
            ResetAnimationCycle();
        }
        if (_state == state || (state == PlayerState.Frozen && _state != PlayerState.Idle))
        {
            return;
        }
        
        bool allowStateChange = true;

        // reset state specific changes
        transform.position += Vector3.left * 0.0001f;
        speed = walkSpeed;
        GetComponent<BoxCollider2D>().offset = new Vector2(0, 0);
        ResetAnimationCycle();

        switch (_state)
        {
            case PlayerState.Idle:
                DefaultAnimationCycle[0] = idleWalk;
                break;
            case PlayerState.Walking:
                DefaultAnimationCycle[0] = idleWalk;
                currentCycle = walkCycle;
                break;
            case PlayerState.Hiding:
                speed = sneakSpeed;
                DefaultAnimationCycle[0] = idleHide;
                currentCycle = hideCycle;
                transform.position += Vector3.right * 0.0001f;
                break;
            case PlayerState.Running:
                speed = runSpeed;
                DefaultAnimationCycle[0] = idleWalk;
                currentCycle = runCycle;
                break;
            case PlayerState.Pushing:
                speed = pushSpeed;
                DefaultAnimationCycle[0] = idlePush;
                currentCycle = pushCycle;
                break;
            case PlayerState.Pulling:
                speed = pushSpeed;
                DefaultAnimationCycle[0] = idlePush;
                currentCycle = pullCycle;
                break;
            case PlayerState.Trapped:
                speed = 0;  // Player cannot move while trapped
                rb.velocity = Vector2.zero;
                // indicate player is trapped somehow:
                //transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y - 0.3f, transform.localScale.z);
                break;
            case PlayerState.Frozen:
                speed = 0;
                break;
        }

        if (allowStateChange)
        {
            state = _state;
        }
    }

    void checkMovement()
    {
        if (!Input.GetKey(Controls.Right) && !Input.GetKey(Controls.Left))
        {
            if (state.IsOneOf(PlayerState.Walking, PlayerState.Running))
            {
                SetState(PlayerState.Idle);
            }
            rb.velocity = Vector2.zero;
            moving = false;
        } else {
            if (Input.GetKey(Controls.Left) &&
                !(logic.currentScene == "Kitchen" && state.IsOneOf(PlayerState.Pushing, PlayerState.Pulling)))
            {
                // Jon note - I added some stuff to the if statement so the player cannot pull the body left in the Kitchen
                rb.velocity = Vector2.left * speed;
                movingRight = false;
            }
            else if (Input.GetKey(Controls.Right))
            {
                rb.velocity = Vector2.right * speed;
                movingRight = true;
            }
            if (!state.IsOneOf(PlayerState.Hiding, PlayerState.Pushing, PlayerState.Pulling, PlayerState.Trapped, PlayerState.Frozen))
            {
                if ((Input.GetKey(Controls.Run[0]) || Input.GetKey(Controls.Run[1])) && logic.currentScene.IsOneOf("Attic", "Forest Chase"))
                {
                    SetState(PlayerState.Running);
                }
                else
                {
                    SetState(PlayerState.Walking);
                }
            }
            moving = !state.IsOneOf(PlayerState.Trapped, PlayerState.Frozen);
            CheckFlip();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && state != PlayerState.Hiding)
        {
            // This is when the monster sees you and you are not behind the box
            logic.Death();
        }
        else if (collision.gameObject.CompareTag("Key"))
        {
            Destroy(collision.gameObject);
            keyCount++;
        }
        else if (collision.gameObject.CompareTag("Door"))
        {
            Door door = collision.gameObject.GetComponent<Door>();
            if (door.isLocked && keyCount > 0)
            {
                door.isLocked = false;
                keyCount--;
            }
        }
        else if (collision.gameObject.name == "MonsterEmerge")
        {
            // This is for the forest chase, it plays the monster coming out of the cabin cutscene
            SetState(PlayerState.Frozen);
            monsterEmergesCutscene = true;
            collision.enabled = false;  // Disable so it doesn't play again
        }
        else if (collision.gameObject.CompareTag("CrawlerSpawner"))
        {
            // I added these for fun lol we can always delete them
            collision.transform.GetChild(0).gameObject.SetActive(true);  // ACTIVATE THE CREATURE >:)
            collision.enabled = false;  // Disable so it doesn't play again
        }
        else if (collision.gameObject.name == "CrawlerRemovalService")
        {
            PlayerPrefs.SetInt("FinalDialogue", -1);  // Stop text from showing right at the start
            shrinkCamera = true;  // Start shrinking the camera for final cutscene
            for (int i=0; i<3; i++)
            {
                collision.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);  // DEACTIVATE THE CREATURES
            }
            
        }
        else if (collision.gameObject.name == "EndingCutscene")
        {
            // This is the final cutscene in the game.
            SetState(PlayerState.Frozen);  // So the player cannot move
            cameraCutscene = true;  // So we can control the camera
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var Hideable = collision.gameObject;
        if (collision.gameObject.CompareTag("Hideable") || collision.gameObject.CompareTag("Box"))
        {
            CheckHiding(Hideable);
        }
        else if (collision.gameObject.CompareTag("TrapArmed") && !state.IsOneOf(PlayerState.Pushing, PlayerState.Pulling))
        {
            logic.trapKills = true;
            if (collision.gameObject.name == "Gore Pile")
            {
                logic.inGore = true;
            }
            SetState(PlayerState.Trapped);
            collision.gameObject.tag = "TrapDisarmed";
            StartCoroutine(ResetTrap(collision));
        }
        else if (collision.gameObject.CompareTag("TrapArmedNoKill") && !state.IsOneOf(PlayerState.Pushing, PlayerState.Pulling))
        {
            logic.trapKills = false;
            SetState(PlayerState.Trapped);
            collision.gameObject.tag = "TrapDisarmed";
            StartCoroutine(ResetTrap(collision));
        }
        if ((collision.gameObject.CompareTag("TrapArmedNoKill") || 
            collision.gameObject.CompareTag("TrapArmed") ||
            collision.gameObject.CompareTag("TrapDisarmed")) && state != PlayerState.Trapped)
        {
            // Slow the player while they are walking over something goopy i.e. mud or gore
            if (state == PlayerState.Walking)
            {
                speed = walkSpeed - 2f;
            }
            else if (state == PlayerState.Running)
            {
                speed = runSpeed - 4f; // lol :)
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hideable") || collision.gameObject.CompareTag("Box"))
        {
            var hideable = collision.gameObject;
            if (state == PlayerState.Hiding)
            {
                // remove fog
                Unhide(hideable);
            }
        }
        if (collision.gameObject.CompareTag("TrapArmedNoKill") ||
            collision.gameObject.CompareTag("TrapArmed") ||
            collision.gameObject.CompareTag("TrapDisarmed"))
        {
            // Fix the walk speed after leaving trap
            if (state == PlayerState.Walking)
            {
                speed = walkSpeed;
            }
            else if (state == PlayerState.Running)
            {
                speed = runSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!LayerMask.LayerToName(collision.gameObject.layer).IsOneOf("Entity", "Interactable", "Floor")
            && !collision.gameObject.CompareTag("Box"))
        {
            touchingWall = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!LayerMask.LayerToName(collision.gameObject.layer).IsOneOf("Entity", "Interactable", "Floor")
            && !collision.gameObject.CompareTag("Box"))
        {
            touchingWall = false;
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
        if (Input.GetKey(Controls.Hide))
        {
            SetState(PlayerState.Hiding);    
            var hideablePosition = Hideable.transform.position;
            Hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, 1);
        }
        else if (state == PlayerState.Hiding)  // unhides only if you were hiding
        {
            Unhide(Hideable);
            // ResetAnimationCycle();
        }
    }

    private void CheckFlip()
    {
        // Checks if player needs to be flipped (i.e. if the player is not facing the direction they are moving)
        if (!state.IsOneOf(PlayerState.Pushing, PlayerState.Pulling, PlayerState.Frozen) && !logic.IsPaused) // if not pushing and facing right and not paused or frozen (timeScale is 1), flips
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
        if (hasFlashlight && Input.GetMouseButtonDown(0))
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

    IEnumerator ResetTrap(Collider2D collision)
    {
        // After (LESS THAN) 5 seconds the trap resets (i.e. player can fall back into mud)
        yield return new WaitUntil(() => state != PlayerState.Trapped);
        if (collision.gameObject.name == "Gore Pile")
        {
            yield return new WaitForSeconds(0.2f);  // Wait for 0.2 seconds to reset gore
            logic.inGore = false;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);  // Wait for 0.5 seconds to reset mud and other traps
        }
        if (logic.trapKills)
        {
            collision.gameObject.tag = "TrapArmed";
        }
        else
        {
            collision.gameObject.tag = "TrapArmedNoKill";
        }
    }
}