using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelDesignHazardScript : MonoBehaviour
{
    [Header("Hazard Options")]
    public float deathRadius = 2f; // controls the distance at which the collectible is collected
    public float deathTimer = 0.1f; // controls how long before the scene is reset after death is triggered (allows sound to play)
    
    [Header("On Death Options")]
    public AudioSource deathAudioSource; // audio source for death sound
    public AudioClip deathClip; // audio clip for death sound
    public Color deathFlash = Color.red; // colour to flash on death

    // control variables
    private bool dying = false;

    // components
    private BoxCollider2D collider;
    private SpriteRenderer renderer;

    // the player
    private Rigidbody2D player;
    private SpriteRenderer playerSprite;

    // Start is called before the first frame update
    void Start()
    {
        collider = (BoxCollider2D)GetComponent("BoxCollider2D");
        renderer = (SpriteRenderer)GetComponent("SpriteRenderer");
        player = (Rigidbody2D)GameObject.Find("Player").GetComponent("Rigidbody2D");
        playerSprite = (SpriteRenderer)player.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // on contact, start the death timer and play the death sound
        if (Mathf.Abs(Vector2.Distance(player.position, transform.position)) <= deathRadius){
            dying = true;
            if (deathAudioSource != null && deathClip != null){
                deathAudioSource.clip = deathClip;
                if (!deathAudioSource.isPlaying){
                    deathAudioSource.Play();
                }
            }
        }

        // handle death timer, and reset scene when we are dead
        if (dying){
            playerSprite.material.color = deathFlash;
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0){
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
