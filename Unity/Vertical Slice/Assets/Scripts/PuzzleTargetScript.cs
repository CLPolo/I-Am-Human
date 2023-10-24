using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTargetScript : MonoBehaviour
{
    public GameObject AffectedObject = null;

    [Header("Target Affect Type")]
    // these allow us to define what the target will do
    public bool LayerSwitch;
    public bool DeleteObject;
    public bool SpawnMonster;
    public bool NoiseTrigger;
    public bool TextTrigger;

    [Header("Affect Specifications")]
    public string LayerToSwitchTo = null;
    public AudioClip NoiseToBePlayed = null;
    public string TextToDisplay = null;
    public GameObject TextDisplay = null;
    public int DisplayTime = 3;

    private bool CurrentlyRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Box" ||  other.gameObject.tag == "Moveable")  // if box passes collides with target
        {
            if (DeleteObject && AffectedObject != null)  // if object should be deleted
            {
                Destroy(AffectedObject);
            }
            if (LayerSwitch && AffectedObject != null && LayerToSwitchTo != null)  // if object layer should be changed
            {
                UpdateObjectLayer(LayerToSwitchTo);
            }
        }

        if (other.gameObject.tag == "Player")  // if player collides with target
        {
            if (SpawnMonster)
            {
                // spawn monster
            }
            if (NoiseTrigger)
            {
                PlayNoise(NoiseToBePlayed);
            }
            if (TextTrigger)
            {
                if (!CurrentlyRunning)
                {
                    CurrentlyRunning = true;
                    StartCoroutine(DisplayText());
                }
            }
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
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = Noise;
        audio.Play();
    }

    private IEnumerator DisplayText()
    {
        TextDisplay.GetComponent<TMPro.TextMeshProUGUI>().text = TextToDisplay;
        TextDisplay.SetActive(true);
        yield return new WaitForSeconds(DisplayTime);
        TextDisplay.SetActive(false);
        CurrentlyRunning = false;
    }

}
