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

    // temporary linear scene progression
    private Dictionary<string, string> nextScene = new Dictionary<string, string>()
    {
        { "FOREST FIRST COMMIT", "FOREST FIRST COMMIT" }
    };

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Door" && !other.GetComponent<Door>().isLocked)
        {
            SceneManager.LoadScene(nextScene[getSceneName()]);
        }
    }

    public string getSceneName()
    {
        // Returns the name of the current scene
        return SceneManager.GetActiveScene().name;
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
