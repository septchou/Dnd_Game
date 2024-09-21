using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;

public class PlayerNameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInput;
    public void Update()
    {
        // Assign a default nickname if the input is empty or whitespace
        if (string.IsNullOrWhiteSpace(usernameInput.text))
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
        }
    }
    public void OnUsernameInputValueChanged()
    {
        // Otherwise, set the nickname to the entered username
        PhotonNetwork.NickName = usernameInput.text;
    }
}
