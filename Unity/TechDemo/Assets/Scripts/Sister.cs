using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sister : AnimatedEntity
{
    [Header("Objects")]
    public GameObject player;

    [Header("Movement")]
    public float speed = 2f;
    public const float defaultSpeed = 5f;
    public const float runSpeed = 9f;

    [Header("Animation Cycles")]
    public List<Sprite> walkCycle;
    public List<Sprite> runCycle;

    [Header("Animation Variables")]
    public Sprite DefaultSprite;
    // Notably, the three below variables (as well as the corresponding function AnimationUpdate() below) are ripped from lab 4, I was mostly using for testing purposes

    [Header("Sound")]
    //public AudioSource AudioSource;
    public List<AudioClip> footstepsWalk;
    public List<AudioClip> footstepsRun;

    // Start is called before the first frame update
    void Start()
    {
        AnimationSetup();
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
        AnimationUpdate();
    }

    private void FollowPlayer()
    {
        Vector3 playerPosition = player.transform.position;
        float distance = Vector2.Distance(transform.position, playerPosition);
        if (distance > 2)
        {
            Vector3 direction = playerPosition - transform.position;
            direction.z = 0;
            transform.position += direction * Time.deltaTime * speed;
            Flip(direction.x);
        }
    }

    private void Flip(float x)
    {
        GetComponent<SpriteRenderer>().flipX = (x < 0);
    }
}
