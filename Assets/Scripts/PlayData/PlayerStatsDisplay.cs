using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text playerNameText;
    public TMP_Text levelText;
    public TMP_Text coinsText;
    public TMP_Text gemsText;
    public TMP_Text coinsText_Friends;
    public TMP_Text gemsText_Friends;
    public Slider xpSlider;

    private string filePath;
    private PlayerData playerData;

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

        LoadPlayerData();
        UpdateUI();
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
            Debug.LogWarning("Player data not found. Creating new data.");
            playerData = new PlayerData("Unknown");
            SavePlayerData();
        }
    }

    public void AddXP(int amount)
    {
        playerData.currentXP += amount;

        // Check for level up
        while (playerData.currentXP >= playerData.xpToNextLevel)
        {
            playerData.currentXP -= playerData.xpToNextLevel;
            playerData.level++;

            // Optional: increase next level XP requirement
            playerData.xpToNextLevel = Mathf.RoundToInt(playerData.xpToNextLevel * 1.2f);

            Debug.Log($"Level Up! New Level: {playerData.level}");
        }

        UpdateUI();
        SavePlayerData();
    }

    void UpdateUI()
    {
        if (playerData != null)
        {
            playerNameText.text = playerData.playerName;
            levelText.text = $"{playerData.level}";
            coinsText.text = $"{playerData.coins}";
            gemsText.text = $"{playerData.gems}";
            coinsText_Friends.text = $"{playerData.coins}";
            gemsText_Friends.text = $"{playerData.gems}";

            if (xpSlider != null)
            {
                xpSlider.maxValue = playerData.xpToNextLevel;
                xpSlider.value = playerData.currentXP;
            }
        }
    }

    void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, json);
    }
}

