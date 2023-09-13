using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDesignPlatformScript : MonoBehaviour
{   
    public enum DIRECTION { Horizontal, Vertical};
    [Header("Platform Movement")]
    public bool moving; // turns platform movement on or off
    //public DIRECTION movementOption = DIRECTION.Horizontal; // controls whether we are moving horizontally or vertically 
    public float speed; // speed of moving platform
    public float distanceFromOrigin; // bounds the platform to be [-distance, distance] away from origin
    
    [Header("On Landing Behaviour")]
    public AudioSource audioSourcePlatform; // audio source for platform landing sound
    public AudioClip platformLandingClip; // audio clip for platform landing sound
    public bool disappear; // disappear after a given time when player lands
    public float timeToDisappear; // controls how long it takes the platform to disappear
    public float flashSpeed = 60f; // controls the speed of the platform flashing (can't be 0) 

    // platform movement
    protected float dir = 1.0f;
    protected float bound1;
    protected float bound2;

    // platform disappearance
    protected bool disappearing;
    protected float currDisappear;
    protected float frameCount = 0f;

    // components
    private BoxCollider2D collider;
    private SpriteRenderer renderer;
    private Color baseColor;

    void Start(){
        // set platform bounds
        bound1 = transform.position.x - distanceFromOrigin;
        bound2 = transform.position.x + distanceFromOrigin;
        collider = (BoxCollider2D)GetComponent("BoxCollider2D");
        renderer = (SpriteRenderer)GetComponent("SpriteRenderer");
        baseColor = renderer.material.color;
        currDisappear = timeToDisappear;
    }
     

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = transform.position;
        if (moving){
            newPosition = horizontalMovement();
        }
        if (disappearing){
            currDisappear -= Time.deltaTime;
            if (currDisappear <= 0.0f){
                collider.enabled = false;
                renderer.enabled = false;
                disappearing = false;
            }
            // don't divide by 0
            if (timeToDisappear > 0){
                // adjust transparency of platform based on how close it is to disappearing
                float transparency = (timeToDisappear - Mathf.Abs(timeToDisappear - currDisappear)) / timeToDisappear;
                Color currColor = baseColor;
                currColor.a = transparency * Mathf.Sin(Mathf.PI * 2 * frameCount / 60f);
                renderer.material.color = currColor;
                frameCount += 1;
            }
            
        }
        transform.position = newPosition;
    }

    Vector3 horizontalMovement(){
        Vector3 newPosition = transform.position;
        // perform movement & clamp w.r.t. bounds
        newPosition.x += speed * Time.deltaTime * dir;
        newPosition.x = Mathf.Clamp(newPosition.x, bound1, bound2);

        if (Mathf.Sign(dir) == 1 && newPosition.x == bound2){
            dir *= -1.0f;
        }
        else if (Mathf.Sign(dir) == -1 && newPosition.x == bound1){
            dir *= -1.0f;
        }
        return newPosition;
    }

    void onContact(){
        if (disappear){
            moving = false;
            disappearing = true;
        }

        if (audioSourcePlatform != null && platformLandingClip != null){
            audioSourcePlatform.clip = platformLandingClip;
            if (!audioSourcePlatform.isPlaying){
                audioSourcePlatform.Play();
            }
        }
    }

    
}
