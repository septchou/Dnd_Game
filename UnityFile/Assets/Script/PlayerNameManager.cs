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

    void Start()
    {
        if (string.IsNullOrEmpty(usernameInput.text))
        {
            SetRandomPlayerName();
        }
    }
    public void OnUsernameInputValueChanged()
    {
        // Otherwise, set the nickname to the entered username
        PhotonNetwork.NickName = usernameInput.text;
    }

    private void SetRandomPlayerName()
    {
       PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }
}
