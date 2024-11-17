using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Burst.CompilerServices;
using System.Net.Http;
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
        GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, $"<b>{GetSenderName()}</b>" + " : " + inputField.text,false);
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

        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, true);
        }
        else
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, false);
        }

    }

    public void SendSkillReport(string characterName, string skillName,int damage,int hit,bool isCrit)
    {
        string content = $"<b>{characterName}</b> hit with the critical!!!";
        if (isCrit)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, true);
            }
            else
            {
                GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, false);
            }
        }

        content = $"<b>{characterName}</b> Cast <b>{skillName}</b> with Hit : <b>{hit}</b> and Damage : <b>{damage}</b>";
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, true);
        }
        else
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, false);
        }

    }

    public void SendSkillBuffReport(string characterName, string skillName)
    {

        string content = $"<b>{characterName}</b> Cast <b>{skillName}</b>";
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, true);
        }
        else
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, false);
        }
    }

    public void SendDodgeReport()
    {

        string content = $"<b>{GetSenderName()}</b> can doge the attack via Warrior’s sense";
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, true);
        }
        else
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, false);
        }
    }

    public void SendHealReport(string characterName, int damage,string targetName)
    {

        string content = $"<b>{characterName}</b> heal <b>{targetName}</b> by <b>{damage}</b> points";
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, true);
        }
        else
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, false);
        }
    }

    public void SendAbilityRollReport(string characterName, string abilityName, int result)
    {

        string content = $"<b>{characterName}</b> roll ability <b>{abilityName}</b> check and get <b>{result}</b>";
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, true);
        }
        else
        {
            GetComponent<PhotonView>().RPC("GetMessage", RpcTarget.All, content, false);
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
    public void GetMessage(String recieveMessage, bool isHideFromPlayer)
    {
        if (isHideFromPlayer && !PhotonNetwork.IsMasterClient) return ;
        GameObject newMessage = Instantiate(message, Vector3.zero , Quaternion.identity, content.transform);
        newMessage.GetComponent<Message>().myMessage.text = recieveMessage;
        newMessage.GetComponent<Message>().ResizeBackgroundToFitText();
    }
}
