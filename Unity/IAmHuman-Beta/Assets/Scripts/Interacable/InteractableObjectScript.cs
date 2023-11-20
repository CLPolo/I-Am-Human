﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.PlayerLoop;

public class InteractableObjectScript : MonoBehaviour
{

    public LogicScript logic;
    public Player player;

    [Header("Sprites")]
    public Sprite Default = null;
    public Sprite Outline = null;
    public bool removeSprite = false;  // used for kitchen drawer

    [Header("Interaction")]
    public GameObject PromptTextObject = null;  // this is the screen that displays prompt to interact
    public GameObject PromptCanvas = null;  // NOTE: need to seperate text object and entire canvas as there are two text object on the canvas
    public String InteractMessage;
    public Boolean ShowPromptOnce;

    [Header("Dialogue Display")]
    public GameObject DialogueTextObject = null;  // this is the screen that displays info about object post interaction
    public GameObject DialogueCanvas = null;  // NOTE: need to seperate text object and entire canvas as there are two text object on the canvas
    public List<String> TextList;

    [Header("Freeze Player")]
    public bool FreezePlayer = false;

    [Header("Play Sound")]
    public bool PlaySound = false;
    public List<AudioClip> Sounds;
    public AudioSource Audio;

    [Header("Unlock Door")]
    public bool Unlock;
    public string NextScene = null;

    [Header("Spawn Object (or enemy) Post Interact")]
    public bool SpawnObject = false;
    public bool SpawnEntity = false;
    public List<GameObject> ObjectsSpawned = null;
    public List<GameObject> EntitySpawn = null;

    // Booleans
    private int textIndex = 0;
    private bool Inside = false;  // if player is inside hit box for object
    private bool HasInteracted = false;  // if player interacted with object
    private bool CurrentlyPlaying = false;  // if flashback is currently playing through
    private bool NoisePlayed = false;
    private bool TextHasPlayed = false;  // makes it so that text doesn't play if press E again while still in collider
    private bool PressedInteract = false;
    private bool isPickup = false;  // will only handle pickup stuff (like turning off object) if the interaction is happening with a pickup.
    private bool triggered = false;

    // Below Should be removed and placed into monster script
    public bool monsterComing = false;  // This is for activating Monster.cs
    


    // Start is called before the first frame update
    void Start()
    {
        logic = LogicScript.Instance;
        player = Player.Instance;
        

        RemovePickedUpObjects();
        PlayerPrefs.SetInt("escaped", 0);

        // turns off all screens
        if (PromptCanvas != null)
        {
            PromptCanvas.SetActive(false);
        }
        if (DialogueCanvas != null)
        {
            DialogueCanvas.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)  // when player assigned on start, but room is left and returned to (hallway hub for example), throws an error as that player version was destroyed. 
        {
            player = Player.Instance;
        }
        if (Audio == null) Audio = GetComponent<AudioSource>();
        if (Inside && Input.GetKey(Controls.Interact) && PlayerPrefs.GetString("CollisionTagInteractable") == "Player") // if player is inside the interactable object's box collider
        {
            PressedInteract = true;
            PromptCanvas.SetActive(false);  // turns off 'interact' prompt
            CheckAndDisplayInfo();  // checks if there's info to display, if so does that
            if (PlaySound && !NoisePlayed)  // used rn for cabin door sounds
            {
                PlayNoise();
                NoisePlayed = true;
            }
            if (Unlock && !triggered) // triggered prevented a billion calls to level loader during the fade out, which prevented the positioning stuff from working properly
            {
                triggered = true;
                UnlockDoor(NextScene);
            }
            if (removeSprite)
            {
                RemoveSprite();
            }
            RemoveOutline(); // removes outline when player has interacted before they exit collider again to remove confusion
            PickupSet();
            if (SpawnObject || SpawnEntity) { SpawnObjectCheck(); } // spawns entity objects
        }
        if (PressedInteract) { CheckAndDisplayInfo(); }  // displays info even if outside of collider, only needed if not frozen
        CheckPickupInteractions();
    }

    // if they press InteractKey to interact & haven't already interacted with object and we only want the interact prompt to appear once

    private void OnTriggerEnter2D(Collider2D col)
    {
        PlayerPrefs.SetString("CollisionTagInteractable", col.tag);  // Allows us to prevent the enemy from triggering items (like doors & outlines)

        if (PlayerPrefs.GetString("CollisionTagInteractable") == "Player")
        {
            if (Inside = true && !Input.GetKeyDown(Controls.Interact))  // if player in collider and has NOT pressed interact key yet
            {
                if (Unlock && this.GetComponent<Door>().IsInteractable == true)
                {
                    DisplayInteractPrompt();  // shows the interact prompt
                }
                else if (!Unlock)
                {
                    DisplayInteractPrompt();
                }
            }
            DisplayOutline();  // when in collider, displays outline on obj
            PickupCheck();
        }  
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        // if player is not actively inside collider, turns off interact prompt

        Inside = false;
        Interactable();
        NoisePlayed = false;
        TextHasPlayed = false;
        RemoveOutline();  // removes outline of sprite once no longer in range (collider)
        PressedInteract = false;
        isPickup = false;
    }

    private void PickupCheck()
    {
        // checks if object being interacted with is a pickup

        if (this.gameObject.tag.isOneOf("Flashlight", "Crowbar", "AtticKey"))  // if one of our pickups, will give us usable string for tag.
        {
            PlayerPrefs.SetString("Pickup", this.tag);
            isPickup = true;

        }
    }

    private void PickupSet()
    {
        // sets appropriate player pref to reflect that pickup has been grabbed

        if (isPickup && PlayerPrefs.GetString("Pickup").isOneOf("Flashlight", "Crowbar", "AtticKey")) // if one of our pickups, it will set playerprefs of that to 1 (true).
        {
            PlayerPrefs.SetInt(PlayerPrefs.GetString("Pickup"), 1);
            this.gameObject.SetActive(false);
            AudioClip clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/item-pickup");
            player.AudioSource.PlayOneShot(clip, 0.5f);
            if (this.tag == "Flashlight")
            {
                clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/Door/cellar-door-close-0");
                player.AudioSource.clip = clip;
                player.AudioSource.PlayOneShot(clip);
            }
        }

    }

    private void RemovePickedUpObjects()
    {
        // ensures they won't spawn on re-entry of a room if they were picked up
        if (this.gameObject.tag.isOneOf("Flashlight", "Crowbar", "AtticKey"))
        {
            if (PlayerPrefs.GetInt(this.gameObject.tag) == 1)
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    private void PlayNoise()
    {
        // plays a random sound from the sound list each time

        AudioSource audio = GetComponent<AudioSource>();
        int randomNumber = UnityEngine.Random.Range(0, Sounds.Count);
        audio.PlayOneShot(Sounds.ElementAt(randomNumber));
    }

    private void DisplayInteractPrompt()
    {
        if (!CurrentlyPlaying && (!HasInteracted || !ShowPromptOnce))  // if they haven't already interacted and they aren't limited to interacting once only, and not currently playing
        {
            // displays interact text
            if (PromptTextObject != null)
            {
                PromptTextObject.SetActive(true);
                PromptTextObject.GetComponent<TextMeshProUGUI>().text = InteractMessage;
            }
            Interactable();  // turns on the interact screen
        }
    }

    private void CheckAndDisplayInfo()
    {
        // if there's info to display it will display it
        if (DialogueCanvas != null && TextList != null && TextList.Count > 0 && !TextHasPlayed)
        {
            DisplayText();
        }
        HasInteracted = true;  // Show interact prompt only once
    }

    private void Interactable()
    {
        // turns interact prompt on or off depending on whether player is inside
        PromptCanvas.SetActive(Inside);
    }

    private void DisplayText()
    {
        // click through version for text interaction. Will remove auto timed version from script & clean it up after
        // we've confirmed it's not being used and that click thru will be default. I also will rename the variables.
    
        if (TextList != null)  // if there is a list of text
        {
            if (FreezePlayer) { player.SetState(PlayerState.Frozen); }  // freezes player until they've worked thru dialogue

            DialogueCanvas.SetActive(true);
            DialogueTextObject.SetActive(true);
            if (textIndex == 0)
            {
                DialogueTextObject.GetComponent<TextMeshProUGUI>().text = TextList[0];
                textIndex++;
            }

            if (Input.GetKeyDown(KeyCode.Return))  // when interact key (enter) is pressed
            {
                if (textIndex < TextList.Count)  // if more text to go through
                {   
                    // change the text & increase index
                    DialogueTextObject.GetComponent<TextMeshProUGUI>().text = TextList[textIndex]; 
                    textIndex++;
                    // play text advance sound
                    AudioClip clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/TextUI/text-advance");
                    player.AudioSource.PlayOneShot(clip, 0.25f);
                }
                else if (textIndex >= TextList.Count && DialogueTextObject != null)  // if no more messages
                {
                    TextHasPlayed = true;
                    DialogueCanvas.SetActive(false);  // turn of canvas (might switch to indiv text on / off w/ one canvas that's always on)
                    textIndex = 0;  // reset to start for re-interactable text prompts
                    DialogueTextObject.GetComponent<TextMeshProUGUI>().text = TextList[0];  // same as above
                    if (FreezePlayer) { player.SetState(PlayerState.Idle); }  // unfreezes player if they were frozen
                    //play text close sound
                    AudioClip clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/TextUI/text-advance-close");
                    player.AudioSource.PlayOneShot(clip, 0.25f);
                }
            }
        }
    }

    private void UnlockDoor(string NextScene)
    {
        // Upon door interact, as long as it is unlocked and interactable, will freeze player (during fade out anim) and load next scene.

        if (NextScene != null && this.GetComponent<Door>().IsInteractable == true)
        {
            player.SetState(PlayerState.Frozen);
            LevelLoader.Instance.loadScene(NextScene);
        }

    }
    private void DisplayOutline()
    {
        // sets sprite of object to sprite with outline

        if (Outline != null && Default != null)
        {
            this.GetComponent<SpriteRenderer>().sprite = Outline;
        }
    }

    private void RemoveOutline()
    {
        // resets sprite of object to default sprite

        if (Default != null && Outline != null)
        {
            this.GetComponent<SpriteRenderer>().sprite = Default;
        }
    }

    private void RemoveSprite()
    {
        // removes sprites and turns off puzzle target script (used for drawer in kitchen, goes from closed to open).

        this.GetComponent<SpriteRenderer>().sprite = null;
        this.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void CheckPickupInteractions()
    {
        if (PlayerPrefs.GetInt("Crowbar") == 1 && this.name == "KitchenDoor (BOARDS)")  // when have crowbar and using on boarded door
        {
            HandleCrowbar();
        }

        if (PlayerPrefs.GetInt("Flashlight") == 1 && SceneManager.GetActiveScene().name == "Basement")  // When player grabs the flashlight
        {
            HandleFlashlight();
        }

        if (PlayerPrefs.GetInt("AtticKey") == 1)
        {
            HandleAtticKey();
        }

    }

    private void HandleCrowbar()
    {
        TextList = null;  // won't display text that was previously on the door
        if (PlayerPrefs.GetInt("escaped") == 0 && PressedInteract)
        {
            PromptCanvas.SetActive(true);
            PromptTextObject.GetComponent<TextMeshProUGUI>().text = "MASH efg TO PULL OFF BOARDS";
            logic.trapKills = false;
            player.SetState(PlayerState.Trapped);
        }
        else if (PlayerPrefs.GetInt("escaped") == 1)
        {
            SetObjectActive(ObjectsSpawned[0], true); // turn on other version of kitchen door
            SetObjectActive(this.gameObject, false);  // turns itself off
            PlayerPrefs.SetInt("DoorOff", 1);
        }
        else if (PlayerPrefs.GetInt("DoorOff") == 1)  // turns off boarded door and on non boarded door on load
        {
            SetObjectActive(ObjectsSpawned[0], true);
            SetObjectActive(this.gameObject, false);  // turns itself off
        }
    }

    private void HandleFlashlight()
    {
        PlayerPrefs.SetInt("DoorClosed", 1);  // door is now closed
        if (PlayerPrefs.GetInt("DoorClosed") == 1)
        {
            SpawnObjectCheck();  // spawns closed door
            if (this.gameObject.name == "Cellar Door (OPENED)")  // turns off open one, and will keep it off everytime you re-enter the scene
            {   
                //only deactivate the open door once the closing sound it done playing
                float len = player.AudioSource.clip.length;
                float passed = player.AudioSource.time;

                if (player.AudioSource.clip.name == "cellar-door-close-0") StartCoroutine(DelayedDeactivate(len - passed - 0.247f)); // float literal accounts for tail of audio file
            }                                                                                                                        // allowing the door closing audio and visual to sync
        }
    }

    private IEnumerator DelayedDeactivate (float duration){
        yield return new WaitForSecondsRealtime(duration);
        this.gameObject.SetActive(false);
    }

    private void HandleAtticKey()
    {
        if (this.gameObject.name == "Attic Door")
        {
            if (SceneManager.GetActiveScene().name == "Hallway Hub")
            {
                SpawnEntity = true;
                SpawnObjectCheck();
            }
            TextList = null;  // won't display text that was previously on the door
            if (PressedInteract)
            {
                this.GetComponent<Door>().IsInteractable = true;
                Unlock = true;
                UnlockDoor(NextScene);
            }
        }
    }

    private void SetObjectActive(GameObject obj, bool State)
    {
        // sets an object active or not depending on State

        obj.gameObject.SetActive(State);
    }

    private void SpawnObjectCheck()
    {
        // checks if enemy should be 'spawned' (turned on), then spawns them.

        if (SpawnObject == true && ObjectsSpawned != null)
        {
            foreach (GameObject obj in ObjectsSpawned)
            {
                SetObjectActive(obj.gameObject, true);
            }
        }
        else if (SpawnEntity == true && EntitySpawn != null)  // spawns any entity in the list
        {
            foreach (GameObject entity in EntitySpawn)
            {
                SetObjectActive(entity.gameObject, true);
            }
        }
    }
}