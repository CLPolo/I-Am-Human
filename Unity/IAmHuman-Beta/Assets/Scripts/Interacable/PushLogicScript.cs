using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PushLogicScript : MonoBehaviour
{
    // This script should be placed on the player, not the box

    public Transform grabDetect;
    public Transform boxHolder;
    public float rayDist;  // distance of raycast (distance between player and obejct where push action is possible)

    [Header("Push Text Prompt")]
    public GameObject TextCanvas = null;
    public GameObject TextObject = null;
    public string Text = null;

    [Header("Outline Display")]
    public Sprite WholeOutline;
    public Sprite LeftOutline;
    public Sprite RightOutline;
    public Sprite Default;
    public Player player;

    public LogicScript logic;
    private int gracePeriod = 0; // temporary for box push/pull audio

    private List<GameObject> spritesToReset = new List<GameObject>();
    private bool InteractionOver = false;
    private bool TextRemoved = false;  // prevents updates from constantly turning off prompt
    private float HoldFor = 1f;

    private GameObject currentBox = null;
    private GameObject prevBox = null;
    private Sprite prevSpriteDef = null;
    private int playerSortOrder = 1;

    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
        logic = LogicScript.Instance;
        playerSortOrder = player.GetComponent<SpriteRenderer>().sortingOrder;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.Instance;
            playerSortOrder = player.GetComponent<SpriteRenderer>().sortingOrder;
        }

        RaycastHit2D grabCheck = Physics2D.RaycastAll(grabDetect.position, Vector2.right * transform.localScale, rayDist).FirstOrDefault(x => x.collider.CompareTag("Box"));  // uses raycast starting at GrabDetect to get object info

        Debug.DrawRay(transform.position + new Vector3(0.2f, 0, 0), Vector3.right * rayDist, Color.green);

        if (grabCheck.collider != null && grabCheck.collider.CompareTag("Box"))  // if there is an object colliding AND that object is a box
        {

            GameObject box = grabCheck.collider.gameObject;
            Rigidbody2D rb = box.GetComponent<Rigidbody2D>();

            LeftOutline = box.GetComponent<InteractableObjectScript>().LeftOutline;
            RightOutline = box.GetComponent<InteractableObjectScript>().RightOutline;
            Default = box.GetComponent<InteractableObjectScript>().Default;
            WholeOutline = box.GetComponent<InteractableObjectScript>().Outline;

            currentBox = box;

            DisplayOutline(box);

            if (!InteractionOver && TextCanvas != null && TextObject != null && Text != null)
            {
                DisplayPushText();
            }
            
            if (Input.GetKey(Controls.Push))  // if player is pressing space (pushing)
            {
                player.GetComponent<SpriteRenderer>().sortingOrder = box.GetComponent<SpriteRenderer>().sortingOrder + 1;  // allows the appearance of objects being behind / above others
                
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
                        aSource.time = UnityEngine.Random.Range(0, aSource.clip.length);
                        aSource.Play();
                    }
                } else if (aSource.isPlaying) {
                    // ok so this kept replaying and pausing almost every frame so i need a
                    // "grace period" of sorts to not immediately pause the first time velocity = 0
                    if (gracePeriod > 2)
                    {
                        aSource.Pause();
                    } else {
                        gracePeriod++;
                    }
                }
            }
            // if push key is not being held down and the player was pushing last frame, now they are not pushing
            else if (player.GetState().IsOneOf(PlayerState.Pulling, PlayerState.Pushing))
            {
                player.GetComponent<SpriteRenderer>().sortingOrder = playerSortOrder;  // resets to player's original sort order
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
                if (currentBox.name == obj.name)
                {
                    obj.GetComponent<SpriteRenderer>().sprite = Default;  // if box out of range, does not show outline
                    obj.GetComponent<AudioSource>().Pause();
                }
            }
            spritesToReset.Clear();

            if (!TextRemoved) { TurnOffText(); }
        }


    }

    private void DisplayOutline(GameObject obj)
    {
        if (obj != null)
        {
            spritesToReset.Add(obj);
            if (player.facingRight && LeftOutline != null)
            {
                obj.GetComponent<SpriteRenderer>().sprite = LeftOutline;
            }
            else if (!player.facingRight && RightOutline != null)
            {
                obj.GetComponent<SpriteRenderer>().sprite = RightOutline;
            }
            else
            {
                obj.GetComponent<SpriteRenderer>().sprite = WholeOutline;
            }
        }
    }

    public void DisplayPushText()
    {
        // displays text for X amount of seconds AFTER player has pushed or pulled an object
        // NOTE: Collider must accomodate area player could move in, player has to push/pull while inside
        TextRemoved = false;
        if (!(player.GetState() == PlayerState.Pushing || player.GetState() == PlayerState.Pulling) && !InteractionOver)  // if not pushing || pulling && hasn't happened already (no reactivation)
        {
            TextCanvas.SetActive(true);  // turn on appropriate canvases
            TextObject.SetActive(true);

            TextObject.GetComponent<TextMeshProUGUI>().text = Text;  // set prompt text (first string in list)
        }
        else  // once pushed or pull
        {
            InteractionOver = true;  // interaction has happened (no reactivation)
            StartCoroutine(RemoveTextAfterWait(HoldFor));  // Will turn text off after FreezeTime Seconds
        }
    }

    private void TurnOffText()
    {
        if (!InteractionOver && TextCanvas != null && TextObject != null && Text != null)
        {
            TextCanvas.SetActive(false);  // turn off appropriate canvases
            TextObject.SetActive(false);
            TextRemoved = true;
        }
    }

    private IEnumerator RemoveTextAfterWait(float Seconds)
    {
        // will turn text off after X amount of seconds (currently useful for push/pull text)

        yield return new WaitForSeconds(Seconds);
        TextObject.SetActive(false);
        TextRemoved = true;
    }

    public string NameOfObjInCast()
    {
        return currentBox == null? "null": currentBox.name;
    }
}
