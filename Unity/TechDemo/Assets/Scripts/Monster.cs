using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public GameObject Logic;
    public float speed = 6f;

    private Vector3 originalPosition;
    private LogicScript logicScript = null;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = this.transform.position;
        logicScript = Logic.GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!logicScript.IsPaused)
        {
            // The teddy has been interacted with, start walking to the left slowly
            transform.position += Vector3.left * Time.deltaTime * speed;
        }
        if (transform.position.x < -20)
        {
            this.gameObject.SetActive(false);  // dies xD
        }
    }
}
