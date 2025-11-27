using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;

public class AvatarSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField playerNameInput;
    public TMP_Text avatarName;
    public Image selectedAvatarPreview;
    public Button saveButton;
    public GameObject avatarButtonContainer; // Parent with avatar buttons (each button represents an avatar image)

    public string selectedAvatarName;

    public GameObject mainMenuPanel;

    void Start()
    {
        // Add click listeners to all avatar buttons in the container
        foreach (Button btn in avatarButtonContainer.GetComponentsInChildren<Button>())
        {
            string avatarName = btn.name; // Button name represents avatar image name
            btn.onClick.AddListener(() => OnAvatarSelected(avatarName));
        }

        saveButton.onClick.AddListener(SavePlayerProfile);
    }

    private void OnAvatarSelected(string avatarName)
    {
        selectedAvatarName = avatarName;
        Debug.Log("Selected Avatar: " + avatarName);

        // Optional: Update preview image (assuming button has Image component)
        Button selectedButton = avatarButtonContainer.transform.Find(avatarName)?.GetComponent<Button>();
        Debug.Log(selectedButton.GetComponent<Image>().sprite);
        if (selectedButton != null)
        {
            selectedAvatarPreview = selectedButton.GetComponent<Image>();
        }
    }

    private void SavePlayerProfile()
    {
        string playerName = playerNameInput.text;

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Player name is empty!");
            return;
        }

        if (string.IsNullOrEmpty(selectedAvatarName))
        {
            Debug.LogWarning("No avatar selected!");
            return;
        }

        var data = new Dictionary<string, string>()
        {
            { "PlayerName", playerName },
            { "AvatarName", selectedAvatarName }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = data
        },
        result =>
        {
            Debug.Log("Player data saved successfully!");
            mainMenuPanel.SetActive(true);
            this.gameObject.SetActive(false);
        },
        error =>
        {
            Debug.LogError("Error saving data: " + error.GenerateErrorReport());
        });
    }

    private void LoadPlayerProfile()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
        result =>
        {
            if (result.Data != null)
            {
                if (result.Data.ContainsKey("PlayerName"))
                {
                    string playerName = result.Data["PlayerName"].Value;
                    avatarName.text = playerName;
                    Debug.Log("Loaded Player Name: " + playerName);
                }

                if (result.Data.ContainsKey("AvatarName"))
                {
                    selectedAvatarName = result.Data["AvatarName"].Value;
                    Debug.Log("Loaded Avatar Name: " + selectedAvatarName);

                    // Update avatar preview
                    Button savedButton = avatarButtonContainer.transform.Find(selectedAvatarName)?.GetComponent<Button>();
                    if (savedButton != null)
                    {
                        selectedAvatarPreview.sprite = savedButton.GetComponent<Image>().sprite;
                    }
                }

                mainMenuPanel.SetActive(true);
                this.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("No saved player data found.");
            }
        },
        error =>
        {
            Debug.LogError("Error loading data: " + error.GenerateErrorReport());
        });
    }
}
