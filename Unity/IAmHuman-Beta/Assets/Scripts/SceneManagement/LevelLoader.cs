using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class LevelLoader : MonoBehaviour
{
    private static LevelLoader _instance;
    public static LevelLoader Instance { get { return _instance; } }
    public Player player = null;

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

        float freezeTime = HoldLonger ? (HoldFor + 0.5f) : 0.5f;
        if (player != null)
        {
            player.SetState(PlayerState.Frozen);
            yield return new WaitForSeconds(freezeTime);
            player.SetState(PlayerState.Idle);
        }
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
            SetPlayerPosition();
            fadeAnimator.SetInteger("EndScene", 1);  // starts the fade out animation
            yield return new WaitForSeconds(1);  // waits one second until it loads other scene so that animation has time to play
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

    private void SetPlayerPosition()
    {
        // sets playerprefs for player's position and scene. 

        if (PlayerPrefs.GetInt("positioningIndex") == 0)  // checks every second scene to see if we're returning to the same scene we entered from
        {
            PlayerPrefs.SetFloat("ScenePositionX", transform.position.x);
            PlayerPrefs.SetFloat("ScenePositionY", transform.position.y);
            PlayerPrefs.SetFloat("ScenePositionZ", transform.position.z);
            PlayerPrefs.SetString("PreviousScene", getSceneName());
            PlayerPrefs.SetInt("positioningIndex", 1);
        }
        else
        {
            PlayerPrefs.SetInt("positioningIndex", 0);
        }
        Debug.Log(PlayerPrefs.GetFloat("ScenePositionX") + " " + PlayerPrefs.GetFloat("ScenePositionY") + " " + PlayerPrefs.GetFloat("ScenePositionZ") + " " + PlayerPrefs.GetString("PreviousScene") + " || " + PlayerPrefs.GetInt("positioningIndex") % 2);
    }
    private void CheckPositioning()
    {
        // Checks to see if player's positioning should be update on load, and resets it.

        if (PlayerPrefs.GetString("PreviousScene") == getSceneName())
        {
            Vector3 pos = new Vector3(PlayerPrefs.GetFloat("ScenePositionX"), PlayerPrefs.GetFloat("ScenePositionY"), PlayerPrefs.GetFloat("ScenePositionZ"));
            this.gameObject.transform.position = pos;
        }
    }

    public void StartGame(string sceneName)
    {
        loadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
