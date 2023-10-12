using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTargetScript : MonoBehaviour
{
    public GameObject AffectedObject = null;

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
        if (other.gameObject.tag == "Box" ||  other.gameObject.tag == "Moveable")
        {
            if (AffectedObject != null)
            {
                Destroy(AffectedObject);
            }
        }
    }


}
