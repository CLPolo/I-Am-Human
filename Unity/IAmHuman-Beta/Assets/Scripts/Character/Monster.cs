using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Monster : NPC
{
    public float speed = 1.5f;

    private LogicScript logic;
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        logic = LogicScript.Instance;
        AnimationSetup();
        audioSource = GetComponent<AudioSource>();
        SetupNPC(speed, 0f, null, 8f);
    }

    // Update is called once per frame
    void Update()
    {   
        if (player == null)
        {
            player = Player.Instance;
        }
        AnimationUpdate();
        FollowPlayer();
    }
}
