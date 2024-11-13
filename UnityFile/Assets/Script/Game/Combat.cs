using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Skill;

public class Combat : MonoBehaviourPun
{ 
    [SerializeField] Chat chatLog;
    public string characterTurnName;
    [SerializeField] string selectedEnemyName;
    [SerializeField] Character Caster;
    [SerializeField] CharacterDisplay Target;
    [SerializeField] List<Skill> skills;
    [SerializeField] List<Button> skillButtons;
    [SerializeField] List<GameObject> skillButtonsGameobject;
    [SerializeField] float delay = 3f;
    public bool isMyturn = false;
    [SerializeField] bool isSelecting = false;
    [SerializeField] Skill selectedSkill;
    [SerializeField] int hitValue, damageValue;
    private bool getHitValue = false, getDamageValue = false;
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            skills = CharacterManager.Instance.selectedCharacter.skills;
            Caster = CharacterManager.Instance.selectedCharacter;
            for (int i = 0; i < skillButtons.Count; i++)
            {
                int index = i; // Capture the current index in a local variable
                skillButtons[i].onClick.AddListener(() => SelectTargetToCastSkill(index));
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(isSelecting && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if(hit.collider != null)
            {
                PhotonView photonView = hit.collider.GetComponent<PhotonView>();
                Target = hit.collider.GetComponent<CharacterDisplay>();
                StartCoroutine(CastSkill());
                isSelecting = false;
            }
        }

        if(isSelecting && Input.GetMouseButtonDown(1))
        {
            isSelecting = false;
            Debug.Log("Cancle Cast");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if(isMyturn && DmSelectCorrectEnemy(characterTurnName))
            {
                for (int i = 0; i < skillButtonsGameobject.Count; i++)
                {
                    skillButtonsGameobject[i].SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < skillButtonsGameobject.Count; i++)
                {
                    skillButtonsGameobject[i].SetActive(false);
                }
            }
            
        }
        else
        {
            for (int i = 0; i < skillButtonsGameobject.Count; i++)
            {
                skillButtonsGameobject[i].SetActive(isMyturn);
            }
        }

    }
    
    private bool DmSelectCorrectEnemy(string characterName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (selectedEnemyName == characterName) return true;
            else return false;
        }
        else return false;
    }
    private void SelectTargetToCastSkill(int i)
    {
        isSelecting = true;
        selectedSkill = skills[i];
    }

    public void DmChangeCaster(string characterName)
    {
        selectedEnemyName = characterName;

        List<Character> list = CharacterManager.Instance.GetEnemyList();

        // Extract the base name by removing the number at the end
        string baseName = characterName.Contains(" ")
            ? characterName.Substring(0, characterName.LastIndexOf(" "))
            : characterName; // Use the full name if there's no space

        Caster = list.Find(character => character.characterName == baseName);

        if (Caster != null)
        {
            // Set skill
            skills = Caster.skills;
            for (int i = 0; i < skillButtons.Count; i++)
            {
                int index = i; // Capture the current index in a local variable
                skillButtons[i].onClick.AddListener(() => SelectTargetToCastSkill(index));
            }
        }
        else
        {
            Debug.LogWarning($"Character with base name '{baseName}' not found in enemy list.");
        }
    }

    private IEnumerator CastSkill()
    {
        string skillName = selectedSkill.name;
        int FlatHit,FlatDmg;

        getHitValue = true;
        GameSceneController.Instance.RollAnimation(selectedSkill.GetHitDiceLists(), HandleDiceRollResult);
        getDamageValue = true;
        GameSceneController.Instance.RollAnimation(selectedSkill.GetDamageDiceLists(), HandleDiceRollResult);
        yield return new WaitForSeconds(delay);

        if (skillName == "Slash")
        {
            // Str / 2
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            hitValue += FlatHit / 2;
            // Dex / 2
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[1]);
            hitValue += FlatHit / 2;
            
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
        }

        bool isMiss = IsThisMiss(hitValue);
        chatLog.SendSkillReport(skillName, damageValue, hitValue, isMiss);
        if (!isMiss)
        {
            Target.ChangeHP(damageValue * -1);
        }
    }
    void HandleDiceRollResult(int result, List<Dice> dices)
    {
        if (getHitValue)
        {
            hitValue = result;
            getHitValue = false;
        }
        else if(getDamageValue)
        {
            damageValue = result;
            getDamageValue = false;
        }
    }

    private bool IsThisMiss(int hitVal)
    {

        return false;
    }


}
