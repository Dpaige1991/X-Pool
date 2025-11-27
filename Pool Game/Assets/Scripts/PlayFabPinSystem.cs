using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Modular PIN system. Handles 4-digit PIN creation and verification.
/// </summary>
public class PlayFabPinSystem : MonoBehaviour
{
    [Header("PIN Input")]
    public string enteredPin;
    public string confirmedPin;

    public Action OnPinCreated;    // Fires after creating new PIN
    public Action OnPinVerified;   // Fires after correct PIN entered
    public Action OnPinMissing;    // Fires if no PIN exists

    private const string PinKey = "SecurityPIN";

    [SerializeField] private GameObject objectToHide1;
    [SerializeField] private GameObject objectToHide2;
    [SerializeField] private GameObject objectToHide3;
    [SerializeField] private GameObject objectToShow;

    public PlayFabLogoutManager logoutManager;

    // -------- CHECK IF PIN EXISTS --------
    public void CheckForPin()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey(PinKey))
            {
                Debug.Log("🔒 PIN exists — ask player to enter it.");
                OnPinMissing?.Invoke(); // Can show PIN input UI
            }
            else
            {
                Debug.Log("⚠ No PIN found — prompt creation.");
                OnPinMissing?.Invoke();
            }
        }, OnError);
    }

    // -------- CREATE PIN --------
    public void CreatePin()
    {
        if (!IsValidPin(enteredPin))
        {
            Debug.LogError("❌ PIN must be exactly 4 numbers.");
            return;
        }

        if (enteredPin != confirmedPin)
        {
            Debug.LogError("❌ PINs do not match!");
            return;
        }

        string hashedPin = HashPin(enteredPin);

        var request = new UpdateUserDataRequest
        {
            Data = new System.Collections.Generic.Dictionary<string, string>
            {
                { PinKey, hashedPin }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log("✅ 4-digit PIN saved securely!");
            OnPinCreated?.Invoke();
        }, OnError);
    }

    // -------- VERIFY PIN --------
    public void VerifyPin()
    {
        if (!IsValidPin(enteredPin))
        {
            Debug.LogError("❌ Invalid PIN format!");
            return;
        }

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey(PinKey))
            {
                string storedHash = result.Data[PinKey].Value;
                string inputHash = HashPin(enteredPin);

                if (storedHash == inputHash)
                {
                    Debug.Log("✅ PIN correct — access granted!");
                    OnPinVerified?.Invoke();
                }
                else
                {
                    Debug.LogError("❌ Incorrect PIN.");
                }
            }
            else
            {
                Debug.LogWarning("⚠ No PIN stored — create one.");
                OnPinMissing?.Invoke();
            }
        }, OnError);
    }

    public void Cancel()
    {
        if (objectToHide1 != null)
            objectToHide1.SetActive(false);

        if (objectToHide2 != null)
            objectToHide2.SetActive(false);

        if (objectToHide3 != null)
            objectToHide3.SetActive(false);

        if (objectToShow != null)
            objectToShow.SetActive(true);

        logoutManager.Logout();
    }

    // -------- HELPERS --------
    private bool IsValidPin(string pin)
    {
        return !string.IsNullOrEmpty(pin) && pin.Length == 4 && int.TryParse(pin, out _);
    }

    private string HashPin(string pin)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(pin);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("❌ PlayFab Error: " + error.GenerateErrorReport());
    }
}

