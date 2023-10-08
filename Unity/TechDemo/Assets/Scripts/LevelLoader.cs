using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{

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
        if (other.tag == "Level Exit")
        {
            if (SceneManager.GetActiveScene().name == "Running Area")
            {
                SceneManager.LoadScene("Grab Test");
            }
            else
            {   
                SceneManager.LoadScene("Running Area");
            }    
        }
    }


}
