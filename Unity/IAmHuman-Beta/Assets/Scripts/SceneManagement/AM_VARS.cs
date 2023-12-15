using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace am_vars{

    public class AM_VARS : MonoBehaviour
    {   
        //player
        private Player p;
        private AudioSource pA;

        private static AudioManager _instance;
        public  static AudioManager Instance{ get{ return _instance; }}

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

        //audioSources
        private Dictionary<string, AudioSource> srcs;
        private AudioSource ambArea;
        private AudioSource ambMisc;
        private AudioSource bgm;
        private AudioSource bgm2;
        private AudioSource bgm3;
        private AudioSource cutscene;
        private AudioSource extern1;
        private AudioSource extern2;
        private AudioSource extern3;

        //BGM management flags
        private bool      playBGM = true;
        private bool     playBGM2 = false;
        private bool     playBGM3 = false;

        //other flags
        private bool  playAmbArea = false;
        private bool  playAmbMisc = false;
        private bool  trapEntered = false;

        //progression flags
        public bool         diaryChecked = false;
        public bool              inTitle = true;
        public bool              inCabin = false;
        public bool            inKitchen = false;
        public bool              inAttic = false;
        public bool     kitchenTriggered = false;
        public bool crowbarFadeTriggered = false;
        public bool          gameStarted = false;
        private bool  monsterTransformed = false;
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
