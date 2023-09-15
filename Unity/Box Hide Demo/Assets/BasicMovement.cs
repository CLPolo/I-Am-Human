using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    public float defaultSpeed = 5f;
    public float speed;
    public SpriteRenderer spriteRender;
    public Sprite standing;
    public Sprite hiding;

    private void Start()
    {
        resetPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.up * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.down * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * Time.deltaTime * speed;
        }
    }

    // This is called every frame that the character is touching "Hideable"
    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("TOUCHING");
        // This object is hideable... ADD POPUP TEXT HERE "press space to hide !"
        if ((collision.gameObject.tag == "Hideable") && Input.GetKey(KeyCode.Space))
        {
            // Space is held. Set speed to 0.8 (slow moving!) and change graphics
            spriteRender.sprite = hiding;
            speed = 0.8f;
            transform.position += Vector3.right*0.0001f;
            // add fog here
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            resetPlayer();
        }
        else
        {
            resetPlayer();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Hideable")
        {
            // remove fog
            resetPlayer();
        }
    }

    private void resetPlayer()
    {
        Debug.Log("RESET");
        speed = defaultSpeed;
        spriteRender.sprite = standing;
        transform.position += Vector3.left * 0.0001f;
    }
}
