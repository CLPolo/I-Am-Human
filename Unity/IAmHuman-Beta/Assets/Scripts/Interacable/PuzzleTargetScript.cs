using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Transactions;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class PuzzleTargetScript : MonoBehaviour
{
    public GameObject AffectedObject = null;  // object to be affected by trigger actions (if desired)
    public GameObject ActivateTriggerObject = null;  // trigger to be deleted / deactivated if player performs this action
    public GameObject DeactivateTriggerObject = null;
    public GameObject DoorToBeOpened = null;

    [Header("Target Affect Type")]
    // these allow us to define what the target will do
    public bool LayerSwitch;                    // will switch layer of object
    public bool DeleteObject;                   // will delete object
    public bool SpawnObject;                    // will spawn object
    public bool TurnOffObject;                  // will set object to off instead of destroying it
    public bool NoiseTrigger;                   // will trigger a noise
    public bool TextTrigger;                    // will trigger text to display
    public bool HideTimed;                      // will take a given action after player has been hidden for certain amount of time
    public bool FreezePlayerObject;             // freeze the player after activating object
    public bool FreezePlayerText;               // freeze player after activating text
    public bool FreezePlayerOnly;               // freezes player once when collide, doesn't trigger anything else tho (does it only once)
    public bool UnlockDoor;                     // unlocks a door when collide w/ box
    public bool FreezeContinuously;             // for impassable target where it still triggers text (messy i know, i'll refactor after)
    public bool NoisePlayOnSpawn;               // again messy i know, just panic
    public bool SpawnDoor;

    [Header("Affect Specifications")]
    public string LayerToSwitchTo = null;       // what layer the object will be switched to
    public AudioClip NoiseToBePlayed = null;    // what noise will be played
    public List<String> TextToDisplay = null;          // what text string will be displayed
    public GameObject TextObject = null;       // canvas for text to be displayed on
    public GameObject TextCanvas = null;
    public bool TextDisplayOnce = false;
    public int DisplayTimeText = 0;             // time for text to be displayed for
    public int DisplayTimeObject = 0;           // time for object to be displayed for
    public int DelayTime = 0;                   // time to delay given action by
    public float HideTime = 0;                  // time player must hide for before action activates
    public float FreezeTime = 0;                // time player is frozen for

    // bools
    private bool TextDisplaying = false;        // whether given text is currently being displayed
    private bool ObjectDisplaying = false;      // whether object has already been spawned 
    private bool TimerComplete = false;         // whether hide timer has completed or not
    private bool InteractionOver = false;
    private bool NoiseTriggered = false;        // whether the noise has already been triggered
    private bool TextPlayed = false;
    private float TimerCount = 0;               // hide timer counter
    private bool FrozenOnce = false;
    private int textIndex = 0;
    private bool PlayerTriggered = false;

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
        if (FreezeContinuously)
        {
            if (this.isActiveAndEnabled == false)
            {
                ActivateTarget(DeactivateTriggerObject, false);
            }
        }
        if (PlayerTriggered && TextTrigger && (TextDisplayOnce ? (!TextPlayed) : true))
        {
            DisplayText();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Box")  // if box passes collides with target
        {
            HandleBox();
        }
        if (other.gameObject.tag == "Player")  // if player collides with target
        {
            HandlePlayer();
            PlayerTriggered = true;
        }
    }

    private void HandleBox()
    {
        // handles any actions post box interaction

        if (DeleteObject && AffectedObject != null)  // if object should be deleted
        {
            Destroy(AffectedObject);
        }
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
            StartCoroutine(FreezeONLY());
            FrozenOnce = true;
        }
        if (SpawnObject && AffectedObject != null)  // if we want to spawn / activate an object
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

        AffectedObject.layer = LayerMask.NameToLayer(Layer);
        PlayerPrefs.SetString("boxlayer", Layer);
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
        // activates an object post hide time

        if (!ObjectDisplaying)
        {
            StartCoroutine(ActivateObject());
            ObjectDisplaying = true;
        }
    }

    private void Unlock()
    {
        AffectedObject.GetComponent<Door>().IsInteractable = true;
    }

    private void DisplayText()
    {
        // click through version for text interaction. Will remove auto timed version from script & clean it up after
        // we've confirmed it's not being used and that click thru will be default. I also will rename the variables.

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
            }
            else if (textIndex >= TextToDisplay.Count && TextObject != null)  // if no more messages
            {
                if (FreezePlayerText) { player.SetState(PlayerState.Idle); }
                TextPlayed = true;
                TextCanvas.SetActive(false);  // turn of canvas (might switch to indiv text on / off w/ one canvas that's always on)
            }
        }
    }

    private IEnumerator ActivateObject()
    {
        // activates an object after DelatTime, then either deletes or turns it off based on specification
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
        if (DeleteObject)
        {
            StartCoroutine(DestroyObject());
        }
        else if (TurnOffObject)
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
        trigger.SetActive(val);
    }

    private IEnumerator DestroyObject()
    {
        // destroys object after display time

        yield return new WaitForSeconds(DisplayTimeObject);
        Destroy(AffectedObject);
    }

    private IEnumerator DeactivateObject()
    {
        // deavtivates object after display time

        yield return new WaitForSeconds(DisplayTimeObject);
        AffectedObject.SetActive(false);
        if (NoisePlayOnSpawn)
        {
            PlayNoise(NoiseToBePlayed);
        }
    }

    private IEnumerator FreezeONLY()
    {
        // 'freezes' the player for freeze time seconds.
        Time.timeScale = 0.000001f;
        yield return new WaitForSeconds(FreezeTime * Time.timeScale);
        Time.timeScale = 1f;

    }
    private IEnumerator Freeze()
    {
        // 'freezes' the player for freeze time seconds.
        player.SetState(PlayerState.Frozen);
        yield return new WaitForSeconds(FreezeTime);
        player.SetState(PlayerState.Idle);

    }
}
