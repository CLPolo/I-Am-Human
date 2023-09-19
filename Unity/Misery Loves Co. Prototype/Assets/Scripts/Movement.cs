﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public const float defaultSpeed = 5f;
    public const float sneakSpeed = 0.8f;
    public float speed = 5f;
    public SpriteRenderer spriteRender;
    public Sprite standing;
    public Sprite hiding;
    public Monster BadGuy;
    public GameObject Logic;

    public bool isHidden = false;
    private LogicScript logicScript = null;

    // Start is called before the first frame update
    void Start()
    {
        logicScript = Logic.GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {

        // Move left
        if (Input.GetKey(KeyCode.A) && !logicScript.IsPaused)
        {
            transform.position += Vector3.left * Time.deltaTime * speed;
        }

        // Move right
        if (Input.GetKey(KeyCode.D) && !logicScript.IsPaused)
        {
            transform.position += Vector3.right * Time.deltaTime * speed;
        }

    }

    // This is called every frame that the character is touching "Hideable"
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!logicScript.IsPaused)
        {
            // This object is hideable... ADD POPUP TEXT HERE "press space to hide !"
            if (collision.gameObject.tag == "Hideable")
            {
                var hideable = collision.gameObject;
                if (Input.GetKey(KeyCode.Space))
                {
                    // Space is held. Set speed to 0.8 (slow moving!) and change graphics
                    var hideablePosition = hideable.transform.position;
                    hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, 1);
                    hideable.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    spriteRender.sprite = hiding;
                    speed = sneakSpeed;
                    transform.position += Vector3.right * 0.0001f;
                    isHidden = true;
                    // add fog here
                }
                else if (isHidden)
                {
                    unhide(hideable);
                }
                else
                {
                    hideable.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
                }
            }
            if (collision.gameObject.tag == "Bad" && !isHidden)
            {
                // This is when the monster sees you and you are not behind the box
                // Gameover can go here! For now I just freeze them
                speed = 0f;
                BadGuy.speed = 0f;
                logicScript.Death();
            }
        }
    }

    private void unhide(GameObject hideable)
    {
        var hideablePosition = hideable.transform.position;
        hideable.transform.position = new Vector3(hideablePosition.x, hideablePosition.y, -1);
        isHidden = false;
        transform.position += Vector3.left * 0.0001f;
        resetPlayer();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Hideable")
        {
            var hideable = collision.gameObject;
            if (isHidden)
            {
                // remove fog
                unhide(hideable);
            } 
            else
            {
                hideable.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    private void resetPlayer()
    {
        speed = defaultSpeed;
        spriteRender.sprite = standing;
    }
}
