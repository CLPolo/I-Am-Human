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

    [Header("Outline Display")]
    public Sprite Outline;
    public Sprite Default;
    public Player player;

    public LogicScript logic;
    private int gracePeriod = 0; // temporary for box push/pull audio

    private List<GameObject> spritesToReset = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
        logic = LogicScript.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.Instance;
        }

        RaycastHit2D grabCheck = Physics2D.RaycastAll(grabDetect.position, Vector2.right * transform.localScale, rayDist).FirstOrDefault(x => x.collider.CompareTag("Box"));  // uses raycast starting at GrabDetect to get object info

        Debug.DrawRay(transform.position + new Vector3(0.2f, 0, 0), Vector3.right * rayDist, Color.green);

        if (grabCheck.collider != null && grabCheck.collider.CompareTag("Box"))  // if there is an object colliding AND that object is a box
        {
            GameObject box = grabCheck.collider.gameObject;
            Rigidbody2D rb = box.GetComponent<Rigidbody2D>();

            spritesToReset.Add(box);
            box.GetComponent<SpriteRenderer>().sprite = Outline;  // if box in range, shows outline
            if (Input.GetKey(Controls.Push))  // if player is pressing space (pushing)
            {
                player.SetState(PlayerState.Pushing);
                if (!PlayerPrefs.HasKey("boxlayer"))
                {
                    // ALL CODE IN HERE WILL ONLY RUN AT START OF PUSH/PULL INSTEAD OF EVERY FRAME
                    box.transform.position = new Vector3(boxHolder.position.x, box.transform.position.y, box.transform.position.z);  // moves object being pushed to boxHolder (by center)
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    string boxlayer = LayerMask.LayerToName(box.layer);
                    PlayerPrefs.SetString("boxlayer", boxlayer);
                    if (boxlayer == "Interactable")
                    {
                        // this can usually be walked through but for pushing & pulling,
                        // we want player to collide with it
                        box.layer = LayerMask.NameToLayer("PlayerExclusiveCollision");
                    }
                }
                rb.velocity = player.GetComponent<Rigidbody2D>().velocity;
                AudioSource aSource = box.GetComponent<AudioSource>();
                if (rb.velocity.magnitude > 0.5 )
                {
                    gracePeriod = 0;
                    if (!aSource.isPlaying)
                    {
                        box.GetComponent<AudioSource>().Play();
                    }
                } else if (aSource.isPlaying) {
                    // ok so this kept replaying and pausing almost every frame so i need a
                    // "grace period" of sorts to not immediately pause the first time velocity = 0
                    if (gracePeriod > 2)
                    {
                        box.GetComponent<AudioSource>().Pause();
                    } else {
                        gracePeriod++;
                    }
                }
            }
            // if push key is not being held down and the player was pushing last frame, now they are not pushing
            else if (player.GetState().IsOneOf(PlayerState.Pulling, PlayerState.Pushing))
            {
                player.SetState(PlayerState.Walking);
                box.transform.parent = null;  // removes boxHolder as parent
                box.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                box.layer = LayerMask.NameToLayer(PlayerPrefs.GetString("boxlayer"));
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                box.GetComponent<AudioSource>().Pause();
                PlayerPrefs.DeleteKey("boxlayer");
            }
        }
        else
        {
            foreach (GameObject obj in spritesToReset) {
                obj.GetComponent<SpriteRenderer>().sprite = Default;  // if box out of range, does not show outline
                obj.GetComponent<AudioSource>().Pause();
            }
            spritesToReset.Clear();
        }
    }
}
