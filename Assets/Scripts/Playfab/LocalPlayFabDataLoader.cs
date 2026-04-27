using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


public class LocalPlayFabDataLoader : MonoBehaviour
{
    [Header("UI - Local Player (Always Left)")]
    public TMPro.TextMeshProUGUI localNameText;
    public TMPro.TextMeshProUGUI localLevelText;
    public UnityEngine.UI.Image localAvatarImage;

    public void LoadLocalPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            string name = result.Data.ContainsKey("DisplayName") ? result.Data["DisplayName"].Value : "Player";
            string level = result.Data.ContainsKey("Level") ? result.Data["Level"].Value : "1";
            string avatar = result.Data.ContainsKey("Avatar") ? result.Data["Avatar"].Value : "";

            // Update UI
            localNameText.text = name;
            localLevelText.text = level;

        }, error =>
        {
            Debug.LogError("Error loading local player data: " + error.GenerateErrorReport());
        });
    }
}

