using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;


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
    private float monsterDefaultSpeed = 7f;
    public float DetectionRange = 3f;

    private LogicScript logic;
    private AudioSource audioSource;
    private Animator enemyAnimator;


    [Header("Patrol Settings")]
    public bool patrolling = true;
    public List<CinematicStep> patrol;
    private int _patrolIndex;
    private float _patrolTimer = 0;
    private Vector3 _priorPosition;
    private float minDiff = 0.000001f;

    private bool stationed = false;
    public bool crawler = true;
    public bool monster = false;

    // Start is called before the first frame update
    void Start()
    {
        logic = LogicScript.Instance;
        audioSource = GetComponent<AudioSource>();
        if (crawler)
        {
            audioSource.clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Crabs/crab-walk-loop-0");
            audioSource.time = Random.Range(0, audioSource.clip.length/1);
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.Play();
        }
        if (monster)
        {
            speed = monsterDefaultSpeed;
            // HI COREY !!! HELLO COREY!!!!! HIIIIIII do audio stuff here :D
        }
        enemyAnimator = GetComponent<Animator>();
        SetupNPC(speed, 0f, null, DetectionRange);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.Instance;
        }

        if (PlayerPrefs.GetInt("Dead") != 1 && PlayerPrefs.GetInt("Paused") != 1 && PlayerPrefs.GetInt("Fading") != 1)  // doesn't move when dead, paused, or fading.
        {
            CheckFollowing();
            if (patrolling) { HandlePatrolling(); }
            else { FollowPlayer(); }
            DeterminePatrolling();
            CheckAudio();
        }


        // update sprite direction
        if ((_priorPosition - transform.position).magnitude > minDiff)  // has moved
        {
            Vector3 pos = (_priorPosition - transform.position);
            Flip(-(pos.x));
        }
        _priorPosition = transform.position;
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
                enemyAnimator.SetInteger("State", 0);
                if (_patrolTimer >= patrol[_patrolIndex].timeAtLocation)
                {
                    _patrolIndex += 1;  //Move on to next one
                    _patrolTimer = 0;
                    enemyAnimator.SetInteger("State", 1);
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

    private void CheckAudio()
    {   
        //if patrolling, play walking audio
        if (enemyAnimator.GetInteger("State") == 1 && !audioSource.isPlaying) audioSource.Play();

        //if stopped, stop audio
        if (enemyAnimator.GetInteger("State") == 0) audioSource.Pause();
    }

    private void DeterminePatrolling()
    {
        // determines if the player is patrolling &/or stationed (hiding latency)
        if (crawler)
        {
            if (!patrolling && PlayerPrefs.GetInt("finishedLatency") == 1)  // changes back to patrolling
            {
                PlayerPrefs.SetInt("finishedLatency", 0);
                patrolling = true;
            }
            else if ((transform.position - player.transform.position).magnitude < DetectionRange && player.GetState() != PlayerState.Hiding)  // if player within range & not hiding
            {
                patrolling = false;  // not patrolling
            }
            else if (!patrolling && player.GetState() == PlayerState.Hiding)  // if following & player hides
            {
                enemyAnimator.SetInteger("State", 0);  // idle animation
                HidingLatency();  // pauses crawler for 2 seconds
            }
            else
            {
                patrolling = true;
            }
        }
        if (monster)
        {
            if (patrolling && (gameObject.transform.position.x < 70 || gameObject.transform.position.x > 60))
            {
                speed = monsterDefaultSpeed - 3;
            }
            if (!patrolling && PlayerPrefs.GetInt("finishedLatency") == 1)  // changes back to patrolling
            {
                PlayerPrefs.SetInt("finishedLatency", 0);
                patrolling = true;
            }
            else if ((transform.position - player.transform.position).magnitude < DetectionRange && player.GetState() != PlayerState.Hiding)  // if player within range & not hiding
            {
                patrolling = false;  // not patrolling
                speed = monsterDefaultSpeed;
            }
            else if (!patrolling && player.GetState() == PlayerState.Hiding)  // if following & player hides
            {
                PlayerPrefs.SetInt("finishedLatency", 1);
            }
            else
            {
                patrolling = true;
            }
        }
        
    }



}
