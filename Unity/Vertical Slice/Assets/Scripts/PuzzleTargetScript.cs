using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Transactions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class PuzzleTargetScript : MonoBehaviour
{
    public GameObject AffectedObject = null;  // object to be affected by trigger actions (if desired)

    [Header("Target Affect Type")]
    // these allow us to define what the target will do
    public bool LayerSwitch;  // will switch layer of object
    public bool DeleteObject;  // will delete object
    public bool SpawnObject;  // will spawn object
    public bool TurnOffObject; // will set object to off instead of destroying it
    public bool NoiseTrigger;  // will trigger a noise
    public bool TextTrigger;  // will trigger text to display
    public bool HideTimed;  // will take a given action after player has been hidden for certain amount of time
    public bool FreezePlayer;  // freeze the player after activating object
    public bool UnlockDoor;  // unlocks a door when collide w/ box

    [Header("Affect Specifications")]
    public string LayerToSwitchTo = null;  // what layer the object will be switched to
    public AudioClip NoiseToBePlayed = null;  // what noise will be played
    public string TextToDisplay = null;  // what text string will be displayed
    public GameObject TextDisplay = null;  // canvas for text to be displayed on
    public bool TextDisplayOnce = false;
    public int DisplayTimeText = 0;  // time for text to be displayed for
    public int DisplayTimeObject = 0;  // time for object to be displayed for
    public int DelayTime = 0; // time to delay given action by
    public float HideTime = 0;  // time player must hide for before action activates
    public float FreezeTime = 0;  // time player is frozen for

    // bools
    private bool TextDisplaying = false;  // whether given text is currently being displayed
    private bool ObjectDisplaying = false;  // whether object has already been spawned 
    private bool TimerComplete = false;  // whether hide timer has completed or not
    private bool InteractionOver = false;
    private bool TextPlayed = false;
    private float TimerCount = 0;  // hide timer counter

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
        if (HideTimed)  // if hide timed, allows timer to function and handles any actions after
        {
            HideTimer();
            if (TimerComplete && !InteractionOver)
            {
                HandleHideTimed();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Box" ||  other.gameObject.tag == "Moveable")  // if box passes collides with target
        {
            HandleBox();
        }

        if (other.gameObject.tag == "Player")  // if player collides with target
        {
            HandlePlayer();
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
    }

    private void HandlePlayer()
    {
        // handles any actions post player interaction

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
        if (TextTrigger && !InteractionOver)  // if we want to spawn text
        {
            if (!TextDisplaying && (TextDisplayOnce ? (!TextPlayed) : true))
            {
                TextDisplaying = true;
                StartCoroutine(DisplayText());  // displays text
            }
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
            //Debug.Log(TimerCount);
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
        AffectedObject.GetComponent<Door>().isLocked = false;
    }

    private IEnumerator DisplayText()
    {
        // displays text for display time

        TextDisplay.GetComponent<TMPro.TextMeshProUGUI>().text = TextToDisplay;
        TextDisplay.SetActive(true);
        yield return new WaitForSeconds(DisplayTimeText);
        TextDisplay.SetActive(false);
        TextDisplaying = false;
        if (TextDisplayOnce)
        {
            TextPlayed = true;
        }
    }

    private IEnumerator ActivateObject()
    {
        // activates an object after DelatTime, then either deletes or turns it off based on specification

        yield return new WaitForSeconds(DelayTime);
        AffectedObject.SetActive(true);
        if (FreezePlayer)
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
        ObjectDisplaying = false;
    }

    private IEnumerator DestroyObject()
    {
        // destroys object after display time

        yield return new WaitForSeconds(DisplayTimeObject * Time.timeScale);
        Destroy(AffectedObject);
    }

    private IEnumerator DeactivateObject()
    {
        // deavtivates object after display time

        yield return new WaitForSeconds(DisplayTimeObject * Time.timeScale);
        AffectedObject.SetActive(false);
    }

    private IEnumerator Freeze()
    {
        // 'freezes' the player for freeze time seconds.

        Time.timeScale = .0000001f;
        yield return new WaitForSeconds(FreezeTime * Time.timeScale);
        Time.timeScale = 1.0f;

    }
}
