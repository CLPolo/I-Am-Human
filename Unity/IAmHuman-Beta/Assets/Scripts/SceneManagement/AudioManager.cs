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
            yield return null;
        }
        yield break;
    }
}


public class AudioManager : MonoBehaviour
{
    //player
    private Player p;
    private AudioSource pA;
    private PlayerState pState;
    
    //scene
    private SceneManager SceneManager;
    private int scene;
    private string pathBGM = "Sounds/Music/";
    private string pathAmb = "Sounds/SoundEffects/Environment/";
    private string pathCutscene = "Sounds/SoundEffects/Misc/";
    private string pathEntity = "Sounds/SoundEffects/Entity/";
    private string fromScene;

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

        //initiate sceneNames list
        //sceneNames 

        //set scene
        scene = SceneManager.GetActiveScene().buildIndex;
        fromScene = SceneManager.GetActiveScene().name;
        Debug.Log("In Audio Manager Start. Scene: " + fromScene);
        
        if (scene == 0) //Title Screen?
        {   
            Debug.Log("In Title Screen Check at start up");
            if (srcs.TryGetValue("BGM", out AudioSource bgm) && !bgm.isPlaying)
            {   
                if (bgm.clip == null) bgm.clip = (AudioClip)Resources.Load("Sounds/Music/title-theme-lofx");
                Debug.Log("Playing bgm: Title");
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
            pState = p.GetState();
        }

        //check for scene change
        if (scene != SceneManager.GetActiveScene().buildIndex)
        {
            scene = SceneManager.GetActiveScene().buildIndex;
            ChangeScene(scene);
        }
        if (p != null)
        {
            CheckPlayer(p.GetState());
        }
        // PUT ALL LOGIC HERE
        CheckScenewide(scene);
    }

    void ChangeScene(int scene)
    {   Debug.Log("Scene change detected. New scene: " + scene);

        int[] woodFloors = {4, 5, 6, 7, 8};

        //Set scene-wide audio sources
        foreach(KeyValuePair<string, AudioSource> src in srcs){

            string name = src.Key;
            AudioSource s = src.Value; 

            switch (scene){
 
                case 0: //Title Screen
                    Debug.Log("In Title Screen case");
                    ToTitleScreen(s, name);
                    break;

                case 2: //Forest Intro
                    Debug.Log("In Forest-Start case");
                    ToForestStart(s, name);
                    break;
                
                case 3: //Basement
                    ToBasement(s, name);
                    break;
            
                case 4: //Hallway
                    ToHallway(s, name);
                    break;
                }
            }
        

        //Set entity audio clips
        if (woodFloors.Contains(scene)){
            p.SetFootfalls(pathEntity+"Player/Footfalls/Wood/Walk", pathEntity+"Player/Footfalls/Wood/Run");
        } else {
            p.SetFootfalls(pathEntity+"Player/Footfalls/Dirt/Walk", pathEntity+"Player/Footfalls/Wood/Run");
        }

        fromScene = SceneManager.GetActiveScene().name;
    }

    void ToTitleScreen(AudioSource s, string name){
        s.Stop();
        if (name == "BGM"){
            s.clip = (AudioClip)Resources.Load(pathBGM + "title-theme-lofx");
            RestartSource(s, true, 0.25f, 1.0f);
        }
        if (name == "Cutscene")  s.clip = null;
        if (name == "AmbArea") s.clip = null;
        if (name == "AmbMisc") s.clip = null;
    }
    void ToForestStart(AudioSource s, string name){
        if (name == "BGM") s.clip = Resources.Load<AudioClip>(pathBGM + "forest-theme");
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
            RestartSource(s, true, 0.03f, 15f);        
        }  
    }
    void ToBasement(AudioSource s, string name){
        s.Stop();
        if (name == "BGM") {
            s.clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A1");
                RestartSource(s);
            }
        if (name == "Cutscene") {
            s.clip = Resources.Load<AudioClip>(pathEntity + "Interactable/Door/cellar-door-close-0");
            s.PlayOneShot(s.clip);
        }
            
        if (name == "AmbArea") {
            s.clip = Resources.Load<AudioClip>(pathAmb + "Cabin/Basement/basement-drips");
            RestartSource(s, true, 0.02f, 10f);
        }
    }
    void ToHallway(AudioSource s, string name){
        if (name == "BGM") { //TO DO: Logic accounting for choosing music based on progression
            s.loop = false;
            while(s.isPlaying); //TO DO: transition which links the two clips without stalling the game
            s.clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A2");
            RestartSource(s);
            s.loop = true;
        }
        if (name == "AmbArea") {
            s.Stop();
            s.clip = null;
        }
    }
    void RestartSource(AudioSource s, bool fade = false, float targetVolume = 0.25f, float duration = 0.01f){

            s.volume = fade ? 0.01f : targetVolume;
            if (s.isPlaying) s.Stop();
            s.Play();
            if (fade) StartCoroutine(Fade.Start(s, targetVolume, duration));
    }
    void StopAll(bool fade = false){   
        foreach(KeyValuePair<string, AudioSource>  src in srcs) 
        {   if (src.Key != "Cutscene"){
                if (fade) StartCoroutine(Fade.Start(src.Value, 0.01f, 1.5f));
                src.Value.Stop();
            }
        }
    }
    void CheckScenewide(int scene){   
        if (scene != 0){ // != Title Screen
            //Check for cutscene audio before playing everything else
            if (!srcs.TryGetValue("Cutscene", out AudioSource cutscene) || !cutscene.isPlaying) {
                //play background music
                if (srcs.TryGetValue("BGM", out AudioSource bgm) && !bgm.isPlaying && bgm.clip != null)
                {
                    Debug.Log(bgm.clip);
                    RestartSource(bgm);
                }
                //play area ambience
                if (srcs.TryGetValue("AmbArea", out AudioSource ambA) && !ambA.isPlaying && ambA.clip != null) RestartSource(ambA);
                //play general ambience
                if(srcs.TryGetValue("AmbMisc", out AudioSource ambM) && !ambM.isPlaying && ambA.clip != null) RestartSource(ambM);
            }
        }
    }   
    void CheckPlayer(PlayerState state){   

        if(!pA.isPlaying && !p.touchingWall && state != PlayerState.Idle)
        {   
            if(state.isOneOf(PlayerState.Walking, PlayerState.Running)) PlayerFootfall(state);
            
            //TODO
            //if(state == PlayerState.Hiding);// Do hiding sound things
            //if(state.isOneOf(PlayerState.Pushing, PlayerState.Pulling));//todo
            //if(state == PlayerState.Trapped);
        }
    }
    void PlayerFootfall(PlayerState state){
        if (state == PlayerState.Running) 
        {
            pA.PlayOneShot(p.footstepsRun[UnityEngine.Random.Range(0, p.footstepsRun.Capacity)]);

        } else if (state == PlayerState.Walking && p.GetAnimationIndex().isOneOf(2, 11)) {
            pA.PlayOneShot(p.footstepsWalk[UnityEngine.Random.Range(0, p.footstepsWalk.Capacity)], 0.25f);
        }   
    }  
}