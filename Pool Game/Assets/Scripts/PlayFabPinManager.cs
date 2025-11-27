using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatePinManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject createPinPanel;
    public GameObject confirmPanel;
    public GameObject enterPinPanel;
    public GameObject avatarPanel;
    public GameObject mainMenuPanel;

    [Header("PIN UI")]
    public TMP_InputField createPinInput;
    public TMP_InputField confirmPinInput;
    public TMP_InputField enterPinInput;

    [Header("Settings")]
    public float messageDuration = 2f;

    private string _playFabId;

    // -------------------- PANEL CONTROL --------------------

    void ShowPanel(GameObject panelToShow)
    {
        loginPanel.SetActive(false);
        createPinPanel.SetActive(false);
        enterPinPanel.SetActive(false);

        if (panelToShow != null)
            panelToShow.SetActive(true);
    }

    // -------------------- LOGIN & SIGNUP --------------------

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("✅ Logged in: " + result.PlayFabId);
        _playFabId = result.PlayFabId;

        CheckIfPlayerHasPIN();
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("🆕 Account created: " + result.PlayFabId);
        _playFabId = result.PlayFabId;

        // New users always need to create a PIN
        ShowPanel(createPinPanel);
    }

    // -------------------- CHECK FOR EXISTING PIN --------------------

    public void CheckIfPlayerHasPIN()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey("PlayerPIN"))
                {
                    Debug.Log("🔐 Existing PIN found.");
                    ShowPanel(enterPinPanel);
                }
                else
                {
                    Debug.Log("🆕 No PIN found. Creating one...");
                    ShowPanel(createPinPanel);
                }
            },
            error =>
            {
                Debug.LogError("Error checking PIN: " + error.GenerateErrorReport());
                ShowPanel(createPinPanel);
            });
    }

    // -------------------- CREATE PIN --------------------

    public void OnCreatePinButton()
    {
        if (createPinInput.text != confirmPinInput.text)
        {
            Debug.LogWarning("❌ PINs do not match!");
            return;
        }

        var pinValue = createPinInput.text;

        var request = new UpdateUserDataRequest
        {
            Data = new System.Collections.Generic.Dictionary<string, string>
            {
                { "PlayerPIN", pinValue }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                Debug.Log("✅ PIN saved successfully!");
                ShowPanel(confirmPanel);
            },
            error => Debug.LogError("Error saving PIN: " + error.GenerateErrorReport()));
    }

    // -------------------- ENTER PIN (Verification) --------------------

    public void OnConfirmPinButton()
    {
        string enteredPin = enterPinInput.text;

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey("PlayerPIN"))
                {
                    string storedPin = result.Data["PlayerPIN"].Value;
                    if (enteredPin == storedPin)
                    {
                        Debug.Log("✅ Correct PIN. Proceeding to main game...");
                        // Load next scene or show main menu here
                        ShowPanel(avatarPanel);
                    }
                    else
                    {
                        Debug.LogWarning("❌ Incorrect PIN!");
                    }
                }
            },
            error => Debug.LogError("Error verifying PIN: " + error.GenerateErrorReport()));
    }

    public void OnEnterPinButton()
    {
        string enteredPin = enterPinInput.text;

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey("PlayerPIN"))
                {
                    string storedPin = result.Data["PlayerPIN"].Value;
                    if (enteredPin == storedPin)
                    {
                        Debug.Log("✅ Correct PIN. Proceeding to main game...");
                        // Load next scene or show main menu here
                        ShowPanel(mainMenuPanel);
                    }
                    else
                    {
                        Debug.LogWarning("❌ Incorrect PIN!");
                    }
                }
            },
            error => Debug.LogError("Error verifying PIN: " + error.GenerateErrorReport()));
    }

    // -------------------- LOGOUT --------------------

    public void OnCancelButton()
    {
        Debug.Log("🚪 Logging out...");
        PlayFabClientAPI.ForgetAllCredentials();
        ShowPanel(loginPanel);
    }

    // -------------------- ERROR HANDLER --------------------

    private void OnError(PlayFabError error)
    {
        Debug.LogError("❌ PlayFab Error: " + error.GenerateErrorReport());
        ShowPanel(loginPanel);
    }
}
