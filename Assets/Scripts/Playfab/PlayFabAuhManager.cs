using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayFabAuthManager : MonoBehaviour
{
    [Header("Email Login")]
    public string userEmail;
    public string userPassword;
    public string username;

    [Header("Apple Login")]
    public string appleIdToken; // Get this from Apple Sign-In plugin

    [Header("PIN System")]
    public PlayFabPinSystem pinSystem; // Reference to separate PIN manager

    // -------- EMAIL REGISTRATION --------
    public void RegisterWithEmail()
    {
        if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userPassword))
        {
            Debug.LogError("Email or Password cannot be empty!");
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Email = userEmail,
            Password = userPassword,
            Username = username,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("✅ Registration successful! Welcome " + result.Username);
        PlayerPrefs.SetString("PlayFabId", result.PlayFabId);

        // After registration, check PIN
        pinSystem.CheckForPin();
    }

    // -------- EMAIL LOGIN --------
    public void LoginWithEmail()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = userEmail,
            Password = userPassword
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    // -------- APPLE LOGIN --------
    public void LoginWithApple()
    {
        if (string.IsNullOrEmpty(appleIdToken))
        {
            Debug.LogError("Missing Apple ID Token.");
            return;
        }

        var request = new LoginWithAppleRequest
        {
            IdentityToken = appleIdToken,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithApple(request, OnLoginSuccess, OnError);
    }

    // -------- COMMON LOGIN SUCCESS --------
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("✅ Login successful! PlayFabId: " + result.PlayFabId);
        PlayerPrefs.SetString("PlayFabId", result.PlayFabId);

        // Trigger PIN check after login
        pinSystem.CheckForPin();
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("❌ PlayFab Error: " + error.GenerateErrorReport());
    }
}
