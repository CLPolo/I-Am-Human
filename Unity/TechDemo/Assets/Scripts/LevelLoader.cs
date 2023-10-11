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
        if (Input.GetKey(KeyCode.LeftBracket))
        {
            // TEMPORARY "SAVE GAME" button until we organize more
            SaveScene();
        }
        else if (Input.GetKey(KeyCode.RightBracket))
        {
            LoadScene();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Level Exit")
        {
            if (getSceneName() == "Running Area")
            {
                SceneManager.LoadScene("Grab Test");
            }
            else
            {   
                SceneManager.LoadScene("Running Area");
            }    
        }
    }

    public string getSceneName()
    {
        // Returns the name of the current scene
        return SceneManager.GetActiveScene().name;
    }

    public void SaveScene()
    {
        SaveManager.SaveLevel(this);
    }

    public void LoadScene()
    {
        LevelSaveData data = SaveManager.LoadLevel();
        SceneManager.LoadScene(data.currentScene);
    }

}
