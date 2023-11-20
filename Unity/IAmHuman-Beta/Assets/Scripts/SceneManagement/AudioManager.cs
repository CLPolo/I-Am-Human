using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    //player
    private Player p;
    private AudioSource pA;
    
    //scene
    private SceneManager SceneManager;
    private int scene;
    private int fromScene; 
    private int[] woodFloors = {2, 3, 4, 5, 6, 7, 8};

    //resource paths
    private string       pathBGM = "Sounds/Music/";
    private string       pathAmb = "Sounds/SoundEffects/Environment/";
    private string    pathEntity = "Sounds/SoundEffects/Entity/";
    private string  pathInteract = "Sounds/SoundEffects/Entity/Interactable/";
    private string  pathCutscene = "Sounds/SoundEffects/Misc/";

    //BGM management flags
    private bool      playBGM = true;
    private bool     playBGM2 = false;
    private bool     playBGM3 = false;
   
    //other flags
    private bool  playAmbArea = false;
    private bool  playAmbMisc = false;
    private bool  trapEntered = false;
    
    //progression flags
    private bool              inCabin = false;
    private bool            inKitchen = false;
    private bool              inAttic = false;
    private bool     kitchenTriggered = false;
    private bool crowbarFadeTriggered = false;
    private bool          gameStarted = false;

    private static AudioManager _instance;
    public  static AudioManager Instance{ get{ return _instance; }}

    private Dictionary<string, AudioSource> srcs;

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
        
        //populate AudioSource dict        
        if (srcs == null || srcs.Count == 0)
        {  
            srcs = GameObject.FindWithTag("AudioManager")
            .GetComponentsInChildren<AudioSource>().ToDictionary(child => child.gameObject.name);
        }

        //set scene
        scene = SceneManager.GetActiveScene().buildIndex;
        
        if (scene == 0) //Title Screen?
        {   
            if (srcs.TryGetValue("BGM", out AudioSource bgm) && !bgm.isPlaying)
            {   
                if (bgm.clip == null) bgm.clip = (AudioClip)Resources.Load("Sounds/Music/title-theme-lofx");
                RestartSource(bgm, true, 0.25f, 1f);
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
            pA = p.gameObject.GetComponent<AudioSource>();
        }

        //check for scene change
        if (scene != SceneManager.GetActiveScene().buildIndex)
        {   
            fromScene = scene;
            scene = SceneManager.GetActiveScene().buildIndex;
            ChangeScene(scene);
        }

        if (p != null) CheckPlayer();
        CheckProgress();
        CheckScenewide(scene);
    }
    void CheckScenewide(int scene){   

        //Only resume other audio if cutscene isn't playing
        if (!srcs.TryGetValue("Cutscene", out AudioSource cutscene) || !cutscene.isPlaying) { //Debug.Log("BGM playing? " + srcs["BGM"].isPlaying + 
                                                                                                   // " || 2 playing? " + srcs["BGM2"].isPlaying +
                                                                                                   //eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee  " || 3 playing? " + srcs["BGM3"].isPlaying);
            //check background music
            if (srcs.TryGetValue("BGM", out AudioSource bgm))
            {    
                // if bgm isn't already playing, has an audio clip, and global is set, then play
                if (!bgm.isPlaying && bgm.clip != null && playBGM) RestartSource(bgm);

                // if bgm is playing and shouldn't be, stop it
                if (bgm.isPlaying && !playBGM) bgm.Stop();

                // if bgm is playing and should be, but the volume is too low, restart it.
                if (bgm.isPlaying && playBGM && bgm.volume < 0.01f) RestartSource(bgm);
            }        
            //check area ambience
            if (srcs.TryGetValue("AmbArea", out AudioSource ambArea))
            {    
                // if ambArea isn't already playing, has an audio clip, and global is set, then play
                if (!ambArea.isPlaying && ambArea.clip != null && playAmbArea) RestartSource(ambArea);

                // if ambArea is playing and shouldn't be, stop it
                if (ambArea.isPlaying && !playAmbArea) ambArea.Stop();

                // if ambArea is playing and should be, but the volume is too low, restart it.
                if (ambArea.isPlaying && playAmbArea && ambArea.volume < 0.01f) RestartSource(ambArea);
            }
            //check general ambience
            if (srcs.TryGetValue("AmbMisc", out AudioSource ambMisc))
            {    
                // if ambMisc isn't already playing, has an audio clip, and global is set, then play
                if (!ambMisc.isPlaying && ambMisc.clip != null && playAmbMisc) RestartSource(ambMisc);

                // if ambMisc is playing and shouldn't be, stop it
                if (ambMisc.isPlaying && !playAmbMisc) ambMisc.Stop();

                // if ambMisc is playing and should be, but the volume is too low, restart it.
                if (ambMisc.isPlaying && playAmbMisc && ambMisc.volume < 0.001f) RestartSource(ambMisc);
            }
        }
    }

    private void CheckProgress()
    {
        if (inCabin)
        {   
            if (!srcs["BGM2"].isPlaying) srcs["BGM2"].Play();
            if (!srcs["BGM3"].isPlaying) srcs["BGM3"].Play();

            if (PlayerPrefs.GetInt("Crowbar") == 1)
            {
                if(!crowbarFadeTriggered)
                {   
                    //Fade into next distortion level
                    playBGM = false;
                    //fade in distorted version
                    StartCoroutine(Start(srcs["BGM2"], 0.33f, 0.75f));
                    //fade out currently playing version
                    StartCoroutine(Start(srcs["BGM"], 0f, 0.75f, true));
                    crowbarFadeTriggered = true;
                }
            }
            //if you have entered the kitchen, play distortion lvl 2
            if (inKitchen && !kitchenTriggered)
            {   
                kitchenTriggered = true;
                playBGM = false;
                    //fade in distorted version
                    StartCoroutine(Start(srcs["BGM3"], 0.33f, 0.75f));
                    //fade out currently playing version
                    StartCoroutine(Start(srcs["BGM2"], 0f, 0.75f, true));
                    crowbarFadeTriggered = true;
            }
        }
    }

    void ChangeScene(int scene)
    {   
        //if coming from the title screen, fade out all audio
        if (fromScene == 0) Stop(true, true);

        //Set scene-wide audio sources
        switch (scene){
            //Title Screen
            case 0: 
                // fade out all other audio when returning to title screen
                inCabin = false;
                Stop(true, true);
                ToTitleScreen();
                break;

            //Forest Intro
            case 1: 
                inCabin = false;
                Stop(true, true);
                ToForestStart();
                break;
            
            //Basement
            case 2:
                //can only enter cabin via basement; no need to set to true anywhere else
                inCabin = true;
                if(fromScene == 1) Stop(true, true);
                ToBasement();
                break;

            //Hallway
            case 3: 
                if (fromScene == 7)
                {
                    inCabin = true; //set so bgm resumes in CheckProgress()
                    kitchenTriggered = false;
                }

                ToHallway();
                break;

            //Kitchen
            case 4:
                inKitchen = true;
                //ToKitchen();
                break;

            //Attic Stairwell
            case 7:
                //if entering from the hall way, pause bgm
                if (fromScene == 3)
                {
                    inCabin = false; // set so CheckProgress() doesn't automatically restart bgm
                    srcs["BGM3"].Pause();
                    srcs["BGM2"].Pause();
                }
                break;

            //Attic
            case 8:
                break;

            //Chase
            case 9:
                break;
        }

        //Set entity audio clips
        if (woodFloors.Contains(scene)){
            p.SetFootfalls(pathEntity+"Player/Footfalls/Wood/Walk", pathEntity+"Player/Footfalls/Wood/Run");
        } else {
            p.SetFootfalls(pathEntity+"Player/Footfalls/Dirt/Walk", pathEntity+"Player/Footfalls/Wood/Run");
        }
    }


    void ToTitleScreen(){
        foreach(KeyValuePair<string, AudioSource> src in srcs){

            string name = src.Key;
            AudioSource s = src.Value; 

            if (name == "BGM"){
                playBGM = true;
                s.clip = (AudioClip)Resources.Load(pathBGM + "title-theme-lofx");
                RestartSource(s, true, 0.25f, 1.0f);
            }
            if (name == "Cutscene") {
                s.clip = null;
            } 
            if (name == "AmbArea"){
                playAmbArea = false;
                s.clip = null;
            }
            if (name == "AmbMisc") {
                playAmbMisc = false;
                s.clip = null;
            }
        }
    }
    void ToForestStart(){
        foreach(KeyValuePair<string, AudioSource> src in srcs){

            string name = src.Key;
            AudioSource s = src.Value; 

            if (name == "BGM") 
            {   
                playBGM = true;
                s.volume = 0;
                s.clip = Resources.Load<AudioClip>(pathBGM + "forest-theme");
                RestartSource(s, true, 0.33f, 10f);
            }
            if (name == "Cutscene" && !gameStarted)
            {   
                gameStarted = true;
                s.clip = Resources.Load<AudioClip>(pathCutscene + "car-crash-comp");
                s.PlayOneShot(s.clip, 0.8f);
            } 
            if (name == "AmbArea") 
            {
                playAmbArea = true;
                s.clip = Resources.Load<AudioClip>(pathAmb + "Forest/forest_ambience");
                RestartSource(s, true, 0.25f, 10f);
            }
            if (name == "AmbMisc")
            {
                playAmbMisc = true;
                s.clip = Resources.Load<AudioClip>(pathAmb + "creepyambience");     
                RestartSource(s, true, 0.01f, 10f);        
            }  
        }
    }

    void ToBasement(){
        inCabin = true;
        foreach(KeyValuePair<string, AudioSource> src in srcs){

            string name = src.Key;
            AudioSource s = src.Value; 

            if (name == "BGM") 
            {
                playBGM = true;
                //if coming from the starting forest scene, load correct audio file
                if(fromScene == 1){
                    s.clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A1");
                    srcs["BGM2"].Play();
                    srcs["BGM3"].Play();
                    RestartSource(s, true, 0.33f, 1.5f);
               }
            }
            //if we're coming from the forest, cue up the cellar door closing clip         
            if (name == "MiscEntity" && fromScene == 1) 
            {
                s.clip = Resources.Load<AudioClip>(pathInteract + "Door/cabin-door-open-0");
                s.loop = false;
                s.volume = 0.5f;
                s.Play();
            }

            if (name == "Cutscene" )
            {
                s.clip = null;
            } 
            
            if (name == "AmbMisc")
            {
                s.clip = null;
                playAmbMisc = false;
            }

            if (name == "AmbArea") 
            {
                playAmbArea = true;
                s.clip = Resources.Load<AudioClip>(pathAmb + "Cabin/Basement/basement-drips");
                RestartSource(s, true, 0.02f, 1.5f);
            }
        }
    }
    void ToHallway(){
        foreach(KeyValuePair<string, AudioSource> src in srcs){

            string name = src.Key;
            AudioSource s = src.Value; 

            if (name == "AmbMisc") {
                playAmbMisc = false;
                Stop(false, true, s);
                s.clip = null;
            }
            if (name == "AmbArea") {
                playAmbArea = false;
                Stop(false, true, s);
                s.clip = null;
            }
        }
    }
    void RestartSource(AudioSource s, bool fade = false, float targetVolume = 0.25f, float duration = 0.01f, bool stop = false)
    {      
            if (stop) 
            {
                if (fade) 
                {
                    targetVolume = 0f;
                    StartCoroutine(Start(s, targetVolume, duration, stop));
                } else {
                    s.Stop();
                }
            } else {
                if (fade) StartCoroutine(Start(s, targetVolume, duration, stop));
                else {
                    s.volume = targetVolume;
                    s.Play();
                }
            }         
    }

    private static IEnumerator Start(AudioSource audioSource, float targetVolume = 1f, float duration = 3f, bool stop = false)
    {   //taken from https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/ 
        float currentTime = 0;
        float start = audioSource.volume;
        if (!audioSource.isPlaying) audioSource.Play();
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;

        }
        if (stop) audioSource.Stop();
        yield break;
    }

    void Stop(bool all = false, bool fade = false, AudioSource s = null){   
        
        if(all){
            foreach(KeyValuePair<string, AudioSource>  src in srcs) 
            {   
                // if audio source isn't a Cutscene
                if (src.Key != "Cutscene") 
                {   
                    if (fade) { StartCoroutine(Start(s, 0f, 1.5f, true));;
                    } else {src.Value.Stop();}
                       
                }
            } 
        //single audio source
        } else if (s != null) {
            if (fade){ StartCoroutine(Start(s, 0f, 1.5f, true));
            } else {s.Stop();}
        }
    }
       
    void CheckPlayer(){
        PlayerState state = p.GetState();
        AudioSource s = srcs["MiscEntity"];
        AudioClip clip = null;

        //Does the player have a reason to make a sound?
        if(!pA.isPlaying && !p.touchingWall && state != PlayerState.Idle)
        {   Debug.Log("Checking player... State: " + state);

            //if walking or running, play appropriate footfall sfx
            if(state.IsOneOf(PlayerState.Walking, PlayerState.Running)) PlayerFootfall(state);
            
            if (state == PlayerState.Trapped){
                //TO DO: specific scene trapped logic
                // forest: mud trap
                if (scene == 1 || scene == 8)
                {   //Debug.Log("In Forest Trap");

                    if (!trapEntered)
                    {   
                        clip = Resources.Load<AudioClip>(pathInteract + "Traps/Mud/mud-trap-entered-0");
                        //Debug.Log("Checking trap. Is clip null? " + clip == null);
                        s.PlayOneShot(clip, 0.3f);
                        trapEntered = true;
                    } else if (Input.GetKeyDown(Controls.Mash) && !s.isPlaying)
                    {   print("key pressed");
                        clip = Resources.Load<AudioClip>(pathInteract + "Traps/Mud/mud-trap-struggle-" + UnityEngine.Random.Range(0,5).ToString());
                        //Debug.Log("Checking trap. Is clip null? " + clip == null);
                        s.PlayOneShot(clip, 0.3f);
                    }
                
                // hallway: prying board off the door 
                } else if (scene == 3)
                {
                    if (Input.GetKeyDown(Controls.Mash))
                    {   
                        //if this is the last mash, play the final sound
                        if (PlayerPrefs.GetInt("DoorOff") == 1)
                        {   
                            clip = Resources.Load<AudioClip>(pathInteract + "Traps/Plywood/plywood-pull-end");
                            //Debug.Log("Checking trap. Is clip null? " + clip == null);
                            s.PlayOneShot(clip, 0.5f);
                        } 
                        else if (!s.isPlaying)
                        {   
                            clip = Resources.Load<AudioClip>(pathInteract + "Traps/Plywood/plywood-pull-" + UnityEngine.Random.Range(0,8).ToString());
                            //Debug.Log("Checking trap. Is clip null? " + clip == null);
                            s.PlayOneShot(clip, 0.5f);
                        }
                    }
                  //kitchen: walking through the gore  
                } else if (scene == 4 && !s.isPlaying)
                {
                        clip = Resources.Load<AudioClip>(pathInteract + "Traps/Gore/gore-trudge-" + UnityEngine.Random.Range(0,8).ToString());
                        //Debug.Log("Checking trap. Is clip null? " + clip == null);
                        s.PlayOneShot(clip, 0.75f);
                }
            //player is no longer trapped
            } else { trapEntered = false; }
        }
    }

    void PlayerFootfall(PlayerState state)
    {    
        //if running, play a random running footfall
        if (state == PlayerState.Running) pA.PlayOneShot(p.footstepsRun[UnityEngine.Random.Range(0, p.footstepsRun.Capacity)]);
            
        //if walking, play a random walking footfall
         if (state == PlayerState.Walking && p.GetAnimationIndex().IsOneOf(2, 11)) pA.PlayOneShot(p.footstepsWalk[UnityEngine.Random.Range(0, p.footstepsWalk.Capacity)], 0.25f);      
    }  

    private float GetTimeRemaining(AudioSource s) 
    {   //get the amount of time left in the currently playing clip
        float len = s.clip.length;
        return len - s.time;
    }

    private IEnumerator DelayedStart(AudioSource s, AudioClip clip, float waitTime, bool oneshot = false)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        
        if(oneshot) {
            s.PlayOneShot(clip);
        } else {
            s.loop = true;
            s.clip = clip;
            s.Play();
        }
    }
}