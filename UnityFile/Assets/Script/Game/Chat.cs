using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using System;
public class Chat : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject message, content, chatBoxUI;
    [SerializeField] bool isChatBoxOpen = false;
    public void OpenChatBox()
    {
        chatBoxUI.SetActive(!isChatBoxOpen);
        isChatBoxOpen = !isChatBoxOpen;
    }
    public void SendMessage()
    {
        string senderName;
        if (PhotonNetwork.IsMasterClient)
        {
            senderName = "DM";
        }
        else 
        {
            senderName = CharacterManager.Instance.selectedCharacter.characterName;
        }
        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All,senderName + " : " + inputField.text);
        inputField.text = "";
    }

    public void SendRollResult(List<Dice> dices, int result)
    {
        string senderName, content;
        if (PhotonNetwork.IsMasterClient)
        {
            senderName = "DM";
        }
        else
        {
            senderName = CharacterManager.Instance.selectedCharacter.characterName;
        }

        content = senderName + " rolled a";
        for(int i = 0; i < dices.Count; i++)
        {
            content += " D" + dices[i].diceType.ToString() + " ";
        }
        content += "and got " + result.ToString();

        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content);

    }

    [PunRPC]
    public void GetMessage(String recieveMessage)
    {
        GameObject newMessage = Instantiate(message, Vector3.zero , Quaternion.identity, content.transform);
        newMessage.GetComponent<Message>().myMessage.text = recieveMessage;
        newMessage.GetComponent<Message>().ResizeBackgroundToFitText();
    }
}
