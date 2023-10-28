using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade {
    public static IEnumerator Start(AudioSource audioSource, float targetVolume = 1f, float duration = 3f)
    {   //taken from https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/ 
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            Debug.Log(audioSource.volume + " " + audioSource.clip.name);
            yield return null;
        }
        yield break;
    }
}

public class AudioManager : MonoBehaviour
{
    private Player p;
    private AudioSource pA;
    
    private SceneManager SceneManager;
    private string scene;

    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            
            return _instance;
        }
    }

    private Dictionary<string, AudioSource> srcs;
    private Fade Fade;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        } else if (Instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        if (srcs == null || srcs.Count == 0){  
            //populate AudioSource dict
            srcs = GameObject.FindWithTag("AudioManager")
            .GetComponentsInChildren<AudioSource>().ToDictionary(child => child.gameObject.name);
        }

        scene = SceneManager.GetActiveScene().name;

        if (scene == "Title Screen")
        {   
            if (srcs.TryGetValue("BGM", out AudioSource bgm) && !bgm.isPlaying)
            {
                bgm.Play();
                StartCoroutine(Fade.Start(bgm, 0.25f, 1f));

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //check if player exists
        if (p == null && Player.Instance != null)
        {
            p = Player.Instance;
            pA = p.AudioSource;
        }

        //check for scene change
        if (scene != SceneManager.GetActiveScene().name){
            scene = SceneManager.GetActiveScene().name;
            ChangeScene(scene);
        }
        if (p != null)
        {
            CheckPlayer(p.GetState());
        }
        // PUT ALL LOGIC HERE
        CheckScenewide(scene);
    }

    void ChangeScene(string scene)
    {   
        string pathBGM = "Sounds/Music/";
        string pathAmb = "Sounds/SoundEffects/Environment/";
        string pathCutscene = "Sounds/SoundEffects/Misc/";

        StopAll(true);
        foreach(KeyValuePair<string, AudioSource> src in srcs){

            string name = src.Key;
            AudioSource s = src.Value; 

            switch (scene){
 
                case "Vertical Slice":
                    
                    if (name == "BGM") s.clip = null;
                    if (name == "Cutscene")
                    {   
                        s.clip = Resources.Load<AudioClip>(pathCutscene + "car-crash-comp");
                        s.PlayOneShot(s.clip, 0.8f);
                    } 
                    if (name == "AmbArea") {
                        s.clip = Resources.Load<AudioClip>(pathAmb + "Forest/forest_ambience");
                        RestartSource(s, true, 0.25f, 15f);
                    }
                    if (name == "AmbMisc"){
                        s.clip = Resources.Load<AudioClip>(pathAmb + "creepyambience");     
                        RestartSource(s, true, 0.08f, 15f);        
                    }  
                    break;

                case "Title Screen":

                    if (name == "BGM") s.clip = (AudioClip)Resources.Load(pathBGM + "title-theme-lofx");
                    if (name == "Cutscene")
                    {
                        // s.clip = (AudioClip)Resources.Load(pathCutscene + "car-crash-comp");
                        // s.Play();
                    } 
                    if (name == "AmbArea") s.clip = (AudioClip)Resources.Load(pathAmb + "Forest/forest_ambience");
                    if (name == "AmbMisc" )s.clip = (AudioClip)Resources.Load(pathAmb + "creepyambience");
                    break;
                case "End of Vertical Slice":
                    if (name == "Cutscene") s.PlayOneShot(Resources.Load<AudioClip>(pathCutscene + "death-temp"), 0.8f);
                    if (name == "AmbMisc") {
                        s.clip = Resources.Load<AudioClip>(pathAmb + "creepyambience");
                        RestartSource(s, true, 0.02f, 20f);
                    }
                    break;
            }       
        }
    }

    void RestartSource(AudioSource s, bool fade = false, float targetVolume = 0.5f, float duration = 0f){
            s.volume = 0.01f;
            if (s.isPlaying) s.Stop();
            s.Play();
            if (fade) StartCoroutine(Fade.Start(s, targetVolume, duration));
    }

    void StopAll(bool fade = true)
    {   
        foreach(KeyValuePair<string, AudioSource>  src in srcs) 
        {   if (src.Key != "Cutscene"){
                if (fade) StartCoroutine(Fade.Start(src.Value, 0.01f, 1.5f));
                src.Value.Stop();
            }
        }
    }


    void CheckScenewide(string scene)
    {   
        if (scene != "Title Screen"){
            //Check for cutscene audio before playing everything else
            if (srcs.TryGetValue("Cutscene", out AudioSource cutscene) && !cutscene.isPlaying) {
                //play background music
                if(srcs.TryGetValue("BGM", out AudioSource bgm) && !bgm.isPlaying && bgm.clip != null) RestartSource(bgm);
                //play area ambience
                if (srcs.TryGetValue("AmbArea", out AudioSource ambA) && !ambA.isPlaying && ambA.clip != null) RestartSource(ambA);
                //play general ambience
                if(srcs.TryGetValue("AmbMisc", out AudioSource ambM) && !ambM.isPlaying && ambA.clip != null) RestartSource(ambM);
            }
        }
    }   
    void CheckPlayer(PlayerState state)
    {
        if(!pA.isPlaying && !p.touchingWall && state != PlayerState.Idle)
        {
            if(state.isOneOf(PlayerState.Walking, PlayerState.Running)) PlayerFootfall(state);
            
            //TODO
            if(state == PlayerState.Hiding);// Do hiding sound things
            if(state.isOneOf(PlayerState.Pushing, PlayerState.Pulling));//todo
            if(state == PlayerState.Trapped);
        }
    }

    void PlayerFootfall(PlayerState state)
    {   
        if (state == PlayerState.Running) 
        {
            pA.PlayOneShot(p.footstepsRun[UnityEngine.Random.Range(0, p.footstepsRun.Capacity)]);

        } else if (state == PlayerState.Walking && p.GetAnimationIndex().isOneOf(2, 11)) {

            pA.PlayOneShot(p.footstepsWalk[UnityEngine.Random.Range(0, p.footstepsWalk.Capacity)], 0.25f);
        }   
    }
}
