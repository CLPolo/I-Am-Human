using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


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
    private AudioSource monsterAudio;
    public AudioClip clip;

    private Animator enemyAnimator;
    public AnimatorOverrideController walkableOverride = null;
    public RuntimeAnimatorController defaultAnimator = null;
    private Sprite idleSprite;
    private bool cutscene = false;


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
    public bool playFootfall = false;
    private bool alertTriggered = false;

    private string monPath = "Sounds/SoundEffects/Entity/Monster/";

    // Start is called before the first frame update
    void Start()
    {
        idleSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
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
        if (SceneManager.GetActiveScene().name == "Attic")
        {
            monsterDefaultSpeed = 4.5f;  // Slow down the monster during attic
        }
        if (monster)
        {
            speed = monsterDefaultSpeed;
            monsterAudio = gameObject.AddComponent<AudioSource>();
            monsterAudio.clip = Resources.Load<AudioClip>(monPath + "creature-idle-breath"); 
            monsterAudio.volume = 0.5f;
            monsterAudio.loop = true;
            
        }
        enemyAnimator = GetComponent<Animator>();
        SetupNPC(speed, 0f, null, DetectionRange);

        if (SceneManager.GetActiveScene().name == "Attic")  // resets things if you restart in the attic.
        {
            PlayerPrefs.SetInt("DoneTransformAnimation", 0);
            if (defaultAnimator != null) enemyAnimator.runtimeAnimatorController = defaultAnimator;
            PlayerPrefs.SetInt("StartTransform", 0);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.Instance;
        }

        if (!cutscene && PlayerPrefs.GetInt("Dead") != 1 && PlayerPrefs.GetInt("Paused") != 1 && PlayerPrefs.GetInt("Fading") != 1)  // doesn't move when dead, paused, or fading.
        {
            CheckFollowing();
            if (patrolling) { HandlePatrolling(); }
            else { FollowPlayer(); }
            DeterminePatrolling();
            CheckAudio();
        } else {
            audioSource?.Stop();
            monsterAudio?.Stop();
        }

        // update sprite direction

        if (SceneManager.GetActiveScene().name == "Attic"? (PlayerPrefs.GetInt("DoneTransformAnimation") == 1) : true && (_priorPosition - transform.position).magnitude > minDiff)  // has moved (and not in attic for cutscene)
        {
            Vector3 pos = (_priorPosition - transform.position);
            Flip(-(pos.x));
        }
        _priorPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "CrawlerRemovalService")
        {
            if (gameObject.name == "Crawler")
            {
                gameObject.SetActive(false);
            }
            cutscene = true;  // Removes patrolling (Similar to freezing player)
            monsterDefaultSpeed = 2f;  // Slow down the monster for the big reveal
            speed = monsterDefaultSpeed;
            enemyAnimator.enabled = false;  // Stop animating for the cutscene
            SpriteRenderer.sprite = idleSprite;
        }
    }

    private void CheckFollowing()
    {
        if (SceneManager.GetActiveScene().name == "Attic" && PlayerPrefs.GetInt("DoneTransformAnimation") == 0)
        {
            if (enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("MonsterTransformEndCutscene"))
            {
                player.SetState(PlayerState.Idle);
                PlayerPrefs.SetInt("DoneTransformAnimation", 1);
                if (walkableOverride != null) enemyAnimator.runtimeAnimatorController = walkableOverride;
            }
            else if (PlayerPrefs.GetInt("StartTransform") == 1)
            {
                enemyAnimator.SetInteger("State", 2);
                player.SetState(PlayerState.Frozen);
            }
        }
        else if (GetFollowing() || Vector2.Distance(transform.position, player.transform.position) >= 0f)
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

    public void CheckAudio()
    {   
        if (PlayerPrefs.GetInt("Paused") == 1 || PlayerPrefs.GetInt("Dead") == 1)
        {
            audioSource.Pause();
            monsterAudio.Pause();
        }
        else
        {
            if (crawler)
            {
                //if patrolling, play walking audio
                if (enemyAnimator.GetInteger("State") == 1 && !audioSource.isPlaying) audioSource.Play();

                //if stopped, stop audio
                if (enemyAnimator.GetInteger("State") == 0) audioSource.Pause();

                //if not patrolling and alert not triggered -> play alert and set flag
                if (!patrolling && !alertTriggered)
                {
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/alert"));
                    alertTriggered = true;
                } 
                //if patrolling and flag not reset -> reset flag.
                if (patrolling && alertTriggered) alertTriggered = false;
            }
            if (monster)
            {   
                //Debug.Log(audioSource.isPlaying);
                //Debug.Log(GetComponent<SpriteRenderer>().sprite.name);
                if(!audioSource.isPlaying && GetComponent<SpriteRenderer>().sprite.name.IsOneOf("monster_walking_sprites_3", "monster_walking_sprites_8") )
                {
                    //Debug.Log("In monster playFootfall");
                    clip = Resources.Load<AudioClip>(monPath + "Footsteps/monster-step-" + UnityEngine.Random.Range(0,4).ToString());
                    audioSource.PlayOneShot(clip);
                }
                if(PlayerPrefs.GetInt("MonsterEnabled") == 2)
                {
                    monsterAudio.Play();
                }
            }
        }
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
