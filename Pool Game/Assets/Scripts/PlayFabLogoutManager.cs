using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

/// <summary>
/// Handles PlayFab logout by clearing client session credentials.
/// Does not use PlayerPrefs and does not change scene.
/// </summary>
public class PlayFabLogoutManager : MonoBehaviour
{
    /// <summary>
    /// Logs out the player by clearing the PlayFab client session.
    /// </summary>
    public void Logout()
    {
        // Forget all PlayFab credentials
        PlayFabClientAPI.ForgetAllCredentials();
        Debug.Log("✅ PlayFab session cleared. Player logged out.");
    }

    /// <summary>
    /// Optional: Delete the player's PIN from PlayFab UserData
    /// Call only if you want to force the player to create a new PIN on next login
    /// </summary>
    public void ClearPinFromPlayFab()
    {
        var request = new UpdateUserDataRequest
        {
            KeysToRemove = new System.Collections.Generic.List<string> { "SecurityPIN" }
        };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log("✅ PIN cleared from PlayFab.");
        }, error =>
        {
            Debug.LogError("❌ Failed to clear PIN: " + error.GenerateErrorReport());
        });
    }
}
