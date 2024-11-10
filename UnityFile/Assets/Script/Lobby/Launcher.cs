using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Realtime;
using System.Linq;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject PlayerListItemPrefab;
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject readyButton;
    [SerializeField] GameObject selectButton;
    [SerializeField] TMP_Text readyButtonText;

    void Awake()
    {
        Instance = this;
    }

    // Connect to Photon server when the game starts
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();  // Connects to Photon master server
    }

    // Callback when connected to the Photon server
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        PhotonNetwork.JoinLobby();  // Joins the default lobby
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.EnableCloseConnection = true;
    }

    // Callback when joined the lobby
    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("Title");
        Debug.Log("Joined Lobby");
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = 4;
        roomOptions.CleanupCacheOnLeave = true;

        PhotonNetwork.CreateRoom(roomNameInputField.text,roomOptions,TypedLobby.Default);
        Debug.Log("Created room success");
        MenuManager.Instance.OpenMenu("loading");
    }


    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("room");
        Debug.Log("Join " + PhotonNetwork.CurrentRoom.Name);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }


        if (!PhotonNetwork.IsMasterClient)
        {
            readyButton.SetActive(true);
            startGameButton.SetActive(false);;
        }
        else {
            startGameButton.SetActive(true);
            readyButton.SetActive(false);
        }
        
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Host left Leave the room");
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        Debug.LogError("Room Creation Failed: " + message);
        MenuManager.Instance.OpenMenu("error");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;

            // Skip the room if it already has the max number of players (4 in this case)
            if (roomList[i].PlayerCount >= roomList[i].MaxPlayers)
                continue;

            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }

        //Debug.Log("Found " + (roomList.Count) + " Room" );
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }


    public void OnReadyButtonClicked()
    {
        bool isReady = false;

        // Check the current ready status and toggle it
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isReady"))
        {
            isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];
        }

        // Toggle ready state
        isReady = !isReady;
        readyButtonText.text = isReady ? "Ready" : "Not Ready";

        // Update the player's custom property for ready status
        Hashtable props = new Hashtable
        {
            { "isReady", isReady }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void StartGame()
    {
        if (AreAllPlayersReady())
        {
            PhotonNetwork.LoadLevel("GameSetUp");
        }
    }

    private bool AreAllPlayersReady()
    {
        bool allReady = true; // Assume all players are ready initially

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Skip the Master Client since they are always considered ready
            if (player.IsMasterClient)
            {
                continue;
            }

            if (player.CustomProperties.ContainsKey("isReady"))
            {
                bool isReady = (bool)player.CustomProperties["isReady"];
                if (!isReady)
                {
                    allReady = false; // Found a player not ready
                    Debug.Log(player.NickName + " is not ready."); // Log the name of the player not ready
                }
            }
            else
            {
                allReady = false; // Player doesn't have the property, so they are not ready
                Debug.Log(player.NickName + " is not ready."); // Log the name of the player not ready
            }
        }

        return allReady; // Return true only if all non-master players are ready
    }

    public void KickPlayer(Player playerToKick)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CloseConnection(playerToKick);
            Debug.Log(playerToKick.NickName + " has been kicked from the room.");
        }
    }

    public void SelectCharacter()
    {
        MenuManager.Instance.OpenMenu("CharacterSelection");
    }
}