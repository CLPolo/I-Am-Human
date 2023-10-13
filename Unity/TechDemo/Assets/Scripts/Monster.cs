using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Monster : MonoBehaviour
{
    public InteractableObjectScript teddy;
    public GameObject Logic;
    public float speed = 1.5f;

    private Vector3 originalPosition = new Vector3(10, 0, 3);
    private LogicScript logicScript = null;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        logicScript = Logic.GetComponent<LogicScript>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {   
        if (teddy.monsterComing && !logicScript.IsPaused)
        {
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
