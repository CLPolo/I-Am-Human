using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class InteractableObjectScript : MonoBehaviour
{

    public GameObject InteractableScreen;  // this is the screen that displays prompt to interact
    public GameObject InfoScreen;  // this is the screen that displays info about object post interaction
    public List<String> InfoMessages;
    public String InteractMessage;
    public int InfoTime = 3;  // time in seconds that info will be displayed for
    public KeyCode InteractKey;  // Keycode for the chosen interact key
    public Boolean ShowPromptOnce;

    private bool Inside = false;  // if player is inside hit box for object
    private bool HasInteracted = false;  // if player interacted with object

    // Start is called before the first frame update
    void Start()
    {
        InteractableScreen.SetActive(false);
        InfoScreen?.SetActive(false);
        Debug.Log(InteractMessage);
    }

    // Update is called once per frame
    void Update()
    {
        if (Inside) // if player is inside the interactable object's box collider
        {
            // if they press InteractKey to interact & haven't already interacted with object
            // and we only want the interact prompt to appear once
            if (Input.GetKeyDown(InteractKey))
            {
                InteractableScreen.SetActive(false);  // turns off 'interact' prompt
                Debug.Log(InfoScreen != null && InfoMessages.Count > 0);
                if (InfoScreen != null && InfoMessages.Count > 0)
                {
                    StartCoroutine(InfoInteraction()); // post interaction function
                }
                HasInteracted = true;  // Show interact prompt only once
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // if we want prompt to always show or the player has never interacted, then show prompt
        Inside = true;
        if (!HasInteracted || !ShowPromptOnce)
        {
            var interactText = InteractableScreen.GetComponentInChildren<Text>();
            interactText.text = InteractMessage;
            Debug.Log(interactText);
            Interactable();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        // if player is not actively inside collider, turns off interact prompt
        Inside = false;
        Interactable();
    }

    private void Interactable()
    {
        // turns interact prompt on or off depending on state provided
        // bool state - state to determine if interact prompt is on or off
        InteractableScreen.SetActive(Inside);
    }

    private IEnumerator InfoInteraction()
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
    }
}