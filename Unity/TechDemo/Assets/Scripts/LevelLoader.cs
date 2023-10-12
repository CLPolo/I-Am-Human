using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    // temporary linear scene progression
    private Dictionary<string, string> nextScene = new Dictionary<string, string>()
    {
        { "Grab Test", "Running Area" },
        { "Running Area", "Button Mash" },
        { "Button Mash", "Flashlight Test" },
        { "Flashlight Test", "Grab Test" }
    };

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Door")
        {
            SceneManager.LoadScene(nextScene[getSceneName()]);
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

    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ExitToMain(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
