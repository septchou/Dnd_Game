using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDisplay : MonoBehaviourPun
{
    public TMP_Text characterNameText;
    public TMP_Text hpText;
    public GameObject Outline;

    // Method to set character info (called locally)
    public void SetCharacterInfo(string name, int hp)
    {
        if (photonView.IsMine)
        {
            Outline.SetActive(true);
            // If this is the local player, update locally
            characterNameText.text = name;
            hpText.text = $"HP: {hp}";
            // Send this info to other players
            photonView.RPC("UpdateCharacterInfo", RpcTarget.OthersBuffered, name, hp);

        }
    }

    // RPC to update character info across the network (for other players)
    [PunRPC]
    public void UpdateCharacterInfo(string name, int hp)
    {
        characterNameText.text = name;
        hpText.text = $"HP: {hp}";
    }
}
