using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private static LevelLoader _instance;
    public static LevelLoader Instance { get { return _instance; } }
    public Player player = null;

    [Header("Scene States")]
    public List<GameObject> saveObjectPositions = new List<GameObject>();
    public List<GameObject> saveObjectActive = new List<GameObject>();

    [Header("Fade Animation")]
    public GameObject fadeAnimatedCanvas = null;
    public Animator fadeAnimator = null;
    public bool HoldLonger = false;  // if we want start fade to be delayed
    public float HoldFor = 0.0f;  // how long we want screen to be dark for initially

    private GameObject deathFade = null;
    private GameObject playerAnimImage = null;

    private List<string> objTags = new List<string> { "LilyBear", "LilyShoe", "Crowbar" };

    // Start is called before the first frame update
    void Start()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        CheckPositioning();

        if (getSceneIndex() == 1 && PlayerPrefs.HasKey("DoneIntroFade"))
        {
            HoldFor = 0;
            if (player.TryGetComponent<Animator>(out var playerAnimator))
            {
                playerAnimator.SetBool("AlreadyPlayed", true);
                playerAnimator.enabled = false;
            }
            if (player.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                spriteRenderer.sprite = player.idleWalk;
            }
        }

        if (fadeAnimatedCanvas != null)
        {
            PlayerPrefs.SetInt("Dead", 0);  // resets if player was dead
            fadeAnimator = fadeAnimatedCanvas.GetComponent<Animator>();
            StartCoroutine(FreezeOnFadeIn());
            if (HoldLonger) { StartCoroutine(HoldDark()); }  // if we are holding on black
            else { fadeAnimator.SetBool("TimeMet", true); }  // if not auto fades in

            deathFade = GameObject.Find("Canvas (Follows Player)/DeathScreenFade"); // finds the deathscreen anim object under canvas, needs to be active to be found
            playerAnimImage = GameObject.Find("Canvas (Follows Player)/DeathScreenFade/Player");
            deathFade?.SetActive(false);  // so we turn it off immediately
        }
    }

    void Update()
    {
        // below handles the player death animation
        if (PlayerPrefs.GetInt("Dead") == 1)  // if player is dead
        {
            deathFade?.gameObject.SetActive(true);  // turn on the deathscreen anim object (has the bkgrnd & player image w/ anims)
            fadeAnimator?.SetBool("Death", true);  // starts the animation
            if (fadeAnimator != null && fadeAnimator.GetCurrentAnimatorStateInfo(0).IsName("DeathScreen"))  // once the animation is done (state transitions to 'DeathScreen') 
            {
                deathFade.gameObject.SetActive(false);  // turn off deathfade screen / obj so we can see the death screen
                LogicScript.Instance.ActivateDeathScreen();  // turns on the death screen
            }
        }
        CheckAndUpdateInventoryOverlay();
    }


    [RuntimeInitializeOnLoadMethod]

    public void CheckAndUpdateInventoryOverlay()
    {

        if (player != null && player.finalCutscene)  // turns the whole thing off during the final cutscene
        {
            GameObject invCanvas = GameObject.Find("Canvas (Follows Player)/Inventory Overlay");
            if (invCanvas != null) { invCanvas.SetActive(false); }
        }
        else
        {
            // handles the inventory items appearing on the overlay
            foreach (string pickup in objTags)
            {
                if (PlayerPrefs.GetInt(pickup) == 1)
                {
                    GameObject invCanvas = GameObject.Find("Canvas (Follows Player)/Inventory Overlay/" + pickup + "Inv");
                    invCanvas?.SetActive(true);
                }
            }

            if (PlayerPrefs.GetInt("StudyKey") == 1 && PlayerPrefs.GetInt("StudyDoorOpened") != 1)  // brief window of time where player has study key and hasn't opened the door yet
            {
                GameObject invCanvas = GameObject.Find("Canvas (Follows Player)/Inventory Overlay/KeyInv");
                invCanvas?.SetActive(true);
            }
            else if (PlayerPrefs.GetInt("StudyKey") == 1 && PlayerPrefs.GetInt("StudyDoorOpened") == 1)
            {
                GameObject invCanvas = GameObject.Find("Canvas (Follows Player)/Inventory Overlay/KeyInv");
                invCanvas?.SetActive(false);
            }

            if (PlayerPrefs.GetInt("AtticKey") == 1)  // this one can be perma in inv, unless we don't want that, cause the atiic door always appears closed so you'd resuse the key everytime you opened it
            {
                GameObject invCanvas = GameObject.Find("Canvas (Follows Player)/Inventory Overlay/KeyInv");
                invCanvas?.SetActive(true);
            }
        }


    }
    public IEnumerator FreezeOnFadeIn()
    {
        // Freezes the player while the fade in is happening.
        PlayerPrefs.SetInt("Fading", 1);
        float freezeTime = HoldLonger ? (HoldFor + 0.5f) : 0.5f;
        if (player != null && getSceneName() != "Hallway Hub")  // would prevent player freezing for lily cutscene (the hallway hub check)
        {
            player.SetState(PlayerState.Frozen);
            yield return new WaitForSeconds(freezeTime);
            player.SetState(PlayerState.Idle);
        }
        PlayerPrefs.SetInt("Fading", 2);
    }
    public IEnumerator HoldDark()
    {
        yield return new WaitForSeconds(HoldFor);  // waits
        fadeAnimator.SetBool("TimeMet", true);  // updates animator parameter so that fade in happens
    }
    public IEnumerator finishLoadScene<T>(T scene, bool saveStates)
    {
        // new fade finish function

        if (PlayerPrefs.GetInt("Dead") == 1)  // prevents fading on player death screen restart
        {
            PlayerPrefs.SetInt("Dead", 0);
            fadeAnimator.SetBool("Death", true);
        }
        else
        {
            SetScenePositions(saveStates);
            PlayerPrefs.SetInt("Fading", 1);
            if (fadeAnimator != null) fadeAnimator.SetInteger("EndScene", 1);  // starts the fade out animation
            yield return new WaitForSeconds(1);  // waits one second until it loads other scene so that animation has time to play
            PlayerPrefs.SetInt("Fading", 0);
        }
        if (typeof(T) == typeof(int))
        {
            SceneManager.LoadScene(Convert.ToInt32(scene));
        }
        else if (typeof(T) == typeof(string))
        {
            SceneManager.LoadScene(scene as string);
        }
    }

    // this is a wrapper around SceneManager.LoadScene to standardize scene change effects
    public void loadScene<T>(T scene, bool saveStates = true)
    {
        StartCoroutine(finishLoadScene<T>(scene, saveStates));
    }

    public string getSceneName()
    {
        // Returns the name of the current scene
        return SceneManager.GetActiveScene().name;
    }

    public int getSceneIndex() => SceneManager.GetActiveScene().buildIndex;

    private void SetScenePositions(bool saveStates)
    {
        if (getSceneName() == "Hallway Hub")
        {
            PlayerPrefs.SetInt("DoneSisterCinematic", 1);
        } else if (getSceneName() == "Basement") {
            if (PlayerPrefs.GetInt("Flashlight") == 1)
            {
                PlayerPrefs.SetInt("CellarDoorClosed", 1);
            }
        } else if (getSceneIndex() == 1) {
            PlayerPrefs.SetInt("DoneIntroFade", 1);
        }

        if (!saveStates) { return; }
        // sets playerprefs for player's position and scene. 
        PlayerPrefs.SetInt(getSceneName(), 1);
        PlayerPrefs.SetFloat(getSceneName() + "playerx", transform.position.x);
        PlayerPrefs.SetFloat(getSceneName() + "playery", transform.position.y);
        PlayerPrefs.SetFloat(getSceneName() + "playerz", transform.position.z);
        foreach (GameObject obj in saveObjectPositions)
        {
            PlayerPrefs.SetFloat(getSceneName() + obj.name + 'x', obj.transform.position.x);
            PlayerPrefs.SetFloat(getSceneName() + obj.name + 'y', obj.transform.position.y);
            PlayerPrefs.SetFloat(getSceneName() + obj.name + 'z', obj.transform.position.z);
        }
        foreach (GameObject obj in saveObjectActive)
        {
            PlayerPrefs.SetInt(getSceneName() + obj.name + "active", obj.activeSelf ? 1 : 0);
        }
    }
    private void CheckPositioning()
    {
        // Checks to see if player's positioning should be update on load, and resets it.

        if (PlayerPrefs.HasKey(getSceneName()))
        {
            string key = getSceneName() + "player";
            if (PlayerPrefs.HasKey(key+'x') && player != null)
            {
                player.transform.position = new Vector3(PlayerPrefs.GetFloat(key + 'x'), PlayerPrefs.GetFloat(key + 'y'), PlayerPrefs.GetFloat(key + 'z')); ;
                CameraMovement.checkBoundsAgain = true;
            }

            foreach (GameObject obj in saveObjectPositions)
            {
                key = getSceneName() + obj.name;
                Vector3 pos = new Vector3(PlayerPrefs.GetFloat(key + 'x'), PlayerPrefs.GetFloat(key + 'y'), PlayerPrefs.GetFloat(key + 'z'));
                if (obj.tag != "Enemy" || Math.Abs(player.transform.position.x - pos.x) > 2.5)
                {
                    obj.transform.position = pos;
                }
            }

            foreach (GameObject obj in saveObjectActive)
            {
                key = getSceneName() + obj.name + "active";
                if (PlayerPrefs.HasKey(key))
                {
                    bool active = PlayerPrefs.GetInt(key) > 0;
                    obj.SetActive(active);
                }
            }
        }
    }

    public void StartGame(string sceneName)
    {
        if (PlayerPrefs.HasKey("StartScene"))
        {
            loadScene(PlayerPrefs.GetString("StartScene"));
        } else {
            loadScene(sceneName);
        }
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        StartGame("Forest Intro");
    }
}
