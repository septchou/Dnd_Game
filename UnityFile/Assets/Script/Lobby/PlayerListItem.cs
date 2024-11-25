using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;  // Required for Hashtable

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text readyStatusText;
    [SerializeField] Button kickButton;
    [SerializeField] GameObject readyFlag;
    [SerializeField] GameObject notReadyFlag;
    [SerializeField] GameObject playerBG;
    [SerializeField] GameObject dmBG;
    Player player;

    // Set up the player's display information
    public void SetUp(Player _player)
    {
        player = _player;
        UpdateItem();
        if (!player.IsMasterClient)
        {
            UpdateReadyStatus();
        }

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

    private void UpdateItem()
    {
        if (player.IsMasterClient)
        {
            playerNameText.text = player.NickName + " (Host)";
            playerNameText.color = Color.white;
            readyStatusText.text = "";
            readyFlag.SetActive(false);
            notReadyFlag.SetActive(false);
            playerBG.SetActive(false);
            dmBG.SetActive(true);
        }
        else
        {
            playerBG.SetActive(true);
            dmBG.SetActive(false);
            playerNameText.text = player.NickName;
        }

        kickButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && !player.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Update the name of this player if they are the new Master Client
        UpdateItem();
    }

    // Method to update ready status based on player's custom properties
    public void UpdateReadyStatus()
    {
        if (player.CustomProperties.ContainsKey("isReady"))
        {
            bool isReady = (bool)player.CustomProperties["isReady"];
            if (isReady)
            {
                readyFlag.SetActive(true);
                notReadyFlag.SetActive(false);
            }
            else
            {
                readyFlag.SetActive(false);
                notReadyFlag.SetActive(true);
            }
            readyStatusText.text = isReady ? "Ready" : "Not Ready"; 
        }
        else
        {
            readyFlag.SetActive(false);
            notReadyFlag.SetActive(true);
            readyStatusText.text = "Not Ready"; 
        }
    }

    // Callback to update ready status when player properties change
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer && changedProps.ContainsKey("isReady"))
        {
            // Master Client is always ready, so update only non-host players
            if (!targetPlayer.IsMasterClient)
            {
                UpdateReadyStatus();  // Refresh the ready status if it has changed
            }
        }
    }

    public void OnKickButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Launcher.Instance.KickPlayer(player); // Call the kick method from the Launcher or relevant class
        }
    }
}
