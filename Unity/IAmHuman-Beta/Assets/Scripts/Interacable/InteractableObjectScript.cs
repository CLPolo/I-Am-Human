using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class InteractableObjectScript : MonoBehaviour
{

    public LogicScript logic;
    public Player player;

    [Header("Sprites")]
    public Sprite Default = null;
    public Sprite Outline = null;

    [Header("Interaction")]
    public GameObject PromptTextObject = null;  // this is the screen that displays prompt to interact
    public GameObject PromptCanvas = null;  // NOTE: need to seperate text object and entire canvas as there are two text object on the canvas
    public String InteractMessage;
    public Boolean ShowPromptOnce;

    [Header("Info Display")]
    public GameObject DialogueTextObject = null;  // this is the screen that displays info about object post interaction
    public GameObject DialogueCanvas = null;  // NOTE: need to seperate text object and entire canvas as there are two text object on the canvas
    public List<String> TextList;

    [Header("Freeze Player")]
    public bool FreezePlayer = false;

    [Header("Play Sound")]
    public bool PlaySound = false;
    public List<AudioClip> Sounds;

    [Header("Unlock Door")]
    public bool Unlock;

    // Booleans
    private int textIndex = 0;
    private bool Inside = false;  // if player is inside hit box for object
    private bool HasInteracted = false;  // if player interacted with object
    private bool IsFlashback = false;  // if the post interaction display is a flashback sequence
    private bool CurrentlyPlaying = false;  // if flashback is currently playing through
    private bool NoisePlayed = false;
    private bool TextHasPlayed = false;  // makes it so that text doesn't play if press E again while still in collider
    private bool PressedInteract = false;


    // Below Should be removed and placed into monster script
    public bool monsterComing = false;  // This is for activating Monster.cs
    public AudioSource rawr = null;


    // Start is called before the first frame update
    void Start()
    {

        logic = LogicScript.Instance;
        player = Player.Instance;

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
        if (Inside && Input.GetKey(Controls.Interact)) // if player is inside the interactable object's box collider
        {
            PressedInteract = true;
            PromptCanvas.SetActive(false);  // turns off 'interact' prompt
            CheckAndDisplayInfo();  // checks if there's info to display, if so does that
            if (PlaySound && !NoisePlayed)
            {
                PlayNoise();
                NoisePlayed = true;
            }
            if (Unlock)
            {
                UnlockDoor();
            }
            RemoveOutline(); // removes outline when player has interacted before they exit collider again to remove confusion
        }
        if (PressedInteract) { CheckAndDisplayInfo(); }
    }

    // if they press InteractKey to interact & haven't already interacted with object and we only want the interact prompt to appear once

    private void OnTriggerEnter2D(Collider2D col)
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
    }

    private void PlayNoise()
    {
        // plays a random sound from the sound list each time

        AudioSource audio = GetComponent<AudioSource>();
        int randomNumber = UnityEngine.Random.Range(0, Sounds.Count + 1);
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
        if (DialogueCanvas != null && TextList.Count > 0 && !TextHasPlayed)
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

            if (Input.GetKeyDown(KeyCode.Return))  // when interact key (enter) is pressed
            {
                Debug.Log("Index: " + textIndex + "|| Count: " + TextList.Count);
                if (textIndex < TextList.Count)  // if more text to go through
                {
                    // change the text & increase index
                    DialogueTextObject.GetComponent<TextMeshProUGUI>().text = TextList[textIndex]; 
                    textIndex++;
                }
                else if (textIndex >= TextList.Count && DialogueTextObject != null)  // if no more messages
                {
                    TextHasPlayed = true;
                    DialogueCanvas.SetActive(false);  // turn of canvas (might switch to indiv text on / off w/ one canvas that's always on)
                    textIndex = 0;  // reset to start for re-interactable text prompts
                    DialogueTextObject.GetComponent<TextMeshProUGUI>().text = TextList[0];  // same as above
                    if (FreezePlayer) { player.SetState(PlayerState.Idle); }  // unfreezes player if they were frozen
                }
            }
        }
    }

    private void UnlockDoor()
    {
        if (this.GetComponent<Door>().IsInteractable == true)
        {
            LevelLoader.Instance.loadScene("End of Vertical Slice");
        }
        //THIS WILL BE CHANGED, AGAIN PANIC 
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
}