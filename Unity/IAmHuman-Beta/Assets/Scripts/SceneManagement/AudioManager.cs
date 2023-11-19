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
    private int[] woodFloors = {4, 5, 6, 7, 8};


    //resource paths
    private string      pathBGM = "Sounds/Music/";
    private string      pathAmb = "Sounds/SoundEffects/Environment/";
    private string pathCutscene = "Sounds/SoundEffects/Misc/";
    private string   pathEntity = "Sounds/SoundEffects/Entity/";


    //BGM management
    private bool      playBGM = true;
    private bool  playNextBGM = false;
    private AudioSource nextBGM;
    //other bools
    private bool  playAmbArea = false;
    private bool  playAmbMisc = false;
    private bool playCutscene = false;
    private bool       fading = false;


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
        {   Debug.Log("player lol");
            p = Player.Instance;
            pA = p.AudioSource;
        }

        //check for scene change
        if (scene != SceneManager.GetActiveScene().buildIndex)
        {   
            fromScene = scene;
            scene = SceneManager.GetActiveScene().buildIndex;
            ChangeScene(scene);
        }
        

        CheckPlayer(p.GetState());
        CheckScenewide(scene);
    }

    void ChangeScene(int scene)
    {   
        while(fading);
        //Set scene-wide audio sources
        switch (scene){

            //Title Screen
            case 0: 
                Stop(true, true);
                ToTitleScreen();
                break;

            //Forest Intro
            case 2: 
                // fadeout all audio if coming from the title screen
                Stop(true, true);
                ToForestStart();
                break;
            
            //Basement
            case 3: 
                if(fromScene == 2) Stop(true, true);
                ToBasement();
                break;
            //Hallway
            case 4: 
                ToHallway();
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
                playCutscene = false;
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
                s.clip = Resources.Load<AudioClip>(pathBGM + "forest-theme");
                RestartSource(s, true, 0.33f, 15f);
            }
            if (name == "Cutscene")
            {   
                playCutscene = true;
                s.clip = Resources.Load<AudioClip>(pathCutscene + "car-crash-comp");
                s.PlayOneShot(s.clip, 0.8f);
            } 
            if (name == "AmbArea") 
            {
                playAmbArea = true;
                s.clip = Resources.Load<AudioClip>(pathAmb + "Forest/forest_ambience");
                RestartSource(s, true, 0.25f, 15f);
            }
            if (name == "AmbMisc")
            {
                playAmbMisc = true;
                s.clip = Resources.Load<AudioClip>(pathAmb + "creepyambience");     
                RestartSource(s, true, 0.03f, 15f);        
            }  
        }
    }

    void ToBasement(){
        foreach(KeyValuePair<string, AudioSource> src in srcs){

            string name = src.Key;
            AudioSource s = src.Value; 

            if (name == "BGM") {
                playBGM = true;
                nextBGM = src["BGM2"];
                //if coming from the starting forest scene, load correct audio file
                //if(fromScene == 2){
                    s.clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A1");
                    RestartSource(s, true);
                    playNextBGM = true;
               // }
            }
            //if we're coming from the forest, cue up the cellar door closing clip 
            if (name == "Cutscene" && fromScene == 2) s.clip = Resources.Load<AudioClip>(pathEntity + "Interactable/Door/cellar-door-close-0");
            
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
            if (name == "BGM") 
            { 
                //if we're playing the first section
                if (s.clip.name == "cabin-theme-A1")
                {   
                    playBGM = true;
                    //calculate the time remaining in the clip
                    float remaining = GetTimeRemaining(s);
                    s.loop = false;
                    //load the next section and play it after current clip is finished
                    AudioClip clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A2");
                    StartCoroutine(DelayedStart(s, clip, remaining, true));
                }

            }
            if (name == "AmbArea") {
                playAmbArea = false;
                Stop(false, true, s);
                s.clip = null;
            }
        }
    }
    void RestartSource(AudioSource s, bool fade = false, float targetVolume = 0.25f, float duration = 0.01f){

            s.volume = fade ? 0.01f : targetVolume;
            if (s.isPlaying) s.Stop();
            s.Play();
            if (fade) StartCoroutine(Start(s, targetVolume, duration));
    }

    private static IEnumerator Start(AudioSource audioSource, float targetVolume = 1f, float duration = 3f)
    {   //taken from https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/ 
        _instance.fading = true;
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        _instance.fading = false;
        yield break;
    }

    void Stop(bool all = false, bool fade = false, AudioSource s = null){   
        
        if(all){
            foreach(KeyValuePair<string, AudioSource>  src in srcs) 
            {   
                // if audio source isn't a Cutscene
                if (src.Key != "Cutscene") 
                {   
                    if (fade) {RestartSource(src.Value, true, 0.001f, 1.5f);
                    } else {src.Value.Stop();}
                       
                }
            } 
        } else if (s != null) {
            if (fade){ RestartSource(s, true, 0.001f, 1.5f);
            } else {s.Stop();}
        }
    }      
    void CheckScenewide(int scene){   

        //Only resume other audio if cutscene isn't playing
        if (!srcs.TryGetValue("Cutscene", out AudioSource cutscene) || !cutscene.isPlaying) {
            //check background music
            if (srcs.TryGetValue("BGM", out AudioSource bgm))
            {    
                // if bgm isn't already playing, has an audio clip, and global is set, then play
                if (!bgm.isPlaying && bgm.clip != null && playBGM) RestartSource(bgm);

                // if bgm is playing and shouldn't be, stop it
                if (bgm.isPlaying && !playBGM) bgm.Stop();

                // if bgm is playing and should be, but the volume is too low, restart it.
                if (bgm.isPlaying && playBGM && bgm.volume < 0.01f) RestartSource(bgm);

                if (playNextBGM) playNext(bgm);
            }
            //check area ambience
            if (srcs.TryGetValue("AmbArea", out AudioSource ambArea))
            {    
                // if ambArea isn't already playing, has an audio clip, and global is set, then play
                if (!ambArea.isPlaying && ambArea.clip != null && playBGM) RestartSource(ambArea);

                // if ambArea is playing and shouldn't be, stop it
                if (ambArea.isPlaying && !playAmbArea) ambArea.Stop();

                // if ambArea is playing and should be, but the volume is too low, restart it.
                if (ambArea.isPlaying && playAmbArea && ambArea.volume < 0.01f) RestartSource(ambArea);
            }
            //check general ambience
            if (srcs.TryGetValue("AmbMisc", out AudioSource ambMisc))
            {    
                // if ambMisc isn't already playing, has an audio clip, and global is set, then play
                if (!ambMisc.isPlaying && ambMisc.clip != null && playBGM) RestartSource(ambMisc);

                // if ambMisc is playing and shouldn't be, stop it
                if (ambMisc.isPlaying && !playAmbMisc) ambMisc.Stop();

                // if ambMisc is playing and should be, but the volume is too low, restart it.
                if (ambMisc.isPlaying && playAmbMisc && ambMisc.volume < 0.01f) RestartSource(ambMisc);
            }
        }
    }
       
    void CheckPlayer(PlayerState state){   

        //Does the player have a reason to make a sound?
        if(!pA.isPlaying && !p.touchingWall && state != PlayerState.Idle)
        {   
            //if walking or running, play appropriate footfall sfx
            if(state.isOneOf(PlayerState.Walking, PlayerState.Running)) PlayerFootfall(state);
            
            //if pushing an obeject, play the push loop starting at a random point in the file
            if(state.isOneOf(PlayerState.Pushing, PlayerState.Pulling)){
                pA.clip = Resources.Load<AudioClip>(pathEntity + "Interactable/push-pull-loop");
                pA.time = Random.Range(0, pA.clip.length/1);
                pA.volume = 0.15f;
                pA.Play();
            }
            
            if (state == PlayerState.Trapped){
                pA.PlayOneShot(Resources.Load<AudioClip>(pathEntity + "/Interactable/mud-trap-entered-0"), 0.5f);
                 
            }
        }
        // if no longer pushing/pulling, stop the push audio
        if (!state.isOneOf(PlayerState.Pushing, PlayerState.Pulling) && pA.isPlaying && pA.clip.name == "push-pull-loop") pA.Stop();
    }
    void PlayerFootfall(PlayerState state){
        
        //if running, play a random running footfall
        if (state == PlayerState.Running) pA.PlayOneShot(p.footstepsRun[UnityEngine.Random.Range(0, p.footstepsRun.Capacity)]);
            
        //if walking, play a random walking footfall
         if (state == PlayerState.Walking && p.GetAnimationIndex().isOneOf(2, 11)) pA.PlayOneShot(p.footstepsWalk[UnityEngine.Random.Range(0, p.footstepsWalk.Capacity)], 0.25f);      
    }  

    private float GetTimeRemaining(AudioSource s) 
    {   //get the amount of time left in the currently playing clip
        float len = s.clip.length;
        return len - s.time;
    }

    private IEnumerator DelayedStart(AudioSource s, AudioClip clip, float waitTime, bool oneshot = false){
        yield return new WaitForSecondsRealtime(waitTime);
        
        if(oneshot) {
            s.PlayOneShot(clip);
        } else {
            s.loop = true;
            s.clip = clip;
            s.Play();
        }
    }

    void loadNext(AudioSource s){
        //This method is mainly just for switching between the sections of the cabin theme, which is why the logic is hard coded in atm
        Debug.Log("s.clip.name is " + s.clip.name);
        if (s.clip.name == "cabin-theme-A2") //A2 is a one shot, need to play A3 as soon as it's done
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/Music/cabin-theme-A3");
            float remaining = GetTimeRemaining(s);
            StartCoroutine(DelayedStart(s, clip, remaining));
            loadNextBGM = false;
        }
    }

}