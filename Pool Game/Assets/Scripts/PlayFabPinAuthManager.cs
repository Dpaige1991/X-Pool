using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayFabPinAuthManager : MonoBehaviour
{
    public Button signupButton;
    public Button appleSignInButton;
    public GameObject pinPanel;
    public GameObject securityPanel;
    public TMP_Text statusText;

    public string storedEmail;
    public string storedPassword;

    public StarterRewardInitializer rewardInitializer;

    void Start()
    {
        pinPanel.SetActive(false);
        statusText.text = "";

        signupButton.onClick.AddListener(OnSignup);
        appleSignInButton.onClick.AddListener(OnAppleSignIn);

        rewardInitializer = GetComponent<StarterRewardInitializer>();
    }

    // SIGNUP NEW USER
    public void OnSignup()
    {
        string email = storedEmail.Trim();
        string password = storedPassword.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter both email and password.";
            return;
        }

        statusText.text = "Creating account...";

        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = false,
            TitleId = PlayFabSettings.staticSettings.TitleId
        };

        PlayFabClientAPI.RegisterPlayFabUser(
            registerRequest,
            result =>
            {
                statusText.text = "✅ Account created and logged in!";
                Debug.Log("New player registered: " + email);
                ShowPinInput();
                rewardInitializer.CheckIfStarterGiven();
            },
            error =>
            {
                statusText.text = "Signup failed: " + error.GenerateErrorReport();
            });
    }

    public void ShowPinInput()
    {
        pinPanel.SetActive(true);
    }

    public void ShowSecurityInput()
    {
        securityPanel.SetActive(true);
    }

    public void OnAppleSignIn()
    {
        statusText.text = "Apple Sign-In not yet implemented.";
    }

    // ------------------------------------------
    // 🔧 DEBUG SIGN-IN (Inspector Button)
    // ------------------------------------------

    [ContextMenu("DEBUG: Sign In (Using Stored Email/Password)")]
    public void DebugSignIn()
    {
        if (string.IsNullOrEmpty(storedEmail) || string.IsNullOrEmpty(storedPassword))
        {
            Debug.LogWarning("DebugSignIn failed — email or password empty.");
            statusText.text = "Debug login failed: Missing email/password.";
            return;
        }

        statusText.text = "Debug Sign-In...";

        var req = new LoginWithEmailAddressRequest
        {
            Email = storedEmail,
            Password = storedPassword,
            TitleId = PlayFabSettings.staticSettings.TitleId
        };

        PlayFabClientAPI.LoginWithEmailAddress(
            req,
            result =>
            {
                Debug.Log("DEBUG: Logged in as " + storedEmail);
                statusText.text = "Debug sign-in success!";
                ShowPinInput();
            },
            error =>
            {
                Debug.LogError("DEBUG LOGIN FAILED: " + error.ErrorMessage);
                statusText.text = "Debug login failed: " + error.ErrorMessage;
            });
    }
}

#if UNITY_EDITOR


[CustomEditor(typeof(PlayFabPinAuthManager))]
public class PlayFabPinAuthManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayFabPinAuthManager manager = (PlayFabPinAuthManager)target;

        GUILayout.Space(10);

        if (GUILayout.Button("DEBUG: Sign In"))
        {
            manager.DebugSignIn();
        }
    }
}
#endif

