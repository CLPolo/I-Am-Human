using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Transform player;
    public Vector3 offset;

    [Header("Level Bounds")]
    // These are the edges of the level where the camera should stop following the player
    public float leftBound;
    public float rightBound;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        offset.Set(0f, 2f, -10f);
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
