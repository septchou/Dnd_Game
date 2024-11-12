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
        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, $"<b>{GetSenderName()}</b>" + " : " + inputField.text);
        inputField.text = "";
    }

    public void SendRollResult(List<Dice> dices, int result)
    {
        string content;
        content = $"<b>{GetSenderName()}</b>" + " rolled a";
        for(int i = 0; i < dices.Count; i++)
        {
            content += "<b> D" + dices[i].diceType.ToString() + " </b>";
        }
        content += "and got " + $"<b>{result}</b>";

        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content);

    }

    public void SendSkillReport(string skillName,int damage,int hit,bool isMiss)
    {
        string content = $"<b>{GetSenderName()}</b> Cast <b>{skillName}</b> with Hit : <b>{hit}</b> and Damage : <b>{damage}</b>";
        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content);
        if (isMiss)
        {
            content = GetSenderName() + " miss";
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content);
        }
    }
        
    private string GetSenderName()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            return "DM";
        }
        else
        {
            return CharacterManager.Instance.selectedCharacter.characterName;
        }
    }
    [PunRPC]
    public void GetMessage(String recieveMessage)
    {
        GameObject newMessage = Instantiate(message, Vector3.zero , Quaternion.identity, content.transform);
        newMessage.GetComponent<Message>().myMessage.text = recieveMessage;
        newMessage.GetComponent<Message>().ResizeBackgroundToFitText();
    }
}
