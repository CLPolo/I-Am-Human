using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Transform player;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        offset.Set(0f, 2.5f, -10f);
    }

    // Update is called once per frame
    void Update()
    {
       transform.position = player.position + offset;

    }
}
