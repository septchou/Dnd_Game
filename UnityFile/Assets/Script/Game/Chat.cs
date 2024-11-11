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
        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, GetSenderName() + " : " + inputField.text);
        inputField.text = "";
    }

    public void SendRollResult(List<Dice> dices, int result)
    {
        string content;
        content = GetSenderName() + " rolled a";
        for(int i = 0; i < dices.Count; i++)
        {
            content += " D" + dices[i].diceType.ToString() + " ";
        }
        content += "and got " + result.ToString();

        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content);

    }

    public void SendSkillReport(string skillName,int damage,int hit,bool isMiss)
    {
        string content = $"{GetSenderName()} Cast {skillName} with Hit : {hit} and Damage : {damage}";
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
