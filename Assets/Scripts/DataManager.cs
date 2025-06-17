using UnityEngine;

public static class DataManager
{
    private const string CurrentLevelKey = "PlayerCurrentLevel";

    public static void SaveCurrentLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, levelIndex);
        PlayerPrefs.Save();
    }

    public static int LoadCurrentLevel()
    {
        return PlayerPrefs.GetInt(CurrentLevelKey, 0);
    }
}