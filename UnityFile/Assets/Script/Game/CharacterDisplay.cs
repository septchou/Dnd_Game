using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDisplay : MonoBehaviourPun
{
    public TMP_Text characterNameText;
    public TMP_Text hpText;
    public GameObject Outline;

    public Character characterData;

    // Method to initialize character data
    public void SetCharacterData(Character character)
    {
        characterData = character;

        // Initialize character UI
        characterNameText.text = character.characterName;
        hpText.text = $"HP: {character.HP}";

        // If this is the local player, activate the outline and sync with other players
        if (photonView.IsMine)
        {
            Outline.SetActive(true);
            photonView.RPC("UpdateCharacterInfo", RpcTarget.OthersBuffered, character.characterName, character.HP);
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
