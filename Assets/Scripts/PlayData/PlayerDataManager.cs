using UnityEngine;
using System.IO;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    private string filePath;
    public PlayerData CurrentPlayerData;
    public GameObject SecurityMenu;
    public GameObject CreatePinMenu;
    public GameObject EnterPinMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.persistentDataPath, "playerdata.json");
            Debug.Log(filePath);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SignIn(string signInMethod)
    {
        if (File.Exists(filePath))
        {
            LoadData();
        }
        else
        {
            CreateNewData(signInMethod);
        }
    }

    void CreateNewData(string signInMethod)
    {
        CurrentPlayerData = new PlayerData(signInMethod);
        SaveData();
        Debug.Log("New player data created");
        SecurityMenu.SetActive(true);
        CreatePinMenu.SetActive(true);
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(CurrentPlayerData, true);
        File.WriteAllText(filePath, json);
    }

    void LoadData()
    {
        string json = File.ReadAllText(filePath);
        CurrentPlayerData = JsonUtility.FromJson<PlayerData>(json);
        Debug.Log("Player data loaded");
        SecurityMenu.SetActive(true);
        EnterPinMenu.SetActive(true);
    }
}
