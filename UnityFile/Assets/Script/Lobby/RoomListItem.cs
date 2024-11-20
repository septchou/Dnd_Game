using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text playerNumber;

    public RoomInfo info;

    public void SetUp(RoomInfo _info)
    {
        info = _info;
        roomName.text = _info.Name;
       

        playerNumber.text = $"0{_info.PlayerCount} / 0{_info.MaxPlayers} "; 
    }

    public void OnClick()
    {
        Launcher.Instance.JoinRoom(info);
    }
}