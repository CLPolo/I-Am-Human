using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonMash : MonoBehaviour
{
    public float mashDelay = 0.5f;  // If you don't mash for 0.5 seconds you die
    public GameObject text;
    public LogicScript logicScript;
    public GameObject Logic;

    private float mash;
    private bool pressed;
    private bool started;

    // Start is called before the first frame update
    void Start()
    {
        logicScript = Logic.GetComponent<LogicScript>();
        mash = mashDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            text.SetActive(true);
            mash -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) && !pressed)
            {
                pressed = true;
                mash = mashDelay;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                pressed = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Trap")
        {
            // You are touching the trap
            started = true;
        }
    }
}
