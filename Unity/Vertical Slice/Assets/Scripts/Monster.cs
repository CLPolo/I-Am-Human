using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Monster : MonoBehaviour
{
    public InteractableObjectScript teddy;
    public float speed = 1.5f;

    private Vector3 originalPosition = new Vector3(10, 0, 3);
    private LogicScript logic;
    private AudioSource audioSource;
    private Door door;
    // Start is called before the first frame update
    void Start()
    {
        logic = LogicScript.Instance;
        audioSource = GetComponent<AudioSource>();
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        door = (Door)FindObjectOfType(typeof(Door));
        door.GetComponent<BoxCollider2D>().enabled = false;
        door.GetComponent<SpriteRenderer>().enabled = false;

    }

    // Update is called once per frame
    void Update()
    {   

        if (teddy.monsterComing)
        {
            //kind of jank; only reveal the monster and the door after the teddy has been interacted with
            GetComponent<BoxCollider2D>().enabled = true;
            GetComponent<SpriteRenderer>().enabled = true;
            door.GetComponent<BoxCollider2D>().enabled = true;
            door.GetComponent<SpriteRenderer>().enabled = true;
            if(!audioSource.isPlaying){ audioSource.Play(); }
            // The teddy has been interacted with, start walking to the left slowly
            
            transform.position += Vector3.left * Time.deltaTime * speed;
        }
        if (transform.position.x < -15)
        {
            audioSource.volume *= 0.90f;
            if(audioSource.volume <0.01)
            {
                audioSource.Stop();
                Destroy(gameObject);
            }
        }
    }
}
