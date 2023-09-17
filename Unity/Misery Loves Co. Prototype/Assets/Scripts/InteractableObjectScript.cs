using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InteractableObjectScript : MonoBehaviour
{

    public GameObject InteractableScreen;  // this is the screen that displays prompt to interact
    public GameObject InfoScreen;  // this is the screen that displays info about object post interaction
    public Text FirstText;  // First bit of info text to be displayed 
    public Text SecondText;  // Second bit of info text to be displayed
    public int InfoTime = 3;  // time in seconds that info will be displayed for
    public KeyCode InteractKey;  // Keycode for the chosen interact key

    private bool Inside = false;  // if player is inside hit box for object
    private bool Not_Interacted = true;  // if player has not yet interacted with object
    private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        coroutine = wait(InfoTime);  // sets coroutine to wait function
        SecondText.enabled = false;  // disables second text initially
    }

    // Update is called once per frame
    void Update()
    {
        if (Inside) // if player is inside the interactable object's box collider
        {  
            if (Input.GetKeyDown(InteractKey) && Not_Interacted)  // if they press InteractKey to interact & haven't already interacted with object
            {
                InteractableScreen.SetActive(false);  // turns off 'interact' prompt
                Interaction(); // post interaction function
                Not_Interacted = false;  // Allows player to only interact once
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // if player is actively inside collider & hasn't already interacted w/ object, turns on interact prompt
        Inside = true;
        if (Not_Interacted)
        {
            Interactable(Inside);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        // if player is not actively inside collider, turns off interact prompt
        Inside = false;
        Interactable(Inside);
    }

    private void Interactable(bool state)
    {
        // turns interact prompt on or off depending on state provided
        // bool state - state to determine if interact prompt is on or off
        InteractableScreen.SetActive(state);
    }

    private void Interaction()
    {
        // Shows object info and runs wait coroutine
        InfoScreen.SetActive(true);
        StartCoroutine(wait(InfoTime));  // will wait InfoTime seconds then remove info text

    }

    IEnumerator wait(int time)
    {
        // Shows each set of text for InfoTime seconds before turning it off
        // int InfoTime - length of time in seconds that info will be displayed
        yield return new WaitForSeconds(time);
        FirstText.enabled = false;
        SecondText.enabled = true;
        yield return new WaitForSeconds(time);
        InfoScreen.SetActive(false);
    }
}