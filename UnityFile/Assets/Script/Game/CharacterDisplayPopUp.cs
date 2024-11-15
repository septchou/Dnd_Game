using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
using static GameSceneController;
using System;


public class CharacterDisplayPopUp : MonoBehaviourPun
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
        public GameObject abilityRollCheckButton;
    }

    [Header("Character Detail Panel")]
    [SerializeField] GameObject characterDetailPopup;
    [SerializeField] GameObject skillDetailPopup;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] TMP_Text classNameText;
    [SerializeField] TMP_Text raceNameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text hpText;
    [SerializeField] List<abilityScoreUI> abilityUI;
    [SerializeField] List<TMP_Text> skillsText;

    [Header("Skill Detail Panel")]
    [SerializeField] TMP_Text skillnameText;
    [SerializeField] TMP_Text skilldetailText;
    [SerializeField] List<Button> getSkillDetailButton;
    [SerializeField] List<string> skillName;
    [SerializeField] List<string> skillDetail;
    [SerializeField] List<TMP_Text> hitDiceDetail;
    [SerializeField] List<TMP_Text> dmgDiceDetail;

    [Header("Race Skill panel")]
    [SerializeField] Transform raceSkillcontent;
    [SerializeField] RectTransform raceSkillPanel;
    [SerializeField] GameObject raceSkilllistPrefab;
    [SerializeField] List <RaceSkillItem> raceSkillList;

    [Header("GameplayUI")]
    [SerializeField] CharacterDisplay clickedCharacter;
    [SerializeField] GameObject changeHPGameobject;
    [SerializeField] GameObject hpChangeInputGameobject;
    [SerializeField] GameObject removeCharacterButtonObject;
    [SerializeField] Button changeHPButton;
    [SerializeField] TMP_InputField hpChangeInput;
    [SerializeField] List<Button> abilityRollButton;
    [SerializeField] List<Dice> abilityRollDice;

    [Header("Combat")]
    [SerializeField] Combat combat;
    [SerializeField] Chat chatLog;

    [SerializeField] List<AbilityScorePointsData> abilityScoreDatas;
    [SerializeField] string characterClickedName;
    void Start()
    {
        // Add listeners to each skill detail button
        for (int i = 0; i < getSkillDetailButton.Count; i++)
        {
            int index = i;  // Capture index for use in the listener
            getSkillDetailButton[i].onClick.AddListener(() => ShowSkillDetail(index));
        }

        for(int i = 0; i < abilityRollButton.Count; i++)
        {
            int index = i;
            abilityRollButton[i].onClick.AddListener(() => AbilityRollCheck(index));
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse click
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                PhotonView photonView = hit.collider.GetComponent<PhotonView>();
                CharacterDisplay characterDisplay = hit.collider.GetComponent<CharacterDisplay>();
                clickedCharacter = characterDisplay;

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

                    // Check ownership for UI options
                    if (photonView != null && (photonView.IsMine || PhotonNetwork.IsMasterClient))
                    {
                        ShowOwnerOption(true);

                        if (photonView.IsMine && PhotonNetwork.IsMasterClient)
                        {
                            removeCharacterButtonObject.SetActive(true);
                        }
                    }
                        
                    else
                        ShowOwnerOption(false);

                    if (PhotonNetwork.IsMasterClient && photonView.IsMine)
                    {
                        combat.DmChangeCaster(characterDisplay.characterName);
                    }
                }
                else
                {
                    Debug.LogError("CharacterDisplay component not found on clicked object.");
                }
            }
        }
    }


    // Show the character details in the UI
    public void ShowCharacterDetails(string characterName, string className , string raceName, int level, int currentHP, int maxHP, object[] abilityDataObj, object[] skillDataObj)
    {
        characterClickedName = characterName;
        // Populate the UI with character information
        characterNameText.text = $"Name : {characterName}";
        classNameText.text = $"Class : {className}";
        raceNameText.text = $"Race : {raceName}";
        levelText.text = "Level: " + level.ToString();
        hpText.text = "HP: " + currentHP + " / " + maxHP;

        abilityScoreDatas = DeserializeAbilityScoreData(abilityDataObj);
        // Display ability scores and modifiers
        for (int i = 0; i < abilityScoreDatas.Count; i++)
        {
            // Ensure that we don't go out of bounds of the abilityUI list
            if (i < abilityUI.Count)
            {
                abilityUI[i].abilityScorePointText.text = $"{abilityScoreDatas[i].abilityScorePoint}";
                int modifier = (int)Math.Floor((abilityScoreDatas[i].abilityScorePoint - 10) / 2.0) + abilityScoreDatas[i].ablityModifierBonus;
                abilityUI[i].abilityScoreModifierText.text = (modifier > 0) 
                    ? $"+{modifier}" 
                    : modifier.ToString();
            }
        }

        List<SkillData> skillData = DeserializeSkillData(skillDataObj);
        // Populate skill text components
        for (int i = 0; i < skillData.Count; i++)
        {
            Skill skill = Resources.Load<Skill>("Skills/" + skillData[i].skillName);
            if (i < skillsText.Count)
            {
                hitDiceDetail[i].text = skill.GetHitDiceData();
                dmgDiceDetail[i].text = skill.GetDmgDiceData();

                skillsText[i].text = $"{skillData[i].skillName}";
                skillName.Add(skillData[i].skillName);
                skillDetail.Add(skillData[i].skillDetail);
            }
        }

        foreach(var racelist in raceSkillList)
        {
            racelist.DestroyListItem();
        }
        Race race = Resources.Load<Race>("Races/" + raceName);
        
        raceSkillList = new List<RaceSkillItem>();
        for( int i = 0; i < race.raceSkills.Count; i++)
        {
            Skill raceskill = race.raceSkills[i];
            raceSkillList.Add(Instantiate(raceSkilllistPrefab, raceSkillcontent).GetComponent<RaceSkillItem>());
            raceSkillList[i].SetUp(raceskill.skillName, raceskill.detail);
        }

        // Adjust raceSkillPanel
        RectTransform prefabRect = raceSkilllistPrefab.GetComponent<RectTransform>();
        float prefabHeight = prefabRect.rect.height;
        float totalHeight = prefabHeight * raceSkillList.Count;
        RectTransform panelRect = raceSkillPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, totalHeight);

        // Activate the popup UI
        characterDetailPopup.SetActive(true);
        
    }

    // Show skill details when button is pressed
    public void ShowSkillDetail(int index)
    {

        if (index >= 0 && index < skillName.Count)
        {
            skillnameText.text = skillName[index];
            skilldetailText.text = skillDetail[index];

            skillDetailPopup.SetActive(true);
        }
    }

    public void ShowOwnerOption(bool isShow)
    {
        for (int i = 0; i < abilityUI.Count; i++)
        {
            abilityUI[i].abilityRollCheckButton.SetActive(isShow);
        }
        changeHPGameobject.SetActive(isShow);
        hpChangeInputGameobject.SetActive(isShow);


    }

    // Hide the character detail popup (optional)
    public void CloseCharacterDetails()
    {
        characterDetailPopup.SetActive(false);
    }

    public void CloseSkillDetails()
    {
        skillDetailPopup.SetActive(false);
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

    public void OnChangeHPButtonClick()
    {
        // Get the value from the input field and try to parse it to an integer
        if (int.TryParse(hpChangeInput.text, out int hpChange))
        {
            // Call the ChangeHP method to modify the HP
            clickedCharacter.ChangeHP(hpChange);
        }
        else
        {
            Debug.LogError("Invalid HP value entered.");
        }

        ShowCharacterDetails(
                        clickedCharacter.characterName,
                        clickedCharacter.className,
                        clickedCharacter.raceName,
                        clickedCharacter.level,
                        clickedCharacter.currentHP,
                        clickedCharacter.maxHP,
                        clickedCharacter.SerializeAbilityScoreData(clickedCharacter.abilityScoreData),
                        clickedCharacter.SerializeSkillData(clickedCharacter.skillData)
                    );
    }

    public void OnDestroyCharacterButtonClick()
    {
        clickedCharacter.DestroyCharacter();
    }
    
    public void UpdateCharacterDisplay()
    {
        ShowCharacterDetails(
                        clickedCharacter.characterName,
                        clickedCharacter.className,
                        clickedCharacter.raceName,
                        clickedCharacter.level,
                        clickedCharacter.currentHP,
                        clickedCharacter.maxHP,
                        clickedCharacter.SerializeAbilityScoreData(clickedCharacter.abilityScoreData),
                        clickedCharacter.SerializeSkillData(clickedCharacter.skillData)
                    );
    }

    public void AbilityRollCheck(int index)
    {

        int modifier = (abilityScoreDatas[index].abilityScorePoint - 10) / 2 + abilityScoreDatas[index].ablityModifierBonus;
        RollResultCallback callback = (result, rolledDices) =>
        {
            int finalResult = result + modifier;
            chatLog.SendAbilityRollReport(characterClickedName, abilityScoreDatas[index].abilityScoreName, result);
        };

        GameSceneController.Instance.RollAnimation(abilityRollDice, callback);

    }
}
