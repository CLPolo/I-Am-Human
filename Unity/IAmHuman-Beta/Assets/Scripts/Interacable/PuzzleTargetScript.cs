﻿using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Transactions;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using UnityEditorInternal;

public class PuzzleTargetScript : MonoBehaviour
{
    public GameObject AffectedObject = null;  // object to be affected by trigger actions (if desired)
    public GameObject ActivateTriggerObject = null;  // trigger to be deleted / deactivated if player performs this action
    public GameObject DeactivateTriggerObject = null;
    public GameObject DoorToBeOpened = null;

    [Header("Target Affect Type")]
    // these allow us to define what the target will do
    public bool LayerSwitch;                    // will switch layer of object
    public bool SpawnObject;                    // will spawn object
    public bool TurnOffObject;                  // will set object to off instead of destroying it
    public bool NoiseTrigger;                   // will trigger a noise
    public bool TextTrigger;                    // will trigger text to display
    public bool HideTimed;                      // will take a given action after player has been hidden for certain amount of time
    public bool FreezePlayerObject;             // freeze the player after activating object
    public bool FreezePlayerText;               // freeze player after activating text
    public bool FreezePlayerOnly;               // freezes player once when collide, doesn't trigger anything else tho (does it only once)
    public bool UnlockDoor;                     // unlocks a door when collide w/ box
    public bool NoisePlayOnSpawn;               // again messy i know, just panic
    public bool SpawnDoor;
    public bool TrappedText;
    public bool PushPullText;

    [Header("Affect Specifications")]
    public string LayerToSwitchTo = null;       // what layer the object will be switched to
    public AudioClip NoiseToBePlayed = null;    // what noise will be played
    public List<String> TextToDisplay = null;   // what text string will be displayed
    public GameObject TextObject = null;        // canvas for text to be displayed on
    public GameObject TextCanvas = null;
    public bool TextDisplayOnce = false;        // if text should only be displayed once (only one pass through)
    public int DisplayTimeObject = 0;           // time for object to be displayed for
    public int DelayTime = 0;                   // time to delay given action by
    public float HideTime = 0;                  // time player must hide for before action activates
    public float FreezeTime = 0;                // time player is frozen for

    // bools
    private bool ObjectDisplaying = false;      // whether object has already been spawned 
    private bool TimerComplete = false;         // whether hide timer has completed or not
    private bool InteractionOver = false;
    private bool NoiseTriggered = false;        // whether the noise has already been triggered
    private bool TextPlayed = false;
    private float TimerCount = 0;               // hide timer counter
    private bool FrozenOnce = false;
    private int textIndex = 0;
    private bool PlayerTriggered = false;
    private bool BoxTriggered = false;

    // misc
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.Instance;
        }
 
        if (HideTimed)  // if hide timed, allows timer to function and handles any actions after
        {
            HideTimer();
            if (TimerComplete && !InteractionOver)
            {
                HandleHideTimed();
            }
        }

        if (PlayerTriggered)
        {
            HandlePlayer();
        }
        if (BoxTriggered)
        {
            HandleBox();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Box"))  // if box passes collides with target
        {
            HandleBox();  // has to happen here ?? in update, box triggers in forest don't work properly.
            BoxTriggered = true;
        }
        if (other.gameObject.CompareTag("Player"))  // if player collides with target
        {
            PlayerTriggered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerTriggered = false;
        BoxTriggered = false;
        if (!TextDisplayOnce)  // if want to retrigger text everytime player walks into collider, resets textplayed after leave
        {
            TextPlayed = false;
        }
    }

    private void HandleBox()
    {
        // handles any actions post box interaction

        if (LayerSwitch && AffectedObject != null && LayerToSwitchTo != null)  // if object layer should be changed
        {
            UpdateObjectLayer(LayerToSwitchTo);
        }
        if (UnlockDoor)
        {
            Unlock();
        }
        if (SpawnDoor)
        {
            DoorToBeOpened.SetActive(true);
        }
    }

    private void HandlePlayer()
    {
        // handles any actions post player interaction
        if (FreezePlayerOnly && !FrozenOnce)
        {
            StartCoroutine(Freeze());
            FrozenOnce = true;
        }
        if (SpawnObject && AffectedObject != null && this.enabled == true)  // if we want to spawn / activate an object
        {
            if (!ObjectDisplaying && !HideTimed)  // if not already there and not a hidetimed event
            {
                StartCoroutine(ActivateObject());  // activates object
                ObjectDisplaying = true;
            }
        }
        if (NoiseTrigger && !NoiseTriggered)  // if we want to play a noise
        {
            PlayNoise(NoiseToBePlayed);
        }
        if (TextTrigger && !TextPlayed) // if we want to spawn text
        {
            DisplayText();
        }
        if (TrappedText)  // if player is trapped and want to display trapped text
        {
            DisplayTrappedText();
        }
        if (PushPullText)  // if want to display text for push pull
        {
            DisplayPushText();
        }
    }

    private void HandleHideTimed()
    {
        // handles actions post hide time and resets timer completion (if want to repeat action)

        if (SpawnObject)  // if want to spawn object
        {
            HideTimedActivation();  // hide timed activation
            TimerComplete = false;  // resets timer if want to re-use
            InteractionOver = true; // interaction is now 'over'
        }
    }
    private void HideTimer()
    {
        // Runs the hide timer which counts how many seconds a player has been hiding for (non-consecutively)

        if (player.GetState() == PlayerState.Hiding && TimerCount <= HideTime)  // if the player is hiding, and the timer still hasn't hit hidetime
        {
            TimerCount += 1 * Time.deltaTime;
        }
        else if (TimerCount >= HideTime)
        {
            TimerComplete = true;
            TimerCount = 0;
        }
    }

    private void UpdateObjectLayer(string Layer)
    {
        // changes an objects layer to the one passed through by the string Layer.
        // For example, used to change box's layer to Interactable, allowing the player to NOT be blocked by the box.
        // Allows to switch from box preventing player passing by / through to allowing player to pass through.
        if (PlayerPrefs.HasKey("boxlayer"))
        {
            PlayerPrefs.SetString("boxlayer", Layer);
        } else {
            AffectedObject.layer = LayerMask.NameToLayer(Layer);
        }
    }

    private void PlayNoise(AudioClip Noise)
    {
        // plays noise once on trigger, then won't play again if re-pass over trigger

        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(Noise);
        NoiseTriggered = true;
    }
    private void HideTimedActivation()
    {
        // Activates an object post hide time
        // NOT SURE IF I NEED THIS STILL

        if (!ObjectDisplaying)
        {
            StartCoroutine(ActivateObject());
            ObjectDisplaying = true;
        }
    }

    private void Unlock()
    {
        // unlocks a door that was previously locked (used for cellar), makes it so that next time they press interact button when prompted, switch scene.
        // NOTE: Affected object must have door script.

        AffectedObject.GetComponent<Door>().IsInteractable = true;
    }

    private void DisplayText()
    {
        // Displays Text Post Interaction, and freezes the player during that text if desired (SHOULD BE STANDARD ?).
        // Text is click through using enter (currently set up for dialogue, can make version for prompt).

        if (FreezePlayerText)
        {
            player.SetState(PlayerState.Frozen);
        }

        TextCanvas.SetActive(true);
        TextObject.SetActive(true);

        if (textIndex == 0)  // sets first message up for when dialogue pops up initially
        {
            TextObject.GetComponent<TextMeshProUGUI>().text = TextToDisplay[textIndex];
            textIndex++;
        }

        if (Input.GetKeyDown(KeyCode.Return))  // then, if more messages, when interact key is pressed
        {
            if (textIndex < TextToDisplay.Count)  // if more text to go through
            {
                // change the text & increase index
                TextObject.GetComponent<TextMeshProUGUI>().text = TextToDisplay[textIndex];
                textIndex++;

                // play text advance sound
                AudioClip clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/TextUI/text-advance");
                player.AudioSource.PlayOneShot(clip, 0.25f);
            }
            else if (textIndex >= TextToDisplay.Count && TextObject != null)  // if no more messages
            {
                if (FreezePlayerText) { player.SetState(PlayerState.Idle); }
                TextPlayed = true;
                TextCanvas.SetActive(false);  // turn of canvas (might switch to indiv text on / off w/ one canvas that's always on)
                AudioClip clip = Resources.Load<AudioClip>("Sounds/SoundEffects/Entity/Interactable/TextUI/text-advance-close");
                player.AudioSource.PlayOneShot(clip, 0.25f);
            }
        }
    }

    private IEnumerator ActivateObject()
    {
        // Activates an object (AffectedObject) after a Delay (DelayTime), then also sets off any of a series of post activation actions.
        // These include: Noise Playing before &/or after, deactivation after X seconds, freezing the player for X seconds,
        // Deactivating &/or Activating a seperate puzzle target / trigger.

        yield return new WaitForSeconds(DelayTime * Time.timeScale);
        AffectedObject.SetActive(true);
        
        if (NoisePlayOnSpawn)
        {
            PlayNoise(NoiseToBePlayed);
        }
        if (FreezePlayerObject)
        {
            StartCoroutine(Freeze());
        }
        if (TurnOffObject)
        {
            StartCoroutine(DeactivateObject());
        }
        if (DeactivateTriggerObject != null)
        {
            ActivateTarget(DeactivateTriggerObject, false);
        }
        if (ActivateTriggerObject != null)
        {
            ActivateTarget(ActivateTriggerObject, true);
        }
        
        ObjectDisplaying = false;
    }

    private void ActivateTarget(GameObject trigger, bool val)
    {
        // activates (or deactivates) a given puzzle target (trigger) based on given value (val) [for example, used for the 'too far hide' trigger]

        trigger.SetActive(val);
    }

    private IEnumerator DeactivateObject()
    {
        // Deactivates object after display time

        yield return new WaitForSeconds(DisplayTimeObject);  // waits X time / object displays for X time
        AffectedObject.SetActive(false);  // turns it off
        if (NoisePlayOnSpawn)  // need to rework, this is used for specific instance of deer head and bush rustle.
        {
            PlayNoise(NoiseToBePlayed);
        }
    }
    private IEnumerator Freeze()
    {
        // 'freezes' the player for freeze time seconds.
        // NOTE: Useful only when trying to freeze the player NOT during dialogue text

        player.SetState(PlayerState.Frozen);
        yield return new WaitForSeconds(FreezeTime);
        player.SetState(PlayerState.Idle);

    }

    public void DisplayTrappedText()
    {
        // displays text for WHILE player is trapped, will reactivate whenever trap is reactivated
        // NOTE: collider / target for this must rest above the trap and be a bit wider than the trap collider


        if (player.GetState() == PlayerState.Trapped)  // once player is trapped
        {
            TextCanvas.SetActive(true);  // turn on appropriate canvases
            TextObject.SetActive(true);

            TextObject.GetComponent<TextMeshProUGUI>().text = TextToDisplay[0];  // set prompt text (first string in list)
        }
        else  // not trapped anymore
        {
            TextObject.SetActive(false);  // turn off text display
        }
    }
    public void DisplayPushText()
    {
        // displays text for X amount of seconds AFTER player has pushed or pulled an object
        // NOTE: Collider must accomodate area player could move in, player has to push/pull while inside

        if (!(player.GetState() == PlayerState.Pushing || player.GetState() == PlayerState.Pulling) && !InteractionOver)  // if not pushing || pulling && hasn't happened already (no reactivation)
        {
            TextCanvas.SetActive(true);  // turn on appropriate canvases
            TextObject.SetActive(true);

            TextObject.GetComponent<TextMeshProUGUI>().text = TextToDisplay[0];  // set prompt text (first string in list)
        }
        else  // once pushed or pull
        {
            InteractionOver = true;  // interaction has happened (no reactivation)
            StartCoroutine(RemoveTextAfterWait(FreezeTime));  // Will turn text off after FreezeTime Seconds
        }
    }

    private IEnumerator RemoveTextAfterWait(float Seconds)
    {
        // will turn text off after X amount of seconds (currently useful for push/pull text)

        yield return new WaitForSeconds(Seconds);
        if (PlayerTriggered)  // prevents collision with other prompt after from turning that prompt off, and instead would just update the text
        {
            TextObject.SetActive(false);
        }
    }

}