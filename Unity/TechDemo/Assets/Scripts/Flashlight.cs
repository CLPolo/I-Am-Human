using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public float Framerate = 5f; //frames per second
    public float movespeed = 1f;

    //private animation stuff
    private float animationTimer;  //current number of seconds since last animation frame update
    private float animationTimerMax;  //max number of seconds for each frame, defined by Framerate

    // Start is called before the first frame update
    void Start()
    {
        // Setup animation stuff ( A LOT OF THIS SCRIPT IS FROM LAB 4)
        animationTimer = 0f;
        animationTimerMax = 1.0f / ((float)(Framerate));
    }

    // Update is called once per frame
    void Update()
    {
        FloatAround();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            gameObject.SetActive(false);  // Make it disappear
            // Play pickup sound here if we have one
            Player player = collision.gameObject.GetComponent<Player>();
            player.hasFlashlight = true;  // The player should now have the flashlight
        }
    }

    private void FloatAround()
    {
        animationTimer += Time.deltaTime;
        transform.position += Vector3.up * Time.deltaTime * movespeed;

        if (animationTimer > animationTimerMax)
        {
            animationTimer = 0;
            movespeed = -movespeed;  // Move downwards after moving upwards, etc.
        }
    }
}
