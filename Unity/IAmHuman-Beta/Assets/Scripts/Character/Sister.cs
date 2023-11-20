using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        sisterAudio = GetComponent<AudioSource>();
        sisterAnimator = GetComponent<Animator>();
        //AnimationSetup();  (ONLY NEEDED IF DECIDE NOT TO USE ANIMATOR)
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
        //AnimationUpdate();  (ONLY NEEDED IF DECIDE NOT TO USE ANIMATOR)
        FollowPlayer();
        Bark();
        if (GetFollowing())
        {
            sisterAnimator.SetInteger("State", 1);
        }
        else
        {
            sisterAnimator.SetInteger("State", 0);
        }
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
}
