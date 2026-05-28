using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Drag all your LevelData ScriptableObjects here in order")]
    [SerializeField] private List<LevelData> allLevels = new List<LevelData>();

    private GameProgressData _progress;

    private void Awake()
    {
        // Standard Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keeps this alive across scene loads

        InitializeProgress();
    }

    private void InitializeProgress()
    {
        _progress = SaveSystem.LoadProgress();

        // Ensure default levels are always unlocked
        bool needsSave = false;
        foreach (LevelData level in allLevels)
        {
            if (level.UnlockedByDefault && !_progress.unlockedLevelIDs.Contains(level.LevelID))
            {
                _progress.unlockedLevelIDs.Add(level.LevelID);
                needsSave = true;
            }
        }

        if (needsSave) SaveSystem.SaveProgress(_progress);
    }

    public bool IsLevelUnlocked(string levelID)
    {
        return _progress.unlockedLevelIDs.Contains(levelID);
    }

    public void UnlockLevel(string levelID)
    {
        if (!_progress.unlockedLevelIDs.Contains(levelID))
        {
            _progress.unlockedLevelIDs.Add(levelID);
            SaveSystem.SaveProgress(_progress);
        }
    }

    public void CompleteLevel(string levelID)
    {
        if (!_progress.completedLevelIDs.Contains(levelID))
        {
            _progress.completedLevelIDs.Add(levelID);
            SaveSystem.SaveProgress(_progress);
        }
    }

    public void LoadLevel(LevelData levelToLoad)
    {
        if (IsLevelUnlocked(levelToLoad.LevelID))
        {
            SceneManager.LoadScene(levelToLoad.SceneName);
        }
        else
        {
            Debug.LogWarning($"Cannot load {levelToLoad.DisplayName}. It is locked!");
        }
    }
}