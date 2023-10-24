using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PushLogicScript : MonoBehaviour
{
    // This script should be placed on the player, not the box

    public Transform grabDetect;
    public Transform boxHolder;
    public float rayDist;  // distance of raycast (distance between player and obejct where push action is possible)
    private Vector3 forward;
    [Header("Outline Display")]
    public Sprite Outline;
    public Sprite Default;

    private List<GameObject> spritesToReset = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D grabCheck = Physics2D.Raycast(grabDetect.position, Vector2.right * transform.localScale, rayDist);  // uses raycast starting at GrabDetect to get object info

        if (grabCheck.collider != null && grabCheck.collider.tag == "Box")  // if there is an object colliding AND that object is a box
        {
            GameObject box = grabCheck.collider.gameObject;
            spritesToReset.Add(box);
            box.GetComponent<SpriteRenderer>().sprite = Outline;  // if box in range, shows outline
            if (Input.GetKey(KeyCode.LeftShift))  // if player is pressing space (pushing)
            {
                GetComponent<Player>().state = PlayerState.Pushing;
                box.transform.parent = boxHolder;  // sets parent of box / object being pushed to boxHolder
                box.transform.position = boxHolder.position;  // moves object being pushed to boxHolder (by center)
                box.GetComponent<Rigidbody2D>().isKinematic = true;  // allows it to staticly move with the player based on that boxHolder position
            }
            else
            {
                GetComponent<Player>().state = PlayerState.Walking;
                box.transform.parent = null;  // removes boxHolder as parent
                box.GetComponent<Rigidbody2D>().isKinematic = false;  // sets it back to being immovable
            }
        }
        else
        {
            foreach (GameObject obj in spritesToReset) {
                obj.GetComponent<SpriteRenderer>().sprite = Default;  // if box out of range, does not show outline
            }
            spritesToReset.Clear();
        }
    }
}
