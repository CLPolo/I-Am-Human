using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]  // This means we can save it in a file
public class LevelSaveData
{

    public string currentScene;  // The scene that the user saved the game in

    public LevelSaveData(LevelLoader level)
    {
        currentScene = level.getSceneName();
    }
}
