using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sister : NPC
{
    [Header("Movement")]
    public float speed = 3f;
    public float detectRange = -1;

    [Header("Animation Cycles")]
    public List<Sprite> walkCycle;

    [Header("Animation Variables")]
    public Sprite DefaultSprite;
    // Notably, the three below variables (as well as the corresponding function AnimationUpdate() below) are ripped from lab 4, I was mostly using for testing purposes

    [Header("Sound")]
    //public AudioSource audioSource;
    public List<AudioClip> footstepsWalk;
    public List<AudioClip> footstepsRun;

    [Header("Barks")]
    private float barkTimer;
    private float barkLimit;
    public float maxTimeBetweenBarks = 10;
    public float minTimeBetweenBarks = 50;
    public List<AudioClip> barks;
    private AudioSource sisterAudio;


    private Animator sisterAnimator;
    private float untilDist = 3f;

    private bool endCutscene = false;

    // Start is called before the first frame update
    void Start()
    {

        sisterAudio = GetComponent<AudioSource>();
        sisterAnimator = GetComponent<Animator>();

        BarkReset();  // Bark will happen randomly every 10-30 seconds
        if (detectRange >= 0)
        {
            SetupNPC(speed, 1f, 3f, detectRange);
        } else {
            SetupNPC(speed, 1f, 3f);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (player ==  null)
        {
            player = Player.Instance;
        }

        FollowPlayer();
        Bark();
        if (SceneManager.GetActiveScene().name == "Hallway Hub")
        {
            if (PlayerPrefs.HasKey("DoneSisterCinematic"))
            {
                this.gameObject.SetActive(false);
            } else {
                sisterAnimator.SetInteger("State", 2);
            }
        }
        else if (SceneManager.GetActiveScene().name == "Forest Intro (Lily)" && PlayerPrefs.GetInt("LilyStandDone") != 1)
        {
            if (PlayerPrefs.GetInt("LilyStandStart") != 1) sisterAnimator.SetInteger("State", 3);
            else if (sisterAnimator.GetCurrentAnimatorStateInfo(0).IsName("StoodUp"))
            {
                PlayerPrefs.SetInt("LilyStandDone", 1);
                SetDetectRange(5);
            }
        }
        else if (GetFollowing())
        {
            sisterAnimator.SetInteger("State", 1);
        }
        else
        {
            sisterAnimator.SetInteger("State", 0);
        }

        if (SceneManager.GetActiveScene().name == "Hallway Hub" && sisterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !sisterAudio.isPlaying)
        {
            sisterAudio.PlayOneShot((AudioClip)Resources.Load("Sounds/SoundEffects/Environment/Cabin/Misc/scare-lily-away"), 0.25f);
            player.SetState(PlayerState.Frozen);
        }

        if (sisterAnimator.GetCurrentAnimatorStateInfo(0).IsName("LilyEndCutscene") && !endCutscene)
        {
            player.SetState(PlayerState.Idle);
            endCutscene = true;
            if (PlayerPrefs.GetInt("LilyHallwayOver") != 2) PlayerPrefs.SetInt("LilyHallwayOver", 1); // for text purposes, messy sorry
        }

        LilyFootfall();
    }

    private void Bark()
    {
        barkTimer += Time.deltaTime;
        if (barkTimer > barkLimit && sisterAudio != null)
        {
            sisterAudio.PlayOneShot(sisterAudio.clip);
            BarkReset();
        }
    }

    private void BarkReset()
    {
        barkTimer = 0f;
        barkLimit = Random.Range(minTimeBetweenBarks, maxTimeBetweenBarks);  // This can be adjusted if it's too often/not enough
        if (barks.Count > 0 && sisterAudio != null)
        {
            sisterAudio.clip = barks[(int)Random.Range(0, barks.Count - 0.1f)];
        }
    }

    void LilyFootfall()
    {
        //if walking, play a random footfall
        if (sisterAnimator.GetInteger("State") == 1 && GetComponent<SpriteRenderer>().sprite.name.IsOneOf("lily_walking_sprites_2", "lily_walking_sprites_11") && !sisterAudio.isPlaying)
        {
            sisterAudio.PlayOneShot(footstepsWalk[UnityEngine.Random.Range(0, footstepsWalk.Capacity)], 0.25f);
        }
    }

    public void SetDetectRange(float range)
    {
        SetupNPC(speed, 1f, 3f, range);
    }
}
