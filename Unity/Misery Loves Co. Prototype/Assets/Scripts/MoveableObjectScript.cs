using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObjectScript : MonoBehaviour
{

    public int PlayerSpeed = 5;
    public GameObject Player;

    private bool Initiate_Move = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.A))
            {
                // MovePlayerToEdge();
                transform.position += Vector3.left * Time.deltaTime * PlayerSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                // MovePlayerToEdge();
                transform.position += Vector3.right * Time.deltaTime * PlayerSpeed;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        Initiate_Move = true;
    }

    private void MovePlayerToEdge()
    {
        if (Initiate_Move && Input.GetKey(KeyCode.A)) 
        {
            Player.transform.position += transform.localPosition * Time.deltaTime;
        }
        if (Initiate_Move && Input.GetKey(KeyCode.D))
        {
            Player.transform.position -= transform.localPosition * Time.deltaTime;
        }
        Initiate_Move = false;
    }
}
