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

    [Header("Fade Animation")]
    public GameObject fadeAnimatedCanvas = null;
    public Animator fadeAnimator = null;
    public bool HoldLonger = false;  // if we want start fade to be delayed
    public float HoldFor = 0.0f;  // how long we want screen to be dark for initially
    
    // fading stuff
    // NOTE: EVEN I DONT KNOW HOW THIS CODE IS WORKING SO PROBABLY DONT TOUCH IT
    private bool fadeOut;
    private bool doneFade = false;
    public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.6f, 0.7f, -1.8f, -1.2f), new Keyframe(1, 0));
    private float _alpha = 1;
    private Texture2D _texture;
    private float _time = 0;
    // how fast should the fade effect be
    private float _timeRatio = 1; // if you set it to 0.5, the fade will take 2x as long
    // number of seconds to wait on black screen
    private float _timeDark = 0; // if you want the black screen longer increase this

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

        if (fadeAnimatedCanvas != null)
        {
            fadeAnimator = fadeAnimatedCanvas.GetComponent<Animator>();
            StartCoroutine(FreezeOnFadeIn());
            if (HoldLonger) { StartCoroutine(HoldDark()); }  // if we are holding on black
            else { fadeAnimator.SetBool("TimeMet", true); }  // if not auto fades in

        }
    }

    // temporary linear scene progression
    private Dictionary<string, string> nextScene = new Dictionary<string, string>()
    {
        { "Cabin Exterior + Door", "End of Vertical Slice" },
        { "Vertical Slice - Player Freezing", "End of Vertical Slice" }
    };

    private void OnTriggerEnter2D(Collider2D other)  // since all doors will be interactable, do we nned this ??
    {
        if (other.tag == "Door" && !other.GetComponent<Door>().isLocked)
        {
            loadScene(nextScene[getSceneName()]);
        }
    }

    //public void Reset()
    //{
    //    doneFade = false;
    //    _alpha = 1;
    //    _time = 0;
    //}

    [RuntimeInitializeOnLoadMethod]
    // COMMENTED OUT FOR NEW FADE
    //public void RedoFade()
    //{
    //    Reset();
    //}

    // COMMENTED OUT FOR NEW FADE
    //public void OnGUI()
    //{
    //    if (_texture != null && _alpha > 0)
    //    {
    //        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texture);
    //    }
    //    if (doneFade) return;
    //    if (_texture == null) _texture = new Texture2D(1, 1);
    //    _texture.SetPixel(0, 0, new Color(0, 0, 0, _alpha));
    //    _texture.Apply();
    //    _time += Time.deltaTime;
    //    if (!fadeOut)
    //    {
    //        _alpha = FadeCurve.Evaluate(_time*_timeRatio);
    //    } else {
    //        _alpha = 1 - FadeCurve.Evaluate(_time*_timeRatio);
    //    }
    //    if ((_alpha <= 0 && !fadeOut) || (_alpha >= 1 && fadeOut)) {
    //        doneFade = true;
    //    }
    //}

    // COMMENTED OUT FOR NEW FADE
    //public void FadeToBlack(bool _fadeOut = true)
    //{
    //    fadeOut = _fadeOut;
    //    doneFade = false;
    //    _time = 0;
    //}

    // COMMENTED OUT FOR NEW FADE
    //public IEnumerator finishLoadScene<T>(T scene)
    //{
    //    yield return new WaitUntil(() => doneFade);
    //    yield return new WaitForSeconds(_timeDark);
    //    if (doneFade)
    //    {
    //        if (typeof(T) == typeof(int))
    //        {
    //            SceneManager.LoadScene(Convert.ToInt32(scene));
    //        } else if (typeof(T) == typeof(string)) {
    //            SceneManager.LoadScene(scene as string);
    //        }
    //        FadeToBlack(false);
    //    }
    //    yield return null;
    //}

    public IEnumerator FreezeOnFadeIn()
    {
        // Freezes the player while the fade in is happening.

        float freezeTime = HoldLonger ? (HoldFor + 0.5f) : 0.5f;

        this.gameObject.GetComponent<Player>().SetState(PlayerState.Frozen);
        yield return new WaitForSeconds(freezeTime);
        this.gameObject.GetComponent<Player>().SetState(PlayerState.Idle);

    }
    public IEnumerator HoldDark()
    {
        yield return new WaitForSeconds(HoldFor);  // waits
        fadeAnimator.SetBool("TimeMet", true);  // updates animator parameter so that fade in happens
    }
    public IEnumerator finishLoadScene<T>(T scene)
    {
        // new fade finish function

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

    // this is a wrapper around SceneManager.LoadScene to standardize scene change effects
    public void loadScene<T>(T scene)
    {
        //FadeToBlack();  // COMMENTED OUT FOR NEW FADE
        StartCoroutine(finishLoadScene<T>(scene));
    }

    public string getSceneName()
    {
        // Returns the name of the current scene
        return SceneManager.GetActiveScene().name;
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
