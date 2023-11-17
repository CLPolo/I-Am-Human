using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;


[System.Serializable]
public struct CinematicStep
{
    public Vector3 location;
    public string statement;
    public float timeAtLocation;
}

public class Monster : NPC
{
    public float speed = 1.5f;

    private LogicScript logic;
    private AudioSource audioSource;
    private Animator enemyAnimator;


    [Header("Patrol Settings")]
    public bool patrolling = true;
    public List<CinematicStep> patrol;
    private int _patrolIndex;
    private float _patrolTimer = 0;
    private Vector3 _priorPosition;

    private bool stationed = false;
    private bool HideWithin = false;

    // Start is called before the first frame update
    void Start()
    {
        logic = LogicScript.Instance;
        //AnimationSetup();
        audioSource = GetComponent<AudioSource>();
        enemyAnimator = GetComponent<Animator>();
        SetupNPC(speed, 0f, null, 8f);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.Instance;
        }

        //AnimationUpdate();
        CheckFollowing();
        if (patrolling) { HandlePatrolling(); }
        else { FollowPlayer(); }
        DeterminePatrolling();
        

        _priorPosition = transform.position;
        Debug.Log(stationed);
    }

    private void CheckFollowing()
    {
        if (GetFollowing() || Vector2.Distance(transform.position, player.transform.position) >= 0f)
        {
            enemyAnimator.SetInteger("State", 1);
        }
        else if (Vector2.Distance(transform.position, player.transform.position) <= 0f)  // fix for state constantly fluctuating (potentially due to NPC following setting)
        {
            enemyAnimator.SetInteger("State", 0);
        }
    }

    private void HandlePatrolling()
    {
        if (_patrolIndex < patrol.Count)
        {
            //Move player to first cinematicSteps location if not there yet
            if (((transform.position - patrol[_patrolIndex].location).magnitude > 0.005f))
            {
                transform.position += (patrol[_patrolIndex].location - transform.position).normalized * Time.deltaTime * speed;
            }
            else
            {
                transform.position = patrol[_patrolIndex].location;
                if (_patrolTimer >= patrol[_patrolIndex].timeAtLocation)
                {
                    _patrolIndex += 1;//Move on to next one
                    _patrolTimer = 0;
                }
                else
                {
                    _patrolTimer += Time.deltaTime;
                }
            }
        }
        else
        {
            // Repeat patrol
            _patrolIndex = 0;
            _patrolTimer = 0;
        }
    }

    private void DeterminePatrolling()
    {
        // determines if the player is patrolling &/or stationed (hiding latency)

        if ((transform.position - player.transform.position).magnitude < 4f && player.GetState() != PlayerState.Hiding)  // if player within range & not hiding
        {
            patrolling = false;  // not patrolling
        }
        else if (player.GetState() == PlayerState.Hiding)  // handles hiding pause / latency if player started hiding within the range
        {
            enemyAnimator.SetInteger("State", 0);
            HidingLatency();
            patrolling = PlayerPrefs.GetInt("Patrol") == 1;
        }
        else
        {
            patrolling = true;
        }
    }



}
