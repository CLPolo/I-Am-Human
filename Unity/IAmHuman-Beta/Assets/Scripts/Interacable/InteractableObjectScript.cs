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
    public Sprite LeftOutline = null;
    public Sprite RightOutline = null;
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
    public int NextSceneIndex = -1;

    [Header("Spawn Object (or enemy) Post Interact")]
    public float spawnDelay = 0;
    public bool SpawnObject = false;
    public bool SpawnEntity = false;
    public List<GameObject> ObjectsSpawned = null;
    public List<GameObject> EntitySpawn = null;

    [Header("Remove Object (or enemy) Post Interact")]
    public bool DestroyAnObject = false;
    public List<GameObject> ObjectsToDestroy = new List<GameObject>();

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
                //NoisePlayed = true;
            }
            if (Unlock && !triggered) // triggered prevented a billion calls to level loader during the fade out, which prevented the positioning stuff from working properly
            {
                if (GetComponent<Door>().Blocked == false) triggered = true;  // prevent bug where when pressing E while door was blocked, triggered became true and you could never go thru the door
                UnlockDoor();
            }
            if (removeSprite)
            {
                RemoveSprite();
            }
            RemoveOutline(); // removes outline when player has interacted before they exit collider again to remove confusion
            PickupSet();
            if (SpawnObject || SpawnEntity) { SpawnObjectCheck(); } // spawns entity objects
            if (DestroyAnObject) { DestroyObjectCheck(); }
        }
        if (PressedInteract) { CheckAndDisplayInfo(); }  // displays info even if outside of collider, only needed if not frozen
        CheckPickupInteractions();
        CheckCorrectSprite();
    }

    // if they press InteractKey to interact & haven't already interacted with object and we only want the interact prompt to appear once

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag != "Sister") PlayerPrefs.SetString("CollisionTagInteractable", col.tag);  // Allows us to prevent the enemy (and sister) from triggering items (like doors & outlines)
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

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            Inside = true;  // player is inside obj's collider

            // this has a door copmponent
            if (TryGetComponent<Door>(out var door))
            {
                if (door.Blocked)
                {
                    RemoveOutline();
                } else {
                    DisplayOutline();
                }

                // if player in collider and has NOT pressed interact key yet
                if (Inside && !Input.GetKeyDown(Controls.Interact) && !PressedInteract)
                {
                    bool unlockAndIsInteractable = Unlock && door.IsInteractable;
                    bool notBlockedAndDontUnlock = !Unlock && !door.Blocked;
                    if (unlockAndIsInteractable || notBlockedAndDontUnlock)
                    {
                        DisplayInteractPrompt();
                    }
                }
            }
            // this does not have a door component
            else if (!Unlock)
            {
                DisplayOutline();
                DisplayInteractPrompt();
            }
            PickupCheck();
        }
    }

    private void PickupCheck()
    {
        // checks if object being interacted with is a pickup

        if (this.gameObject.tag.IsOneOf("Flashlight", "Crowbar", "AtticKey", "StudyKey", "LilyBear", "LilyShoe"))  // if one of our pickups, will give us usable string for tag.
        {
            PlayerPrefs.SetString("Pickup", this.tag);
            isPickup = true;
        }
        if (this.gameObject.tag == "Flashlight")
        {
            PickupSet();
            SpawnObjectCheck();
        }
    }

    private void PickupSet()
    {
        // sets appropriate player pref to reflect that pickup has been grabbed

        if (isPickup && PlayerPrefs.GetString("Pickup").IsOneOf("Flashlight", "Crowbar", "AtticKey", "StudyKey", "LilyBear", "LilyShoe")) // if one of our pickups, it will set playerprefs of that to 1 (true).
        {
            PlayerPrefs.SetInt(PlayerPrefs.GetString("Pickup"), 1);

            if (PlayerPrefs.GetString("Pickup").IsOneOf("Flashlight")) { this.gameObject.SetActive(false); }  // items w/ text (crowbar, keys, lily objs) handled seperately in their respective handle funcs

            AudioClip clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/item-pickup");

            if (Input.GetKeyDown(Controls.Interact)) player.AudioSource.PlayOneShot(clip, 0.5f);
            if (this.tag == "Flashlight")
            {
                clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/Door/cellar-door-close-0");
                player.AudioSource.clip = clip;
                if (!player.AudioSource.isPlaying) player.AudioSource.PlayOneShot(clip);
            }
        }

    }

    private void RemovePickedUpObjects()
    {
        // ensures they won't spawn on re-entry of a room if they were picked up

        if (this.gameObject.tag.IsOneOf("Flashlight", "Crowbar", "AtticKey", "StudyKey", "LilyBear", "LilyShoe"))
        {
            if (PlayerPrefs.GetInt(this.gameObject.tag) == 1)
            {
                this.gameObject.SetActive(false);
                SpawnObject = false;
            }
        }
    }

    private void PlayNoise()
    {   
        // plays a random sound from the sound list each time
        Sounds = Resources.LoadAll<AudioClip>("Sounds/SoundEffects/Entity/Interactable/Door/Locked/").ToList();  // this might make this function not work for any other noises we'd want
        AudioSource audio = GetComponent<AudioSource>();
        int randomNumber = UnityEngine.Random.Range(0, Sounds.Count);
        if (!audio.isPlaying) { audio.PlayOneShot(Sounds.ElementAt(randomNumber)); }
    }

    private void DisplayInteractPrompt()
    {
        if (!CurrentlyPlaying && (!HasInteracted || !ShowPromptOnce))  // if they haven't already interacted and they aren't limited to interacting once only, and not currently playing
        {
            // displays interact text
            if (PromptTextObject != null && PromptCanvas != null)
            {
                PromptCanvas.SetActive(true);
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
        if (PromptCanvas != null) PromptCanvas.SetActive(Inside);
    }

    private void DisplayText()
    {
        // click through version for text interaction. Will remove auto timed version from script & clean it up after
        // we've confirmed it's not being used and that click thru will be default. I also will rename the variables.
    
        if (TextList != null)  // if there is a list of text & door is not blocked
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
                    player.AudioSource.PlayOneShot(clip, 0.75f);
                }
                else if (textIndex >= TextList.Count && DialogueTextObject != null)  // if no more messages
                {
                    TextHasPlayed = ShowPromptOnce? true : false;
                    DialogueCanvas.SetActive(false);  // turn of canvas (might switch to indiv text on / off w/ one canvas that's always on)
                    textIndex = 0;  // reset to start for re-interactable text prompts
                    DialogueTextObject.GetComponent<TextMeshProUGUI>().text = TextList[0];  // same as above
                    if (FreezePlayer) { player.SetState(PlayerState.Idle); }  // unfreezes player if they were frozen
                    //play text close sound
                    AudioClip clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/TextUI/text-advance-close");
                    player.AudioSource.PlayOneShot(clip, 0.75f);
                    PressedInteract = false;
                    if (removeSprite)
                    {
                        this.GetComponent<BoxCollider2D>().enabled = false;
                        this.GetComponent<SpriteRenderer>().sprite = null;
                    }
                }
            }
        }
    }

    private void UnlockDoor()
    {
        // Upon door interact, as long as it is unlocked and interactable, will freeze player (during fade out anim) and load next scene.

        if (this.GetComponent<Door>().IsInteractable == true && !GetComponent<Door>().Blocked)
        {
            if (!this.gameObject.name.IsOneOf("Basement Stairs", "Attic Stairs") && !player.AudioSource.isPlaying)
            {
                player.AudioSource.PlayOneShot((AudioClip)Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/Door/cabin-door-open-0"), 0.25f);  // plays door open sound when opening
            }
            if (NextSceneIndex >= 0) {
                player.SetState(PlayerState.Frozen);
                LevelLoader.Instance.loadScene(NextSceneIndex);
            } else if (!NextScene.IsNullOrEmpty()) {
                player.SetState(PlayerState.Frozen);
                LevelLoader.Instance.loadScene(NextScene);
            }
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
        if (TextList == null) { this.GetComponent<BoxCollider2D>().enabled = false; }  // if text after interact, will wait to turn off collider once text done
    }

    private void CheckPickupInteractions()
    {
        if (PlayerPrefs.GetInt("Crowbar") == 1)  // when have crowbar and using on boarded door
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

        if (PlayerPrefs.GetInt("StudyKey") == 1)
        {
            HandleStudyKey();
        }

        if (SceneManager.GetActiveScene().name == "Forest Chase" && this.name == "HideBush" && PlayerPrefs.GetInt("MonsterEmerges") == 2)  // temp implementation of run text (in chase scene it's in spawn object for hide bush)
        {
            SpawnObject = true;
            SpawnObjectCheck();
        }

    }

    private void HandleStudyKey()
    {
        if (this.name == "Study Door")
        {
            TextList = null;
            if (PressedInteract)
            {
                SetObjectActive(this.gameObject, false);  // turns itself off
                PlayerPrefs.SetInt("StudyDoorOpened", 1);
            }
            else if (PlayerPrefs.GetInt("StudyDoorOpened") == 1)  // keep perma opened after opening it once
            {
                SetObjectActive(this.gameObject, false);
            }
        }
        else if (this.name == "StudyKey" && PlayerPrefs.GetInt("StudyKey") == 1)
        {
            this.GetComponent<SpriteRenderer>().sprite = null;
            if (TextHasPlayed)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
    private void HandleCrowbar()
    {
        if (this.name == "KitchenDoor (BOARDS)")
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
        else if (this.name == "Crowbar" && PlayerPrefs.GetInt("Crowbar") == 1)
        {
            GetComponent<SpriteRenderer>().sprite = null;
            if (TextHasPlayed) 
            { 
                this.gameObject.SetActive(false); //turns collider off (must remain on for text to be able to be skipped thru
                SpawnEntity = true; // allows entity (crawler) to spawn
                SpawnObjectCheck(); // Spawns crawler
            }
        }
    }

    private void HandleFlashlight()
    {
        PlayerPrefs.SetInt("DoorClosed", 1);  // door is now closed
        if (PlayerPrefs.GetInt("DoorClosed") == 1)
        {
            if (this.gameObject.name == "Cellar Door (OPENED)" && this.gameObject.activeSelf)  // turns off open one, and will keep it off everytime you re-enter the scene
            {
                //only deactivate the open door once the closing sound it done playing
                float len = player.AudioSource.clip?.length ?? 0;
                float passed = player.AudioSource.time;

                if (player.AudioSource.clip.name == "cellar-door-close-0") StartCoroutine(DelayedActivate(len - passed - 0.247f)); // float literal accounts for tail of audio file
            }                                                                                                                        // allowing the door closing audio and visual to sync
            if (this.gameObject.name == "Basement Stairs" && !Unlock)
            {
                Unlock = true;
                TextList = null;  // won't display text that was previously on the door
            }
        }
    }

    private IEnumerator DelayedActivate (float duration) {
        player.SetState(PlayerState.Frozen);  // player will unfreeze from puzzle target text
        yield return new WaitForSecondsRealtime(duration);
        SetObjectActive(EntitySpawn[0], true); // add puzzle target for lily text that appears on player
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
                UnlockDoor();
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

    private void DestroyObjectCheck()
    {
        if (DestroyAnObject == true && ObjectsToDestroy.Count > 0)
        {
            foreach (GameObject obj in ObjectsToDestroy)
            {
                SetObjectActive(obj, false);
            }
        }
    }

    public void CheckCorrectSprite()  // fixes a bug with raycast & pushlogic and removing sprites where if there was a side highlight and you moved into another raycasted obj it wouldn't remove it. This was the only way it could work that I could find.
    {
        if (this.name != player.GetComponent<PushLogicScript>().NameOfObjInCast() && !Inside && (this.tag == "Box" || this.tag == "Hideable"))
        {
            if (GetComponent<SpriteRenderer>().sprite != Default)
            {
                GetComponent<SpriteRenderer>().sprite = Default;
            }
        }
    }

}