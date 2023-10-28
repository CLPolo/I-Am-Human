using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            if (_instance == null)
            {
                //GameObject AM = Instantiate(Resources.Load("AudioManager", typeof(GameObject))) as GameObject;
                DontDestroyOnLoad(gameObject);
                _instance =.GetComponent<AudioManager>();
            }
            return _instance;
        }
    }

    private Dictionary<string, AudioSource> srcs;


    // Start is called before the first frame update
    void Start()
    {   
        if (Instance != null && Instance != this) Destroy(gameObject);        
        
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

                }
        }
        
    }

    // Update is called once per frame
    void Update()
    {   
        //check if player exists
        if (p == null)  p = Player.Instance;
        if (pA == null) pA = p.AudioSource;

        //check for scene change
        if (scene != SceneManager.GetActiveScene().name){
            scene = SceneManager.GetActiveScene().name;
            ChangeScene(scene);
        }
        // PUT ALL LOGIC HERE
        CheckPlayer(p.GetState());
        CheckScenewide(scene);
    }

    void ChangeScene(string scene)
    {   
        string pathBGM = "Sounds/Music/";
        string pathAmb = "/SoundEffects/Environment/";
        string pathCutscene = "/SoundEffects/Misc/";

        StopAll(false);

        switch (scene){
            case "Vertical Slice":
                foreach(KeyValuePair<string, AudioSource> src in srcs)
                {
                    string name = src.Key;

                    if (name == "BGM") src.Value.clip = null;
                    if (name == "Cutscene")
                    {
                        src.Value.clip = (AudioClip)Resources.Load(pathCutscene + "car-crash-comp.ogg");
                        src.Value.Play();
                    } 
                    if (name == "AmbArea") src.Value.clip = (AudioClip)Resources.Load(pathAmb + "Forest/forest_ambience.ogg");
                    if (name == "AmbMisc") src.Value.clip = (AudioClip)Resources.Load(pathAmb + "creepyambience.ogg");                         
                }  
                break;
            case "Title Screen":
                foreach(KeyValuePair<string, AudioSource>  src in srcs)
                {
                    string name = src.Key;

                    if (name == "BGM") src.Value.clip = (AudioClip)Resources.Load(pathBGM + "title-theme.ogg");
                    if (name == "Cutscene")
                    {
                        src.Value.clip = (AudioClip)Resources.Load(pathCutscene + "car-crash-comp.ogg");
                        src.Value.Play();
                    } 
                    if (name == "AmbArea") src.Value.clip = (AudioClip)Resources.Load(pathAmb + "Forest/forest_ambience.ogg");
                    if (name == "AmbMisc" )src.Value.clip = (AudioClip)Resources.Load(pathAmb + "creepyambience.ogg");
                }
                break;
        }
    }
    void StopAll(bool fade = true)
    {   
        if (fade)
        {
            foreach(KeyValuePair<string, AudioSource>  src in srcs) 
                FadeOut(src.Value);
        } else {

            foreach(KeyValuePair<string, AudioSource>  src in srcs) 
                src.Value.Stop();
        }
    }

    void FadeOut(AudioSource s)
    {
        while (s.volume > 0.01f) s.volume *= 1/2f;
        s.Stop();
    }

    void CheckScenewide(string scene)
    {   
        if (scene != "Title Screen"){
            //Check for cutscene audio before playing everything else
            if (srcs.TryGetValue("Cutscene", out AudioSource cutscene) && !cutscene.isPlaying){
                //play background music
                if(srcs.TryGetValue("BGM", out AudioSource bgm) && !bgm.isPlaying) bgm.PlayOneShot(bgm.clip, 1f);
                //play area ambience
                if(srcs.TryGetValue("AmbArea", out AudioSource ambA) && !ambA.isPlaying) ambA.PlayOneShot(ambA.clip, 1f);
                //play general ambience
                if(srcs.TryGetValue("AmbMisc", out AudioSource ambM) && !ambM.isPlaying) ambM.PlayOneShot(ambM.clip, 1f);
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
        
        {
            if (state == PlayerState.Running) 
            {
                pA.PlayOneShot(p.footstepsRun[UnityEngine.Random.Range(0, p.footstepsRun.Capacity)]);

            } else if (state == PlayerState.Walking && p.GetAnimationIndex().isOneOf(2, 11)) {

                pA.PlayOneShot(p.footstepsWalk[UnityEngine.Random.Range(0, p.footstepsWalk.Capacity)], 0.25f);
            }
        }
    }
}
