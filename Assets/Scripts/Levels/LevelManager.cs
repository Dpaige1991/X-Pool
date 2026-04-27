using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public LevelData[] levels;

    private PlayerData playerData;
    private string filePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.persistentDataPath, "playerdata.json");
            LoadPlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TryEnterLevel(string levelId)
    {
        LevelData level = GetLevel(levelId);
        if (level == null) return;

        if (playerData.coins < level.entryFee)
        {
            Debug.Log("Not enough coins to enter " + levelId);
            // TODO: Show insufficient coins UI
            return;
        }

        // Deduct coins
        playerData.coins -= level.entryFee;

        // Save immediately
        SavePlayerData();

        // Show level UI (or load scene)
        SceneManager.LoadScene(levelId);

        Debug.Log($"Entered {levelId} | Coins left: {playerData.coins}");
    }

    LevelData GetLevel(string levelId)
    {
        foreach (var lvl in levels)
        {
            if (lvl.levelId == levelId)
                return lvl;
        }

        Debug.LogError("Level not found: " + levelId);
        return null;
    }

    void LoadPlayerData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            // Fallback (should rarely happen)
            playerData = new PlayerData("guest");
        }
    }

    void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, json);
    }
}
