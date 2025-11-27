using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;

public class PlayerNamesDisplay : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_Text player1Text;
    public TMP_Text player2Text;

    private void Start()
    {
        UpdatePlayerNames();
    }

    public override void OnJoinedRoom()
    {
        UpdatePlayerNames();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerNames();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerNames();
    }

    private void UpdatePlayerNames()
    {
        // Clear both fields first
        player1Text.text = "";
        player2Text.text = "";

        // Sort players by join order (ActorNumber)
        List<Player> sortedPlayers = new List<Player>(PhotonNetwork.PlayerList);
        sortedPlayers.Sort((a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

        if (sortedPlayers.Count >= 1)
        {
            player1Text.text = "Player 1: " + sortedPlayers[0].NickName;
        }

        if (sortedPlayers.Count >= 2)
        {
            player2Text.text = "Player 2: " + sortedPlayers[1].NickName;
        }
    }
}
