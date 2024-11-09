using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.EventSystems;

public class CharacterDisplayPopUp : MonoBehaviour
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

    [System.Serializable]
    public class abilityScoreUI
    {
        public TMP_Text abilityScorePointText;
        public TMP_Text abilityScoreModifierText;
        public Button increaseButton, decreaseButton;
    }

    // References to the UI elements that will show character details
    [SerializeField] GameObject characterDetailPopup;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] TMP_Text classNameText;
    [SerializeField] TMP_Text raceNameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text hpText;
    [SerializeField] List<abilityScoreUI> abilityUI;
    [SerializeField] List<TMP_Text> skillsText;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse click
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("Hit");
                // Check if the hit object has a CharacterDisplay component
                CharacterDisplay characterDisplay = hit.collider.GetComponent<CharacterDisplay>();

                if (characterDisplay != null)
                {
                    // Get data from CharacterDisplay and pass it to ShowCharacterDetails
                    ShowCharacterDetails(
                        characterDisplay.characterName,
                        characterDisplay.className,
                        characterDisplay.raceName,
                        characterDisplay.level,
                        characterDisplay.currentHP,
                        characterDisplay.maxHP,
                        characterDisplay.SerializeAbilityScoreData(characterDisplay.abilityScoreData),
                        characterDisplay.SerializeSkillData(characterDisplay.skillData)
                    );
                }
            }
        }
    }

    // Show the character details in the UI
    public void ShowCharacterDetails(string characterName, string className , string raceName, int level, int currentHP, int maxHP, object[] abilityDataObj, object[] skillDataObj)
    {
        // Populate the UI with character information
        characterNameText.text = $"Name : {characterName}";
        classNameText.text = $"Class : {className}";
        raceNameText.text = $"Race : {raceName}";
        levelText.text = "Level: " + level.ToString();
        hpText.text = "HP: " + currentHP + " / " + maxHP;

        List<AbilityScorePointsData> abilityScoreData = DeserializeAbilityScoreData(abilityDataObj);
        // Display ability scores and modifiers
        for (int i = 0; i < abilityScoreData.Count; i++)
        {
            // Ensure that we don't go out of bounds of the abilityUI list
            if (i < abilityUI.Count)
            {
                abilityUI[i].abilityScorePointText.text = $"{abilityScoreData[i].abilityScorePoint}";
                abilityUI[i].abilityScoreModifierText.text = (abilityScoreData[i].ablityModifierBonus > 0) 
                    ? $"+{abilityScoreData[i].ablityModifierBonus}" 
                    : abilityScoreData[i].ablityModifierBonus.ToString();
            }
        }

        List<SkillData> skillData = DeserializeSkillData(skillDataObj);
        // Populate skill text components
        for (int i = 0; i < skillData.Count; i++)
        {
            if (i < skillsText.Count)
            {
                skillsText[i].text = $"{skillData[i].skillName}";
            }
        }

        // Activate the popup UI
        characterDetailPopup.SetActive(true);
    }

    // Hide the character detail popup (optional)
    public void CloseCharacterDetails()
    {
        characterDetailPopup.SetActive(false);
    }

    private List<AbilityScorePointsData> DeserializeAbilityScoreData(object[] data)
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

    private List<SkillData> DeserializeSkillData(object[] data)
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
}
