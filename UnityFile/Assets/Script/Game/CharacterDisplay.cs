using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class CharacterDisplay : MonoBehaviourPun
{
    [System.Serializable]
    public class AbilityScorePointsData
    {
        public string abilityScoreName;
        public int abilityScorePoint;
        public int ablityModifierBonus;
    }

    [System.Serializable]
    public class SkillData
    {
        public string skillName;
        public string skillDetail;
    }

    [Header("Data")]
    public int currentHP, maxHP;
    public string characterName, className, raceName;
    public int level;
    public List<AbilityScorePointsData> abilityScoreData;
    public List<SkillData> skillData;
    public bool isMyTurn;
    public bool isDodge = false;

    [Header("GameObject")]
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] TMP_Text hpText;
    [SerializeField] SpriteRenderer characterImage;
    [SerializeField] SpriteRenderer Outline;
    public Character characterData;
    [SerializeField] Chat chatlog;
    [SerializeField] Slider slider;
    // Method to initialize character data
    public void SetCharacterData(Character character)
    {
        characterData = character;

        // Set character basic data
        characterName = character.characterName;
        className = character.characterClass.className;
        raceName = character.race.raceName;
        level = character.level;
        maxHP = character.HP;
        currentHP = maxHP;

        // Set ability score data
        abilityScoreData.Clear();
        foreach (var ability in character.abilityScorepoints)
        {
            abilityScoreData.Add(new AbilityScorePointsData
            {
                abilityScoreName = ability.abilityScore.name,
                abilityScorePoint = ability.abilityScorePoint,
                ablityModifierBonus = ability.ablityModifierBonus
            });
        }

        // Set skill data
        skillData.Clear();
        foreach (var skill in character.skills)
        {
            skillData.Add(new SkillData
            {
                skillName = skill.skillName,
                skillDetail = skill.detail
            });
        }

        // Initialize character UI
        characterNameText.text = character.characterName;
        hpText.text = $"HP: {currentHP} / {maxHP}";
        UpdatehealthBar();

        string hexColor = "#000000";
        if (PhotonNetwork.IsMasterClient)
        {
            hexColor = "#76161C";
            gameObject.tag = "Villain";

        }
        else
        {
            hexColor = "#298041";
            gameObject.tag = "Hero";
        }

        if (UnityEngine.ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            Outline.color = color; // Assign the color to the sprite
        }
        else
        {
            Debug.LogError("Invalid Hexadecimal Color Code");
        }

        CharacterIconSwitcher.Instance.SetCharacterIcon(className, raceName,null, characterImage, true);

        // If this is the local player, activate the outline and sync with other players
        if (photonView.IsMine)
        {
            // Call RPC to update data for other players
            photonView.RPC("UpdateCharacterInfo", RpcTarget.OthersBuffered, characterName, maxHP, className, raceName, level);
            photonView.RPC("UpdateAbilityScores", RpcTarget.OthersBuffered, SerializeAbilityScoreData(abilityScoreData));
            photonView.RPC("UpdateSkills", RpcTarget.OthersBuffered, SerializeSkillData(skillData));
            photonView.RPC("SetFaction", RpcTarget.OthersBuffered);
            photonView.RPC("SetCharacterImage", RpcTarget.OthersBuffered);
        }


    }

    private void UpdatehealthBar()
    {
        slider.value = (float)currentHP / (float)maxHP;
    }

    // Method to change HP locally and synchronize with others
    public void ChangeHP(int hpChange)
    {
        if (isDodge)
        {
            chatlog.SendDodgeReport();
            photonView.RPC("RPC_useWariorSense", RpcTarget.AllBuffered);
            isDodge = false;
            return;
        }
        currentHP = Mathf.Clamp(currentHP + hpChange, 0, maxHP);
        hpText.text = $"HP: {currentHP} / {maxHP}";
        UpdatehealthBar();
        photonView.RPC("SyncHP", RpcTarget.OthersBuffered, currentHP);
    }

    // Serialize AbilityScorePointsData to a format that Photon can handle (such as a string array or a simple array)
    public object[] SerializeAbilityScoreData(List<AbilityScorePointsData> abilityScores)
    {
        List<object> serializedData = new List<object>();

        foreach (var score in abilityScores)
        {
            serializedData.Add(score.abilityScoreName);
            serializedData.Add(score.abilityScorePoint);
            serializedData.Add(score.ablityModifierBonus);
        }

        return serializedData.ToArray();
    }

    // Deserialize AbilityScorePointsData back into the custom class
    public List<AbilityScorePointsData> DeserializeAbilityScoreData(object[] data)
    {
        List<AbilityScorePointsData> abilityScores = new List<AbilityScorePointsData>();

        for (int i = 0; i < data.Length; i += 3)
        {
            abilityScores.Add(new AbilityScorePointsData
            {
                abilityScoreName = (string)data[i],
                abilityScorePoint = (int)data[i + 1],
                ablityModifierBonus = (int)data[i + 2]
            });
        }

        return abilityScores;
    }

    // Serialize SkillData similarly
    public object[] SerializeSkillData(List<SkillData> skills)
    {
        List<object> serializedData = new List<object>();

        foreach (var skill in skills)
        {
            serializedData.Add(skill.skillName);
            serializedData.Add(skill.skillDetail);
        }

        return serializedData.ToArray();
    }

    public List<SkillData> DeserializeSkillData(object[] data)
    {
        List<SkillData> skills = new List<SkillData>();

        for (int i = 0; i < data.Length; i += 2)
        {
            skills.Add(new SkillData
            {
                skillName = (string)data[i],
                skillDetail = (string)data[i + 1]
            });
        }

        return skills;
    }

    public void DestroyCharacter()
    {
        // Check if this object belongs to the local player before destroying
        if (photonView.IsMine)
        {
            // Call an RPC to destroy the object on all clients
            photonView.RPC("RPC_DestroyCharacter", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void SetFaction()
    {
        string hexColor = "#000000";
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            hexColor = "#298041";
            gameObject.tag = "Hero";
        }

        else if(!photonView.IsMine && photonView.Owner != PhotonNetwork.MasterClient)
        {
            hexColor = "#4F3F21";
            gameObject.tag = "Hero";
        }

        else if(photonView.Owner == PhotonNetwork.MasterClient)
        {
            hexColor = "#76161C";
            gameObject.tag = "Villain";
        }

        if (UnityEngine.ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            Outline.color = color; // Assign the color to the sprite
        }
        else
        {
            Debug.LogError("Invalid Hexadecimal Color Code");
        }
    }

    // RPC to update character info across the network (for other players)
    [PunRPC]
    public void UpdateCharacterInfo(string Name, int hp, string classname, string race, int lv)
    {
        characterNameText.text = Name;
        hpText.text = $"HP: {hp} / {hp}";

        characterName = Name;
        className = classname;
        raceName = race;
        level = lv;
        maxHP = hp;
        currentHP = hp;
    }

    // RPC to update ability score data across the network (for other players)
    [PunRPC]
    public void UpdateAbilityScores(object[] abilityScores)
    {
        abilityScoreData = DeserializeAbilityScoreData(abilityScores);
    }

    // Update the RPC for skills
    [PunRPC]
    public void UpdateSkills(object[] skills)
    {
        skillData = DeserializeSkillData(skills);
    }

    // RPC to synchronize HP with all players
    [PunRPC]
    public void SyncHP(int newHP)
    {
        currentHP = newHP;
        hpText.text = $"HP: {currentHP} / {maxHP}";

        if (characterData != null)
        {
            characterData.HP = currentHP;
        }
        UpdatehealthBar();
    }

    [PunRPC]
    private void RPC_DestroyCharacter()
    {
        // Destroy the game object this script is attached to
        Destroy(gameObject);
    }

    public void GetWariorSense()
    {
        isDodge = true;
        photonView.RPC("RPC_getWariorSense", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_getWariorSense()
    {
        isDodge = true;
    }

    [PunRPC]
    private void RPC_useWariorSense()
    {
        isDodge = false;
    }

    [PunRPC]
    private void SetCharacterImage()
    {
        CharacterIconSwitcher.Instance.SetCharacterIcon(className, raceName, null, characterImage, true);
    }

    public void UpdateAbilityFromBuff()
    {
        photonView.RPC("UpdateAbilityScores", RpcTarget.OthersBuffered, SerializeAbilityScoreData(abilityScoreData));
    }
    public int GetCharacterModifier(string abilityName)
    {
        int index = abilityScoreData.FindIndex(a => a.abilityScoreName == abilityName);
        
        return (int)Math.Floor((abilityScoreData[index].abilityScorePoint - 10) / 2.0) + abilityScoreData[index].ablityModifierBonus; ;
    }
}
