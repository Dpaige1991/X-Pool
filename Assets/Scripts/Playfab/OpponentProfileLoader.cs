using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
public class OpponentProfileLoader : MonoBehaviourPunCallbacks
{
    [Header("UI - Opponent (Right Side)")]
    public TMPro.TextMeshProUGUI opponentNameText;
    public TMPro.TextMeshProUGUI opponentLevelText;
    public UnityEngine.UI.Image opponentAvatarImage;

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            LoadOpponentData(newPlayer);
        }
    }

    private void LoadOpponentData(Player opponent)
    {
        Debug.Log("Second player entered. Loading opponent PlayFab data...");

        string opponentPlayFabId = opponent.UserId;

        GetUserDataRequest request = new GetUserDataRequest
        {
            PlayFabId = opponentPlayFabId
        };

        PlayFabClientAPI.GetUserData(request, result =>
        {
            string name = result.Data.ContainsKey("DisplayName") ? result.Data["DisplayName"].Value : "Opponent";
            string level = result.Data.ContainsKey("Level") ? result.Data["Level"].Value : "1";
            string avatar = result.Data.ContainsKey("Avatar") ? result.Data["Avatar"].Value : "";

            opponentNameText.text = name;
            opponentLevelText.text = "Lvl " + level;

            Debug.Log("Opponent data loaded successfully.");

        }, error =>
        {
            Debug.LogError("Failed to load opponent PlayFab data: " + error.GenerateErrorReport());
        });
    }
}
