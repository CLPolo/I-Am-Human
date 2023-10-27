using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{

    private float length, startpos;
    public GameObject Camera;
    public float ParallaxEffect;


    // Start is called before the first frame update
    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        float temp = (Camera.transform.position.x * (1 - ParallaxEffect));
        float dist = (Camera.transform.position.x * ParallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        if (temp > (startpos + length))
        {
            startpos += length;
        }
        else if (temp < (startpos - length))
        {
            startpos -= length;
        }
    }
}
