using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunableAreaScript : MonoBehaviour
{

    public GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player.GetComponent<Player>().isRunning = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player.GetComponent<Player>().isRunning = false;
    }

}
