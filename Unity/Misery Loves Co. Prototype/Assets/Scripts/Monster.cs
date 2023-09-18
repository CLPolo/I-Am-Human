using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public InteractableObjectScript teddy;
    public float speed = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (teddy.monsterComing)
        {
            // The teddy has been interacted with, start walking to the left slowly
            transform.position += Vector3.left * Time.deltaTime * speed;
        }
    }
}
