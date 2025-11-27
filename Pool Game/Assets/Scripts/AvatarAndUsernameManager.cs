using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class AvatarAndUsernameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public Image selectedAvatarDisplay;
    public List<Button> avatarButtons;

    private int selectedAvatarIndex = 0;

    private void Start()
    {
        // Setup button listeners
        for (int i = 0; i < avatarButtons.Count; i++)
        {
            int index = i;
            avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }

        // Load saved data
        LoadPlayerData();
    }

    public void SelectAvatar(int index)
    {
        selectedAvatarIndex = index;

        // Get the sprite directly from the button's Image component
        Sprite avatarSprite = avatarButtons[index].GetComponent<Image>().sprite;
        selectedAvatarDisplay.sprite = avatarSprite;
    }

    public void SavePlayerData()
    {
        string username = usernameInput.text;

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"Username", username},
                {"AvatarIndex", selectedAvatarIndex.ToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaved, OnError);
    }

    private void OnDataSaved(UpdateUserDataResult result)
    {
        Debug.Log("Player data saved successfully!");
    }

    private void LoadPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataLoaded, OnError);
    }

    private void OnDataLoaded(GetUserDataResult result)
    {
        if (result.Data != null)
        {
            if (result.Data.ContainsKey("Username"))
                usernameInput.text = result.Data["Username"].Value;

            if (result.Data.ContainsKey("AvatarIndex"))
            {
                int index;
                if (int.TryParse(result.Data["AvatarIndex"].Value, out index))
                {
                    selectedAvatarIndex = index;
                    Sprite avatarSprite = avatarButtons[index].GetComponent<Image>().sprite;
                    selectedAvatarDisplay.sprite = avatarSprite;
                }
            }
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab error: " + error.GenerateErrorReport());
    }
}
