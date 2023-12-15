using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    //self
    private static AudioManager _instance;
    public  static AudioManager Instance{get{return _instance;} }

    //player
    private Player p;
    private AudioSource pA;
    
    //scene
    private SceneManager SceneManager;
    private int scene;
    private int fromScene; 
    private int[] woodFloors = {2, 3, 4, 5, 7};

    //resource paths
    private string       pathBGM = "Sounds/Music/";
    private string       pathAmb = "Sounds/SoundEffects/Environment/";
    private string    pathEntity = "Sounds/SoundEffects/Entity/";
    private string  pathInteract = "Sounds/SoundEffects/Entity/Interactable/";
    private string  pathCutscene = "Sounds/SoundEffects/Misc/";
   
    //other flags
    private bool  trapEntered = false;
    private bool fading = false;
    
    //progression flags
    public bool       deathTriggered = false;
    public bool            inKitchen = false;
    public bool     kitchenTriggered = false;
    public bool crowbarFadeTriggered = false;
    public bool          gameStarted = false;
    private bool  monsterTransformed = false;
    
    //AudioSources
    private Dictionary<string, AudioSource> srcs;
    private AudioSource ambArea;
    private AudioSource ambMisc;
    private AudioSource bgm;
    private AudioSource bgm2;
    private AudioSource bgm3;
    private AudioSource cutscene;
    private AudioSource miscEntity;

    //Audio Floats
    public float          bgmVolume = 0.5f;
    public float areaAmbienceVolume = 0.02f;
    public float miscAmbienceVolume = 0.02f;
    public float    defaultFadeTime = 2.5f;

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
            PopulateAudioSources();
        }

        //set scene
        scene = SceneManager.GetActiveScene().buildIndex;
        if (scene == 0) 
        {   
            ToTitleScreen();
        } else {
            ChangeScene(scene);
        }   
    }
    // Update is called once per frame
    void Update(){   
        //check if player exists
        if (p == null && Player.Instance != null)
        {   
            p = Player.Instance;
            pA = p.gameObject.GetComponent<AudioSource>();
            SetPlayerFootFalls();
        }

        //check for scene change
        if (scene != SceneManager.GetActiveScene().buildIndex)
        {   
            fromScene = scene;
            scene = SceneManager.GetActiveScene().buildIndex;
            ChangeScene(scene);
        }

        if (p != null) CheckPlayer();
        //are we dead?
        CheckDeath();
        CheckProgress(scene);
        CheckScenewide(scene);
    }
    private void CheckProgress(int scene){   
        //if we're not in the title screen
        if(scene != 0) 
        {   
            //if we are inside the cabin
            if (scene != 1 && scene != 9)
            {   
                if (PlayerPrefs.GetInt("Crowbar") == 1)
                {
                    if(!crowbarFadeTriggered)
                    {   
                        //fade in distorted version and fade out currently playing version
                        StartCoroutine(Start(bgm2, bgmVolume, 0.75f));
                        StartCoroutine(Start(bgm, 0f, 0.75f, true));
                        crowbarFadeTriggered = true;
                    }
                } else {
                    crowbarFadeTriggered = false;
                }
                //if you have entered the kitchen, play distortion lvl 2
                if (inKitchen && !kitchenTriggered)
                {   
                    kitchenTriggered = true;
                    bgm.clip = null;
                    //fade in distorted version and fade out currently playing version
                    StartCoroutine(Start(bgm3, bgmVolume, 0.75f));
                    StartCoroutine(Start(bgm2, 0f, 0.75f, true));
                    crowbarFadeTriggered = true;
                }
            }
            if (scene == 8 && !monsterTransformed && scene != 9 && Input.GetKeyDown(KeyCode.Return))
            {
                monsterTransformed = true;
                cutscene.loop = false;
                cutscene.clip = Resources.Load<AudioClip>(pathEntity + "Monster/monster-transform");
                cutscene.volume = 0.5f;
                cutscene.Play();
            }
        }
    }
    void CheckDeath(){
        if (PlayerPrefs.GetInt("Dead") == 1)
        {   

            if (!deathTriggered)
            {   
                Stop(true, false);
                deathTriggered = true;
            }
            
            monsterTransformed = false;
            
            //play death screen music if not already
            if (!bgm.isPlaying || bgm.clip == null || bgm.clip.name != "death-music")
            {
                bgm.volume = 0f;
                bgm.clip = Resources.Load<AudioClip>(pathBGM + "death-music");
                RestartSource(bgm, true, 1.0f, 12f);
            }
        } else if (PlayerPrefs.GetInt("Dead") == 0) deathTriggered = false;
    }
    void CheckScenewide(int scene){   

        //Only resume other audio if cutscene isn't playing, audio isn't already fading in/out, and we're not dead
        if (!cutscene.isPlaying && !fading && !deathTriggered) { 

            //Check scene-wide audio sources 
            if (scene == 1) CheckForest();
            if (scene.IsOneOf(2, 3, 4, 5, 6)) CheckCabin(scene);
            if (scene == 7) CheckAtticStairwell();
            if (scene == 8) CheckAttic();
            if (scene == 9) CheckChase();
        }
    }
    void CheckForest(){
        if (    bgm.clip == null ||     bgm.clip.name != "forest-theme")        bgm.clip = Resources.Load<AudioClip>(pathBGM + "forest-theme");
        if (ambArea.clip == null || ambArea.clip.name != "forest_ambience") ambArea.clip = Resources.Load<AudioClip>(pathAmb + "Forest/forest_ambience");
        if (ambMisc.clip == null || ambMisc.clip.name != "creepyambience")  ambMisc.clip = Resources.Load<AudioClip>(pathAmb + "creepyambience");

        if     (!bgm.isPlaying) RestartSource(bgm3,    true, bgmVolume, defaultFadeTime);
        if (!ambArea.isPlaying) RestartSource(ambArea, true, bgmVolume, defaultFadeTime);
        if (!ambMisc.isPlaying) RestartSource(ambMisc, true, bgmVolume, defaultFadeTime);
    }
    void CheckCabin(int scene){
        //check for correct clips
        if ( bgm.clip == null ||  bgm.clip.name != "cabin-theme-A1") bgm.clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A1");
        if (bgm2.clip == null || bgm2.clip.name != "cabin-theme-A2") bgm.clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A2");
        if (bgm3.clip == null || bgm3.clip.name != "cabin-theme-A3") bgm.clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A3");

        //check game state -- have we picked up the crowbar?
        if (PlayerPrefs.GetInt("Crowbar") == 1)
        {   
            if (PlayerPrefs.GetInt("AtticKey") == 1 || kitchenTriggered) //have we entered the kitchen?
            {   
                //play the extra distorted cabin theme
                if (!fading) bgm3.volume = bgmVolume;
                if (bgm.isPlaying)   bgm.Stop();
                if (bgm2.isPlaying) bgm2.Stop();
                if (!bgm3.isPlaying) RestartSource(bgm3, true, bgmVolume, defaultFadeTime);
            } else {
                //play the distorted cabin theme
                bgm3.volume = 0;
                if (bgm.isPlaying)   bgm.Stop();
                if (!bgm2.isPlaying) RestartSource(bgm2, true, bgmVolume, defaultFadeTime);
                if (!bgm3.isPlaying) bgm3.Play();
            } 
            if (scene == 4){ //are we currently in the kitchen?
                if (ambArea.clip == null || ambArea.clip.name != "gore-body-push") ambArea.clip = Resources.Load<AudioClip>(pathInteract + "gore-body-push");
            }
        } else {

            //play the normal cabin music
            bgm2.volume = 0;
            bgm3.volume = 0;
            if (!bgm.isPlaying) RestartSource(bgm, true, bgmVolume, defaultFadeTime);;
            if (!bgm2.isPlaying) bgm2.Play();
            if (!bgm3.isPlaying) bgm3.Play();
        }
        if (scene == 3){
            if (ambMisc.isPlaying) Stop(false, true, ambMisc);
            if (ambArea.isPlaying) Stop(false, true, ambArea);
        }
    }
    void CheckAtticStairwell(){
        bgm.Pause();
        bgm3.Pause();
        bgm2.Pause();

        if (ambMisc.clip == null || ambMisc.clip.name != "creepyambience") ambMisc.clip = Resources.Load<AudioClip>(pathAmb + "creepyambience");
        if (!ambMisc.isPlaying){
            ambMisc.volume = 0.001f;
            RestartSource(ambMisc, true, 0.01f, 3f);
        }
    }
    void CheckAttic(){
        if (bgm.clip == null || bgm.clip.name != "Monster_Chase_Full") bgm.clip = Resources.Load<AudioClip>(pathBGM + "Monster_Chase_Full");
        if (!bgm.isPlaying) RestartSource(bgm, true, bgmVolume, defaultFadeTime);
    }
    void CheckChase(){
        if (bgm.clip == null || bgm.clip.name != "Monster_Chase_Full") bgm.clip = Resources.Load<AudioClip>(pathBGM + "Monster_Chase_Full");
        if (!bgm.isPlaying){
            bgm.volume = 0.01f;
            RestartSource(bgm, true, bgmVolume, defaultFadeTime);
        }
    }
    void ChangeScene(int scene){   
        //if coming from the title screen, fade out all audio
        if (fromScene == 0) Stop(true, true);

        //Set player footfall samples to correct material
        SetPlayerFootFalls();
        
        //Set scene-wide audio sources
        switch (scene){
            //Title Screen
            case 0: 
                // fade out all other audio when returning to title screen
                Stop(true, true);
                ToTitleScreen();
                break;

            //Forest Intro
            case 1: 
                Stop(true, true);
                ToForestStart();
                break;
            
            //Basement
            case 2:
                //Stop music if entering from the forest
                if(fromScene == 1) Stop(true, true);
                ToBasement();
                break;

            //Attic Stairwell
            case 6:
                ToAtticStairs();
                break;

            //Attic
            case 7:
                ToAttic();
                break;
        }
    }
    void ToTitleScreen(){

        bgm.clip = (AudioClip)Resources.Load("Sounds/Music/title-theme-lofx");
        RestartSource(bgm, true, bgmVolume, defaultFadeTime);

        cutscene.clip = null;
        ambArea.clip = null;
        ambMisc.clip = null;
    }
    void ToForestStart(){

            bgm.clip = Resources.Load<AudioClip>(pathBGM + "forest-theme");
        ambArea.clip = Resources.Load<AudioClip>(pathAmb + "Forest/forest_ambience");
        ambMisc.clip = Resources.Load<AudioClip>(pathAmb + "creepyambience");  

        //have we played the intro cutscene?
        if(!gameStarted && fromScene == 0){
            gameStarted = true;
            cutscene.clip = Resources.Load<AudioClip>(pathCutscene + "car-crash-update");
            cutscene.PlayOneShot(cutscene.clip, 0.8f);

            //play background music and ambience, fading in over 10 seconds
            bgm.volume = 0;
            RestartSource(bgm, true, bgmVolume, 10f);
            RestartSource(ambArea, true, 0.25f, 10f);
            RestartSource(ambMisc, true, miscAmbienceVolume/2f, 10f);       
        } else{

            RestartSource(bgm, true, bgmVolume, defaultFadeTime);
            RestartSource(ambArea, true, 0.25f, defaultFadeTime);
            RestartSource(ambMisc, true, miscAmbienceVolume/2f, defaultFadeTime);  
        }      
    }
    void ToBasement(){

        //if coming from the starting forest scene, load correct audio file
        if(fromScene.IsOneOf(0, 1)){
            bgm.clip = Resources.Load<AudioClip>(pathBGM + "cabin-theme-A1");
            bgm2.Play();
            bgm3.Play();
            RestartSource(bgm, true, bgmVolume, defaultFadeTime);
        }
        //if we're coming from the forest, cue up the cellar door closing clip         
        if (fromScene == 1) 
        {
            miscEntity.volume = 0.5f;
            miscEntity.PlayOneShot(Resources.Load<AudioClip>(pathInteract + "Door/cabin-door-open-0"));
        }

        ambArea.clip = Resources.Load<AudioClip>(pathAmb + "Cabin/Basement/basement-drips");
        RestartSource(ambArea, true, areaAmbienceVolume, defaultFadeTime);      
    } 
    void ToAtticStairs(){
        bgm.Pause();
        bgm3.Pause();
        bgm2.Pause();

        ambMisc.clip = Resources.Load<AudioClip>(pathAmb + "creepyambience");
        ambMisc.volume = 0.001f;
        ambMisc.Play();
        RestartSource(ambMisc, true, 0.01f, 3f);
    }
    void ToAttic(){ //Do we want the music to play automatically, or should it happen when the transformation starts?
        bgm.clip = Resources.Load<AudioClip>(pathBGM + "Monster_Chase_Full");
        bgm.volume = 0.01f;
        RestartSource(bgm, true, bgmVolume, defaultFadeTime);
    }
    void RestartSource(AudioSource s, bool fade = false, float targetVolume = 0.5f, float duration = 2.5f, bool stop = false){      
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
                if (fade) 
                {
                    if (!s.isPlaying) miscEntity.Play();
                    StartCoroutine(Start(s, targetVolume, duration, stop));
                }
                else {
                    s.volume = targetVolume;
                    miscEntity.Play();
                }
            }         
    }
    private static IEnumerator Start(AudioSource audioSource, float targetVolume = 1f, float duration = 3f, bool stop = false){   //taken from https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/ 
        if (audioSource == null)
        {
            yield break;
        }
        float currentTime = 0;
        float start = audioSource.volume;
        if (!audioSource.isPlaying) audioSource.Play();
        while (currentTime < duration)
        {   
            _instance.fading = true;
            
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;

        }
        if (stop)
        {   
            yield return new WaitUntil(() => audioSource.volume >= 0.001f);
            audioSource.Stop();
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
                    if (fade) { StartCoroutine(Start(s, 0f, defaultFadeTime, true));;
                    } else {src.Value.Stop();}
                       
                }
            } 
        //single audio source
        } else if (s != null) {
            if (fade){ StartCoroutine(Start(s, 0f, defaultFadeTime, true));
            } else s.Stop();
        }
    }  
    void CheckPlayer(){
        
        AudioClip clip;
        PlayerState state = p.GetState();

        //Does the player have a reason to make a sound?
        if(!pA.isPlaying && !p.touchingWall && state != PlayerState.Idle) {
           
           //if walking or running, play appropriate footfall sfx
            if(state.IsOneOf(PlayerState.Walking, PlayerState.Running)) PlayerFootfall(state);
            
            if (state == PlayerState.Trapped){

                // forest: mud trap
                if (scene == 1 || scene == 8)
                {   
                    if (!trapEntered)
                    {   
                        clip = Resources.Load<AudioClip>(pathInteract + "Traps/Mud/mud-trap-entered-0");
                        miscEntity.PlayOneShot(clip, 0.3f);
                        trapEntered = true;
                    } else if (Input.GetKeyDown(Controls.Mash) && !miscEntity.isPlaying)
                    {   
                        clip = Resources.Load<AudioClip>(pathInteract + "Traps/Mud/mud-trap-struggle-" + UnityEngine.Random.Range(0,5).ToString());
                        miscEntity.PlayOneShot(clip, 0.3f);
                    }
                
                // hallway: prying board off the door 
                } else if (scene == 3){
                
                    if (Input.GetKeyDown(Controls.Mash))
                    {   
                        //if this is the last mash, play the final sound
                        if (PlayerPrefs.GetInt("DoorOff") == 1)
                        {   
                            clip = Resources.Load<AudioClip>(pathInteract + "Traps/Plywood/plywood-pull-end");
                            miscEntity.PlayOneShot(clip, 0.75f);
                        } 
                        else if (!miscEntity.isPlaying)
                        {   
                            clip = Resources.Load<AudioClip>(pathInteract + "Traps/Plywood/plywood-pull-" + UnityEngine.Random.Range(0,8).ToString());
                            miscEntity.PlayOneShot(clip, 0.75f);
                        }
                    }
                //kitchen: walking through the gore  
                } else if (scene == 4 && !miscEntity.isPlaying)
                {
                        clip = Resources.Load<AudioClip>(pathInteract + "Traps/Gore/gore-trudge-" + UnityEngine.Random.Range(0,8).ToString());
                        miscEntity.PlayOneShot(clip, 0.75f);
                }
            //player is no longer trapped
            } else trapEntered = false;  

            //is player hiding??
            if (state == PlayerState.Hiding && !miscEntity.isPlaying){
                //set appropriate heartbeat intensity according to progression
                if (PlayerPrefs.GetInt("AtticKey") == 1){ 
                    clip = Resources.Load<AudioClip>(pathInteract + "heartbeat-fast");
                } else if (PlayerPrefs.GetInt("CrowBar") == 1){ 
                    clip = Resources.Load<AudioClip>(pathInteract + "heartbeat-med");
                } else {
                    clip = Resources.Load<AudioClip>(pathInteract + "heartbeat-slow");
                }
                miscEntity.clip = clip;
                miscEntity.time = UnityEngine.Random.Range(0, clip.length);
                miscEntity.PlayOneShot(clip);
            }
        }
        //stop hiding sound if player is no longer hiding
        if (state != PlayerState.Hiding && miscEntity.isPlaying) {
            if (miscEntity.clip.name.IsOneOf("heartbeat-fast", "heartbeat-med", "heartbeat-slow")) miscEntity.Stop();
        }
    }
    void SetPlayerFootFalls(){
        if (woodFloors.Contains(scene))
        {
            p.SetFootfalls(pathEntity+"Player/Footfalls/Wood/Walk", pathEntity+"Player/Footfalls/Wood/Run");
        } else {
            p.SetFootfalls(pathEntity+"Player/Footfalls/Dirt/Walk", pathEntity+"Player/Footfalls/Dirt/Run");
        }
    }
    void PlayerFootfall(PlayerState state){    
        //if running, play a random running footfall
        if (state == PlayerState.Running && p.GetAnimationIndex().IsOneOf(2, 7)) 
        {
            pA.PlayOneShot(p.footstepsRun[UnityEngine.Random.Range(0, p.footstepsRun.Capacity)]);
        }   
        //if walking, play a random walking footfall
        if (state == PlayerState.Walking && p.GetAnimationIndex().IsOneOf(2, 11)) 
        {
            pA.PlayOneShot(p.footstepsWalk[UnityEngine.Random.Range(0, p.footstepsWalk.Capacity)], 0.25f);      
        }
    }  
    // used when player dies
    public void StopAllSources()
    {
        foreach (AudioSource src in srcs.Values)
        {
            src.Stop();
        }
    }
    private void PopulateAudioSources(){

        foreach(KeyValuePair<string, AudioSource> src in srcs)
        {
            string name = src.Key;
            AudioSource s = src.Value;
            if(name == "BGM") bgm = s;
            if(name == "BGM2") bgm2 = s;
            if(name == "BGM3") bgm3 = s;
            if(name == "AmbArea") ambArea = s;
            if(name == "AmbMisc") ambMisc = s;
            if(name == "MiscEntity") miscEntity = s;
            if(name == "Cutscene") cutscene = s;
        }
    }
}