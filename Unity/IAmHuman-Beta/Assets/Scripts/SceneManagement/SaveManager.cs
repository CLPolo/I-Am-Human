using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;  // Save as a binary so it is less editable by user

// NOTE: I GOT ALL THIS SAVE INFO FROM https://www.youtube.com/watch?v=XOjd_qU2Ido
// RIGHT NOW, ONLY SAVING WORKS ON A SCENE IF THE PLAYER HAS LevelLoader.cs ATTACHED TO IT
public static class SaveManager
{
    public static void SaveLevel (LevelLoader level)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string saveFilePath = Application.persistentDataPath + "/level.data";  // The first bit is just a location that Unity has ready for us
        FileStream stream = new FileStream(saveFilePath, FileMode.Create);

        LevelSaveData levelData = new LevelSaveData(level);

        formatter.Serialize(stream, levelData);
        stream.Close();
    }

    public static LevelSaveData LoadLevel()
    {
        string saveFilePath = Application.persistentDataPath + "/level.data";
        Debug.Log(Application.persistentDataPath);
        if (File.Exists(saveFilePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(saveFilePath, FileMode.Open);

            LevelSaveData data = (LevelSaveData)formatter.Deserialize(stream);  // Cast to correct type (LevelSaveData)
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + saveFilePath);
            return null;
        }
    }
}
