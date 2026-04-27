using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabLogin : MonoBehaviour
{
    void Start()
    {
        PlayFabSettings.staticSettings.TitleId = "1F61C7";

        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(
            request,
            result => Debug.Log("Logged in to PlayFab"),
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }
}
