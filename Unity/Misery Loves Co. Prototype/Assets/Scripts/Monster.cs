using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public InteractableObjectScript teddy;
    public GameObject Logic;
    public float speed = 1.5f;

    private LogicScript logicScript = null;

    // Start is called before the first frame update
    void Start()
    {
        logicScript = Logic.GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (teddy.monsterComing && !logicScript.IsDead && !logicScript.IsPaused)
        {
            // The teddy has been interacted with, start walking to the left slowly
            transform.position += Vector3.left * Time.deltaTime * speed;
        }
    }
}
