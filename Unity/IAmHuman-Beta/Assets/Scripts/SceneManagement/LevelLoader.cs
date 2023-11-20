using System;
using System.Collections;
using System.Collections.Generic;
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

        if (fadeAnimatedCanvas != null)
        {
            PlayerPrefs.SetInt("Dead", 0);  // resets if player was dead
            fadeAnimator = fadeAnimatedCanvas.GetComponent<Animator>();
            StartCoroutine(FreezeOnFadeIn());
            if (HoldLonger) { StartCoroutine(HoldDark()); }  // if we are holding on black
            else { fadeAnimator.SetBool("TimeMet", true); }  // if not auto fades in
        }
    }

    // temporary linear scene progression
    private Dictionary<string, string> nextScene = new Dictionary<string, string>()  // again (see below), since all doors will be on interact, do wee need this?
    {
        { "Cabin Exterior + Door", "End of Vertical Slice" },
        { "Vertical Slice - Player Freezing", "End of Vertical Slice" }
    };

    private void OnTriggerEnter2D(Collider2D other)  // since all doors will be interactable, do we nned this ??
    {
        if (other.CompareTag("Door") && !other.GetComponent<Door>().isLocked)
        {
            loadScene(nextScene[getSceneName()]);
        }
    }

    [RuntimeInitializeOnLoadMethod]

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
        PlayerPrefs.SetInt("Fading", 0);
    }
    public IEnumerator HoldDark()
    {
        yield return new WaitForSeconds(HoldFor);  // waits
        fadeAnimator.SetBool("TimeMet", true);  // updates animator parameter so that fade in happens
    }
    public IEnumerator finishLoadScene<T>(T scene)
    {
        // new fade finish function

        if (PlayerPrefs.GetInt("Dead") == 1)  // prevents fading on player death screen restart
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            fadeAnimator.SetBool("Death", true);
        }
        else
        {
            SetScenePositions();
            PlayerPrefs.SetInt("Fading", 1);
            fadeAnimator.SetInteger("EndScene", 1);  // starts the fade out animation
            yield return new WaitForSeconds(1);  // waits one second until it loads other scene so that animation has time to play
            PlayerPrefs.SetInt("Fading", 0);
            if (typeof(T) == typeof(int))
            {
                SceneManager.LoadScene(Convert.ToInt32(scene));
            }
            else if (typeof(T) == typeof(string))
            {
                SceneManager.LoadScene(scene as string);
            }
            
        }

    }

    // this is a wrapper around SceneManager.LoadScene to standardize scene change effects
    public void loadScene<T>(T scene)
    {
        StartCoroutine(finishLoadScene<T>(scene));
    }

    public string getSceneName()
    {
        // Returns the name of the current scene
        return SceneManager.GetActiveScene().name;
    }

    private void SetScenePositions()
    {
        if (getSceneName() == "Hallway Hub")
        {
            PlayerPrefs.SetInt("DoneSisterCinematic", 1);
        } else if (getSceneName() == "Basement") {
            if (PlayerPrefs.GetInt("Flashlight") == 1)
            {
                PlayerPrefs.SetInt("CellarDoorClosed", 1);
            }
        }
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
            Vector3 pos = new Vector3(PlayerPrefs.GetFloat(key+'x'), PlayerPrefs.GetFloat(key+'y'), PlayerPrefs.GetFloat(key+'z'));
            this.gameObject.transform.position = pos;
            CameraMovement.checkBoundsAgain = true;

            foreach (GameObject obj in saveObjectPositions)
            {
                key = getSceneName() + obj.name;
                obj.transform.position = new Vector3(PlayerPrefs.GetFloat(key + 'x'), PlayerPrefs.GetFloat(key + 'y'), PlayerPrefs.GetFloat(key + 'z'));
            }

            foreach (GameObject obj in saveObjectActive)
            {
                key = getSceneName() + obj.name + "active";
                if (PlayerPrefs.HasKey(key))
                {
                    obj.SetActive(PlayerPrefs.GetInt(key) > 0);
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
