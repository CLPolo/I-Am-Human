using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Mathematics;
using System.Linq;

public class InteractableObjectScript : MonoBehaviour
{

    [Header("Interaction")]
    public GameObject InteractableScreen;  // this is the screen that displays prompt to interact
    public String InteractMessage;
    public Boolean ShowPromptOnce;

    [Header("Info Display")]
    public GameObject InfoScreen;  // this is the screen that displays info about object post interaction
    public List<String> InfoMessages;
    public int InfoTime = 3;  // time in seconds that info will be displayed for
    public int DelayBySeconds = 0;  // will delay text showing up (i.e. if want text post flashback)

    [Header("Flashback Sequences")]
    public List<Sprite> FlashbackImages;  // all images for the flashback sequence
    public GameObject FlashbackScreen;  // screen that displays the flashback images
    public int FlashbackTime = 4;  // time between images in sequence (in seconds)
    public LogicScript logic;

    [Header("Freeze Player")]
    public bool FreezePlayer = false;
    public float FreezeTime = 0;

    [Header("Play Sound")]
    public bool PlaySound = false;
    public List<AudioClip> Sounds;


    // Booleans
    private bool Inside = false;  // if player is inside hit box for object
    private bool HasInteracted = false;  // if player interacted with object
    private bool IsFlashback = false;  // if the post interaction display is a flashback sequence
    private bool CurrentlyPlaying = false;  // if flashback is currently playing through
    private bool NoisePlayed = false;
    private bool TextHasPlayed = false;  // makes it so that text doesn't play if press E again while still in collider


    // Below Should be removed and placed into monster script
    public bool monsterComing = false;  // This is for activating Monster.cs
    public AudioSource rawr = null;


    // Start is called before the first frame update
    void Start()
    {

        logic = LogicScript.Instance;

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
        if (Inside && Input.GetKey(Controls.Interact)) // if player is inside the interactable object's box collider
        {
            InteractableScreen.SetActive(false);  // turns off 'interact' prompt
            if (FreezePlayer && !CurrentlyPlaying)
            {
                StartCoroutine(Freeze());
            }
            CheckAndDisplayInfo();  // checks if there's info to display, if so does that
            if (IsFlashback)  // If post interaction display is a flashback sequence
            {
                CheckAndDisplayFlashback();  // checks and displays flashback sequence
            }
            if (PlaySound && !NoisePlayed)
            {
                PlayNoise();
                NoisePlayed = true;
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
        if (Inside = true && !Input.GetKeyDown(Controls.Interact))  // if player in collider and has NOT pressed interact key yet
        {
            DisplayInteractPrompt();  // shows the interact prompt
        }
        
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        // if player is not actively inside collider, turns off interact prompt
        Inside = false;
        Interactable();
        NoisePlayed = false;
        TextHasPlayed = false;
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
            if (InteractableScreen.GetComponentInChildren<Text>() != null)
            {
                var interactText = InteractableScreen.GetComponentInChildren<Text>();
                interactText.text = InteractMessage;
            }
            else if (InteractableScreen.GetComponentInChildren<TMPro.TextMeshProUGUI>() != null) // allows us to use text mesh pro aswell, which looks much nicer and scales better IMO
            {
                var interactText = InteractableScreen.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                interactText.text = InteractMessage;
            }
            Interactable();  // turns on the interact screen
        }
    }

    private void CheckAndDisplayInfo()
    {
        // if there's info to display it will display it
        if (InfoScreen != null && InfoMessages.Count > 0 && !TextHasPlayed)
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
        if (DelayBySeconds > 0)
        {
            yield return new WaitForSeconds(DelayBySeconds * Time.timeScale);
        }
        
        // Set InfoScreen to active
        InfoScreen.SetActive(true);

        // I know this is bulky, will reduce it later
        if (InfoScreen.GetComponentInChildren<Text>() != null)
        {
            var InfoText = InfoScreen.GetComponentInChildren<Text>();
            // iterate through info messages and show all
            foreach (var message in InfoMessages)
            {
                InfoText.text = message;
                yield return new WaitForSeconds(InfoTime * Time.timeScale);
            }
        }
        else if (InfoScreen.GetComponentInChildren<TMPro.TextMeshProUGUI>() != null) // allows us to use text mesh pro aswell, which looks much nicer and scales better IMO
        {
            var InfoText = InfoScreen.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            // iterate through info messages and show all
            foreach (var message in InfoMessages)
            {
                InfoText.text = message;
                yield return new WaitForSeconds(InfoTime * Time.timeScale);
            }
        }
        InfoScreen.SetActive(false);

        // Below Should be removed and placed somewhere in monster script
        monsterComing = true;
        if (rawr != null)
        {
            rawr.Play();
        }

        TextHasPlayed = true;

    }

    private IEnumerator DisplayFlashback()
    {
        FlashbackScreen.SetActive(true);  // activates flashback screen 
        var ImageDisplay = FlashbackScreen.GetComponentInChildren<Image>();  // gets the empty image area where flashback stills will be placed

        foreach (var Still in FlashbackImages)  // iterates through all images and displays them in turn
        {
                ImageDisplay.sprite = Still;
                yield return new WaitForSeconds(FlashbackTime *Time.timeScale);
        }

        FlashbackScreen.SetActive(false);  // turns screen off when done
        CurrentlyPlaying = false;  // sequence is finished
    }

    private IEnumerator Freeze()
    {
        // 'freezes' the player for freeze time seconds.

        Time.timeScale = .0000001f;
        yield return new WaitForSeconds(FreezeTime * Time.timeScale);
        Time.timeScale = 1.0f;

    }
}