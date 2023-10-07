using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float sneakSpeed = 0.8f;
    public const float pushSpeed = 2.5f;
    public const float defaultSpeed = 5f;

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
    //public AudioSource AudioSource;
    public List<AudioClip> footstepsWalk;
    public List<AudioClip> footstepsRun;


    //boolean state based variables
    //private bool isHiding = false;
    private bool facingRight = true;
    private bool movingRight = true;
    public bool IsPushing = false;
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
        //checkAudio();
        CheckPushing();
    }
    void checkMovement()
    {   
        // Move left
        if (Input.GetKey(KeyCode.A) && !logicScript.IsPaused)
        {
            transform.position += Vector3.left * Time.deltaTime * speed;
            isWalking = true;
            movingRight = false;
            CheckFlip();
            
        }

        // Move right
        if (Input.GetKey(KeyCode.D) && !logicScript.IsPaused)
        {
            transform.position += Vector3.right * Time.deltaTime * speed;
            isWalking = true;
            movingRight = true;
            CheckFlip();
            
        }
        if((!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) || logicScript.IsPaused)
        {
            isWalking = false;
        }
    }

    //void checkAudio()
    //{
    //    if(!logicScript.IsPaused)
    //    {
    //        playFootfall();
    //    } else 
    //    {
    //        AudioSource.Stop();
    //    }
    //}   

    //void playFootfall()
    //{
    //    if(!AudioSource.isPlaying)
    //    {
    //        if(isWalking) 
    //        {
    //            AudioSource.PlayOneShot(footstepsWalk[Random.Range(0, footstepsWalk.Capacity)]);
    //        }
    //}       if(isRunning){}
    //}

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
        else
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

}