using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDesignCollectibleScript : MonoBehaviour
{
    [Header("Collection Options")]
    public float collectRadius = 2f; // controls the distance at which the collectible is collected

    [Header("On Collect Behaviour")]
    public AudioSource audioSourceCollect; // audio source for collecting sound
    public AudioClip collectClip; // audio clip for collecting sound

    // control
    private bool enabled;

    // components
    private BoxCollider2D collider;
    private SpriteRenderer renderer;

    // the player
    private Rigidbody2D player;


    // Start is called before the first frame update
    void Start()
    {
        collider = (BoxCollider2D)GetComponent("BoxCollider2D");
        renderer = (SpriteRenderer)GetComponent("SpriteRenderer");
        player = (Rigidbody2D)GameObject.Find("Player").GetComponent("Rigidbody2D");
        enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled){
            if (Mathf.Abs(Vector2.Distance(player.position, transform.position)) <= collectRadius){
                onContact();
            }
        }
    }

    void onContact(){
        // play collect sound
        if (audioSourceCollect != null && collectClip != null){
            audioSourceCollect.clip = collectClip;
            if (!audioSourceCollect.isPlaying){
                audioSourceCollect.Play();
            }
        }
        // disappear (get collected)
        collider.enabled = false;
        renderer.enabled = false;
        enabled = false;
    }
}
