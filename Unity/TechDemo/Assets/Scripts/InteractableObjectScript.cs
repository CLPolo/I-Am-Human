using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEditor.U2D;

public class InteractableObjectScript : MonoBehaviour
{

    [Header("Interaction")]
    public GameObject InteractableScreen;  // this is the screen that displays prompt to interact
    public String InteractMessage;
    public KeyCode InteractKey;  // Keycode for the chosen interact key
    public Boolean ShowPromptOnce;

    [Header("Info Display")]
    public GameObject InfoScreen;  // this is the screen that displays info about object post interaction
    public List<String> InfoMessages;
    public int InfoTime = 3;  // time in seconds that info will be displayed for

    [Header("Flashback Sequences")]
    public List<Sprite> FlashbackImages;  // all images for the flashback sequence
    public GameObject FlashbackScreen;  // screen that displays the flashback images
    public int FlashbackTime = 4;  // time between images in sequence (in seconds)

    // Booleans
    private bool Inside = false;  // if player is inside hit box for object
    private bool HasInteracted = false;  // if player interacted with object
    private bool IsFlashback = false;  // if the post interaction display is a flashback sequence
    private bool CurrentlyPlaying = false;  // if flashback is currently playing through


    // Below Should be removed and placed into monster script
    public bool monsterComing = false;  // This is for activating Monster.cs
    //public AudioSource rawr;


    // Start is called before the first frame update
    void Start()
    {
        // turns off all screens
        InteractableScreen.SetActive(false);
        if (InfoScreen != null)
        {
            InfoScreen.SetActive(false);
        }
        if (FlashbackScreen != null)
        {
            FlashbackScreen.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Inside && Input.GetKey(InteractKey)) // if player is inside the interactable object's box collider
        {
            InteractableScreen.SetActive(false);  // turns off 'interact' prompt
            CheckAndDisplayInfo();  // checks if there's info to display, if so does that
            if (IsFlashback)  // If post interaction display is a flashback sequence
            {
                CheckAndDisplayFlashback();  // checks and displays flashback sequence
            }
        }
    }

    // if they press InteractKey to interact & haven't already interacted with object and we only want the interact prompt to appear once

    private void OnTriggerEnter2D(Collider2D col)
    {
        // if we want prompt to always show or the player has never interacted, then show prompt
        if (tag == "Flashback")  // NOTE: object that triggers flashback sequence shouldbe tagged as 'Flashback'
        {
            IsFlashback = true;
        }
        if (Inside = true && !Input.GetKeyDown(InteractKey))  // if player in collider and has NOT pressed interact key yet
        {
            DisplayInteractPrompt();  // shows the interact prompt
        }
        
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        // if player is not actively inside collider, turns off interact prompt
        Inside = false;
        Interactable();
    }

    private void DisplayInteractPrompt()
    {
        if (!CurrentlyPlaying && (!HasInteracted || !ShowPromptOnce))  // if they haven't already interacted and they aren't limited to interacting once only, and not currently playing
        {
            // displays interact text
            var interactText = InteractableScreen.GetComponentInChildren<Text>();
            interactText.text = InteractMessage;
            Interactable();  // turns on the interact screen
        }
    }

    private void CheckAndDisplayInfo()
    {
        // if there's info to display it will display it
        if (InfoScreen != null && InfoMessages.Count > 0)
        {
            StartCoroutine(DisplayInfo()); // post interaction function
        }
        HasInteracted = true;  // Show interact prompt only once
    }

    private void CheckAndDisplayFlashback()
    {
        // if there's a flashback to display it will display it
        if (FlashbackScreen != null && FlashbackImages.Count > 0 && !CurrentlyPlaying)  // won't allow you to play the flashback sequence if already playing
        {
            CurrentlyPlaying = true;
            StartCoroutine(DisplayFlashback()); // post interaction function
        }
        HasInteracted = true;  // Show interact prompt only once
    }

    private void Interactable()
    {
        // turns interact prompt on or off depending on whether player is inside
        InteractableScreen.SetActive(Inside);
    }

    private IEnumerator DisplayInfo()
    {
        // Set InfoScreen to active
        InfoScreen.SetActive(true);
        var infoText = InfoScreen.GetComponentInChildren<Text>();
        // iterate through info messages and show all
        foreach (var message in InfoMessages)
        {
            infoText.text = message;
            yield return new WaitForSeconds(InfoTime);
        }
        InfoScreen.SetActive(false);

        // Below Should be removed and placed somewhere in monster script
        //monsterComing = true;
        //rawr.Play();
    }

    private IEnumerator DisplayFlashback()
    {
        FlashbackScreen.SetActive(true);  // activates flashback screen 
        var ImageDisplay = FlashbackScreen.GetComponentInChildren<Image>();  // gets the empty image area where flashback stills will be placed
        foreach (var Still in FlashbackImages)  // iterates through all images and displays them in turn
        {
            ImageDisplay.sprite = Still;
            yield return new WaitForSeconds(FlashbackTime);
        }
        FlashbackScreen.SetActive(false);  // turns screen off when done
        CurrentlyPlaying = false;  // sequence is finished
    }
}