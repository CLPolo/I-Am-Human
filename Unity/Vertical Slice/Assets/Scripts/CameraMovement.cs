using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Transform player;
    public Vector3 offset;

    [Header("Level Bounds")]
    // These are the edges of the level where the camera should stop following the player
    public float leftBound = -100;
    public float rightBound = 100;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        offset.Set(0f, 2f, -10f);
        if (player.transform.position.x <= leftBound)
        {
            transform.position = new Vector3(leftBound, player.position.y, player.position.z) + offset;
        }
        else if (player.transform.position.x >= rightBound)
        {
            transform.position = new Vector3(rightBound, player.position.y, player.position.z) + offset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.x > leftBound && player.transform.position.x < rightBound)
        {
            transform.position = player.position + offset;  // Only change position if within bounds
        }

    }
}
