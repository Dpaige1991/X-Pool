using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BackToMenuBeforeMatch : MonoBehaviourPunCallbacks
{
    [Header("Main Menu Scene Name")]
    public string mainMenuScene = "MainMenu";

    [Header("Match State")]
    public bool potCreated = false;  // This will be set to TRUE by your pot script when pot is made.

    // Called when player presses BACK button
    public void OnBackButtonPressed()
    {
        // If pot already created, DO NOT allow exit
        if (potCreated)
        {
            Debug.LogWarning("Cannot exit, pot already created!");
            return;
        }

        // Leave room normally
        StartCoroutine(LeaveRoomAndGoToMenu());
    }

    private System.Collections.IEnumerator LeaveRoomAndGoToMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        // Wait until left the room
        while (PhotonNetwork.InRoom)
            yield return null;

        // Load main menu
        PhotonNetwork.LoadLevel(mainMenuScene);
    }

    // Safety callback: If something forces room exit
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(mainMenuScene);
    }

    // If disconnected unexpectedly
    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel(mainMenuScene);
    }
}
