using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sister : AnimatedEntity
{
    [Header("Objects")]
    public GameObject player;

    [Header("Movement")]
    public float speed = 1f;

    [Header("Animation Cycles")]
    public List<Sprite> walkCycle;
    public List<Sprite> runCycle;

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

    private bool following = false;

    // Start is called before the first frame update
    void Start()
    {
        sisterAudio = GetComponent<AudioSource>();
        BarkReset();  // Bark will happen randomly every 10-30 seconds
        AnimationSetup();
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
        AnimationUpdate();
        Bark();
    }

    private void FollowPlayer()
    {
        Vector3 playerPosition = player.transform.position;
        float distance = Vector2.Distance(transform.position, playerPosition);
        if (following ? distance > 1 : distance > 3)
        {
            following = true;
            Vector3 direction = playerPosition - transform.position;
            direction.y = 0;
            direction.z = 0;
            direction = direction.normalized;
            transform.position += direction * Time.deltaTime * speed;
            Flip(direction.x);
        }
        else
        {
            following = false;
        }
    }

    private void Flip(float x)
    {
        GetComponent<SpriteRenderer>().flipX = (x < 0);
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
