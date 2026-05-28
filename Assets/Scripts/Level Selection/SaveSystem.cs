using System.Collections.Generic;
using UnityEngine;

// This class holds the actual data we write to disk
[System.Serializable]
public class GameProgressData
{
    // HashSets are faster, but Unity's JSON Utility only serializes Lists.
    public List<string> unlockedLevelIDs = new List<string>();
    public List<string> completedLevelIDs = new List<string>();
}

public static class SaveSystem
{
    private const string SAVE_KEY = "PlayerProgress";

    public static void SaveProgress(GameProgressData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static GameProgressData LoadProgress()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            return JsonUtility.FromJson<GameProgressData>(json);
        }

        // If no save exists, return a fresh data object
        return new GameProgressData();
    }

    // Helpful for debugging
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
    }
}