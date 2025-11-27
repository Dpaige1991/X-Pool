using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public Transform levelParent; // Where levels will spawn
    public List<GameObject> levelPrefabs; // List of level prefabs
    private GameObject currentLevel;

    private int currentLevelIndex = -1;

    void Start()
    {
        // Optional: Load first level automatically
        LoadLevel(0);
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levelPrefabs.Count)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        // Destroy current level if one is loaded
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }

        // Instantiate the new level
        currentLevel = Instantiate(levelPrefabs[index], levelParent);
        currentLevelIndex = index;

        Debug.Log($"Loaded level {index}: {levelPrefabs[index].name}");
    }

    public void ReloadCurrentLevel()
    {
        if (currentLevelIndex >= 0)
        {
            LoadLevel(currentLevelIndex);
        }
    }
}
