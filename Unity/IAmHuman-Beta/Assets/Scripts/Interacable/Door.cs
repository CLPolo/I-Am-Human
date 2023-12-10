using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Boolean isLocked = true;
    public bool IsInteractable = false;
    public bool Blocked = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Hideable" || col.tag == "Box")  // basically, if there's a box in the way, you can't go through the door (specific example: bookshelf in hallway, could also apply to forest intro cellar box)
        {
            IsInteractable = false;
            Blocked = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Hideable" || col.tag == "Box")  // once the box isnt in front of the door, can be opened
        {
            IsInteractable = true;
            Blocked = false;
        }
    }

}
