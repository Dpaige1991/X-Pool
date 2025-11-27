using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PhotonSmartRoomJoiner : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private byte maxPlayersPerRoom = 2;

    [Header("UI")]
    public GameObject menuUI; // Assign your menu Canvas or panel here

    public void StartGame()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server!");
        PhotonNetwork.JoinLobby(); // Join lobby so we can get a list of rooms
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby — fetching room list...");
        PhotonNetwork.GetCustomRoomList(TypedLobby.Default, ""); // Trigger room list update
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"Room list updated. Found {roomList.Count} rooms.");

        // Try to find a room with exactly ONE player
        foreach (RoomInfo room in roomList)
        {
            if (room.PlayerCount == 1 && room.PlayerCount != room.MaxPlayers && !room.RemovedFromList)
            {
                Debug.Log($"Found a room with 1 player: {room.Name}. Joining...");
                PhotonNetwork.JoinRoom(room.Name);
                return;
            }
        }

        // If no suitable room found, try joining any random room
        Debug.Log("No room with 1 player found, trying random room...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No random room available. Creating a new one...");

        string roomName = "Room_" + Random.Range(1000, 9999);
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name} ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})");
        // You can spawn your player prefab here if needed:
        // PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon: {cause}");
    }

    private void UpdateMenuVisibility()
    {
        int players = PhotonNetwork.CurrentRoom.PlayerCount;

        if (players == 2)
        {
            // Determine if THIS player is player #1 or player #2
            List<Player> sortedPlayers = new List<Player>(PhotonNetwork.PlayerList);
            sortedPlayers.Sort((a, b) => a.ActorNumber.CompareTo(b.ActorNumber)); // Sort by join order

            Player player1 = sortedPlayers[0];
            Player player2 = sortedPlayers[1];

            // If I am player1 → show menu
            if (PhotonNetwork.LocalPlayer == player1)
            {
                menuUI.SetActive(true);
            }
            else
            {
                menuUI.SetActive(false);
            }
        }
        else
        {
            // If alone → default to show menu
            menuUI.SetActive(true);
        }
    }
}
