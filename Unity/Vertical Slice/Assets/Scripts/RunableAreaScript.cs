using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunableAreaScript : MonoBehaviour
{

    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        player.SetState(PlayerState.Running);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        player.SetState(PlayerState.Walking);
    }

}
