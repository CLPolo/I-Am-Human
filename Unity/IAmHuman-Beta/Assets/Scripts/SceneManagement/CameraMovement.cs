﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Transform player;
    public Vector3 offset;
    public Player playerAccess;

    [Header("Screen Shake")]
    public float shakeDuration = 1f;
    public AnimationCurve curve;
    private bool shaking = false;

    [Header("Level Bounds")]
    // These are the edges of the level where the camera should stop following the player
    public float leftBound = -100;
    public float rightBound = 100;

    // Start is called before the first frame update
    void Start()
    {
        playerAccess = Player.Instance;
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
        //if (playerAccess.GetState() == PlayerState.Hiding && !shaking)  // ?? for some reason the player.instance isn't working ??
        if (Input.GetKeyDown(KeyCode.K))  // just for test purposes until the above is figured out
        {
            shaking = true;
            StartCoroutine(Shaking());
        }
    }

    IEnumerator Shaking()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / shakeDuration);
            transform.position = player.position + offset + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = player.position + offset;
        shaking = false;
    }
}