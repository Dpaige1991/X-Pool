using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AvatarSelectionManager : MonoBehaviour
{
    [Header("Profile UI")]
    public TMP_InputField nameInputField;
    public Button saveButton;

    [Header("Avatar Scroll View Content")]
    public Transform avatarContent;

    [Header("Scene Transition")]
    public string nextSceneName;

    private bool initialized = false;

    private void OnEnable()
    {
        TryInitialize();
    }

    void TryInitialize()
    {
        if (initialized) return;

        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("PlayerDataManager not ready yet.");
            return;
        }

        if (PlayerDataManager.Instance.CurrentPlayerData == null)
        {
            Debug.LogWarning("PlayerData not loaded yet.");
            return;
        }

        RegisterAvatarButtons();

        saveButton.onClick.RemoveAllListeners();
        saveButton.onClick.AddListener(SaveProfile);

        initialized = true;
    }

    void RegisterAvatarButtons()
    {
        if (avatarContent == null) return;

        foreach (Transform avatar in avatarContent)
        {
            Button button = avatar.GetComponent<Button>();
            Image avatarImage = avatar.GetComponent<Image>();

            if (button == null || avatarImage == null)
                continue;

            string avatarId = avatar.gameObject.name;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SelectAvatar(avatarId, avatarImage.sprite);
            });
        }
    }

    void SelectAvatar(string avatarId, Sprite avatarSprite)
    {
        PlayerDataManager.Instance.CurrentPlayerData.selectedAvatarId = avatarId;
        PlayerDataManager.Instance.CurrentPlayerData.avatarSprite = avatarSprite.name;
    }

    void SaveProfile()
    {
        PlayerData data = PlayerDataManager.Instance.CurrentPlayerData;
        if (data == null) return;

        data.playerName = nameInputField != null
            ? nameInputField.text.Trim()
            : "";

        PlayerDataManager.Instance.SaveData();

        // Switch scene AFTER saving
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name not set on AvatarSelectionManager.");
        }
    }

    private void Update()
    {
        // One-time delayed init fallback
        if (!initialized)
        {
            TryInitialize();
        }
    }
}

