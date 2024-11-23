using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Character;
using static Skill;

[System.Serializable]
public class Buff
{
    public string buffName;
    public int duration;

    public Buff(string name, int duration)
    {
        this.buffName = name;
        this.duration = duration;
    }

}
[System.Serializable]
public class EnemyBuff
{
    public string enemyName;
    public List<Buff> buff;
    public EnemyBuff(string name, List<Buff> buffs)
    {
        this.enemyName = name;
        this.buff = buffs;
    }
}
[System.Serializable]
public class abilityBuff
{
    public AbilityScore abilityscore;
    public int point;
    public abilityBuff(AbilityScore ability, int mod)
    {
        this.abilityscore = ability;
        this.point = mod;
    }
}

public class Combat : MonoBehaviourPun
{
    [SerializeField] Chat chatLog;
    [SerializeField] OverloyOverMouse overlay;
    [SerializeField] GameObject selectTargetOverlayUI;
    [SerializeField] CharacterDisplayPopUp characterDisplayPopUp;

    [Header("Character")]
    public string characterTurnName;
    [SerializeField] string selectedEnemyName;
    [SerializeField] Character Caster;
    [SerializeField] CharacterDisplay CasterDisplay;
    [SerializeField] CharacterDisplay Target;

    [Header("List")]
    [SerializeField] List<Skill> skills;
    [SerializeField] List<Button> skillButtons;
    [SerializeField] List<GameObject> skillButtonsGameobject;

    [Header("Stat")]
    [SerializeField] float delay = 3f;
    public bool isMyturn = false;
    [SerializeField] int normalAction = 1;
    [SerializeField] bool isCastMultiple = false;
    [SerializeField] int multipleTarget = 0;
    [SerializeField] List<CharacterDisplay> multipleTargetlist;
    [SerializeField] bool isSelecting = false;
    [SerializeField] Skill selectedSkill;
    [SerializeField] int hitValue, damageValue;
    private bool getHitValue = false, getDamageValue = false;

    [Header("Buff")]
    [SerializeField] List<Buff> activeBuffs;
    [SerializeField] List<EnemyBuff> enemyListBuffs;
    [SerializeField] int hitDiceBuff = 0, damageDiceBuff = 0;
    [SerializeField] List<Dice> addDamageDices;
    [SerializeField] int modifierBeforeRage;

    [Header("UI")]
    [SerializeField] GameObject skillPointText;
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            skills = CharacterManager.Instance.selectedCharacter.skills;
            Caster = CharacterManager.Instance.selectedCharacter;
            for (int i = 0; i < skillButtons.Count; i++)
            {
                int index = i; // Capture the current index in a local variable
                if (skills[i].skillType == SkillType.Damage)
                {
                    skillButtons[i].onClick.AddListener(() => SelectTargetToCastSkill(index));
                }
                else if (skills[i].skillType == SkillType.Buff)
                {
                    skillButtons[i].onClick.AddListener(() => CastBuff(index));
                }
            }
        }
        else
        {

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isSelecting && Input.GetMouseButtonDown(0))
        {

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                PhotonView photonView = hit.collider.GetComponent<PhotonView>();
                Target = hit.collider.GetComponent<CharacterDisplay>();
                if (photonView != null && Target != null)
                {
                    if ((isCastMultiple && multipleTargetlist.Contains(Target)) || (!isCastMultiple))
                    {
                        if(isCastMultiple) multipleTargetlist.Add(Target);

                        isSelecting = false;
                        overlay.RemoveOverlay();
                        StartCoroutine(CastSkill());
                    }

                }

            }
        }

        if (isSelecting && Input.GetMouseButtonDown(1))
        {
            overlay.RemoveOverlay();
            isSelecting = false;
            Debug.Log("Cancle Cast");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (isMyturn && DmSelectCorrectEnemy(characterTurnName))
            {
                for (int i = 0; i < skillButtonsGameobject.Count; i++)
                {
                    skillButtonsGameobject[i].SetActive(true);
                }
                skillPointText.GetComponent<TextMeshProUGUI>().text = normalAction.ToString();
                skillPointText.SetActive(true);
            }
            else
            {
                for (int i = 0; i < skillButtonsGameobject.Count; i++)
                {
                    skillButtonsGameobject[i].SetActive(false);
                }
                skillPointText.SetActive(false);
            }

        }
        else
        {
            for (int i = 0; i < skillButtonsGameobject.Count; i++)
            {
                skillButtonsGameobject[i].SetActive(isMyturn);
            }
            skillPointText.SetActive(isMyturn);
            TextMeshProUGUI tmp = skillPointText.GetComponent<TextMeshProUGUI>();
            tmp.text = normalAction.ToString();
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
        overlay.CreatedMouseOverlay(selectTargetOverlayUI);
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
                skillButtons[i].onClick.RemoveAllListeners();
            }

            for (int i = 0; i < skillButtons.Count; i++)
            {
                int index = i;
                if (skills[i].skillType == SkillType.Damage)
                {
                    skillButtons[i].onClick.AddListener(() => SelectTargetToCastSkill(index));
                }
                else if (skills[i].skillType == SkillType.Buff)
                {
                    skillButtons[i].onClick.AddListener(() => CastBuff(index));
                }
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
        int FlatHit, FlatDmg, totalDiceRoll = 0; ;
        int levelScaledDice = (Caster.level / 5);
        bool dmgAgainst = false;
        hitValue = 0;
        damageValue = 0;

        if(normalAction <= 0)
        {
            Debug.Log("Not enough normal action point");
        }
        else
        {
            normalAction--;

            if (PhotonNetwork.IsMasterClient)
            {
                int enemyBuffIndex = enemyListBuffs.FindIndex(a => a.enemyName == CasterDisplay.characterName);
                List<Buff> enemyBuffs = enemyListBuffs[enemyBuffIndex].buff;
                if(enemyBuffs.Any(buff => buff.buffName == "Rage"))
                {
                    hitDiceBuff++;
                }
            }

            if (skillName != "Heal")
            {
                // Roll Hit
                getHitValue = true;
                GameSceneController.Instance.RollAnimation(selectedSkill.GetHitDiceLists(hitDiceBuff), HandleDiceRollResult);
                totalDiceRoll += selectedSkill.GetHitDiceLists(hitDiceBuff).Count;
            }

            // Roll Dmg
            List<Dice> totalDamageDices = new List<Dice>();
            totalDamageDices.AddRange(selectedSkill.GetDamageDiceLists(levelScaledDice + damageDiceBuff));
            totalDamageDices.AddRange(addDamageDices);

            getDamageValue = true;
            GameSceneController.Instance.RollAnimation(totalDamageDices, HandleDiceRollResult);
            totalDiceRoll += totalDamageDices.Count;

            yield return new WaitForSeconds(delay *(float)totalDiceRoll);

            if (skillName == "Slash")
            {
                // Hit Dice: 1d20 + STR Modifier		
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 1d10 + STR Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;
            }
            else if (skillName == "Circle slash")
            {
                // Hit Dice: 1d20 + STR Modifier		
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 1d4 + STR Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;
            }
            else if (skillName == "Single shot")
            {
                // Hit Dice: 1d20 + DEX Modifier		
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 1d6 + DEX Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;
            }
            else if (skillName == "Multi shots")
            {
                if (!isCastMultiple)
                {
                    normalAction++;
                    isCastMultiple = true;
                    multipleTarget = 3;
                    multipleTargetlist.Add(Target);
                }

                // Hit Dice: 1d20 + DEX Modifier		
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 1d4 + DEX Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;
            }
            else if (skillName == "Fireball")
            {
                // Hit Dice: 1d20 + INT Modifier		
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 1d8 + INT Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;

                if (Target.raceName == "Slime" || Target.raceName == "Half beast") dmgAgainst = true;
            }
            else if (skillName == "Lightning Stike")
            {
                // Hit Dice: 1d20 + INT Modifier		
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 1d8 + INT Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;

                if (Target.raceName == "Elf" || Target.raceName == "Human") dmgAgainst = true;
            }
            else if (skillName == "Ice shot")
            {
                // Hit Dice: 1d20 + INT Modifier		
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 1d6 + INT Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;

                if (Target.raceName == "Orc") dmgAgainst = true;
            }
            else if (skillName == "Heal")
            {

                // Damage: 1d6 + INT Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;
                damageValue *= Caster.level;
            }
            else if (skillName == "Bonk")
            {
                // Hit Dice: 1d20 + INT Modifier	
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 1d10 + INT Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;
            }
            else if (skillName == "Holy Shot")
            {
                // Hit Dice: 1d20 + INT Modifier 		
                FlatHit = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                hitValue += FlatHit;

                // Damage: 3d4 + INT Modifier
                FlatDmg = CasterDisplay.GetCharacterModifier(selectedSkill.associatedAbility[0].abilityName);
                damageValue += FlatDmg;
            }


            if (skillName != "Heal")
            {
                bool isMiss = IsThisMiss(hitValue);
                chatLog.SendSkillReport(Caster.characterName,skillName, damageValue, hitValue, dmgAgainst);
                if (!isMiss)
                {
                    if (dmgAgainst) damageValue += damageValue / 2;
                    if (skillName != "Circle slash")
                    {
                        Target.ChangeHP(damageValue * -1);
                    }
                    else
                    {
                        if (!PhotonNetwork.IsMasterClient)
                        {
                            GameObject[] villainObjects = GameObject.FindGameObjectsWithTag("Villain");
                            foreach (GameObject villainObject in villainObjects)
                            {
                                CharacterDisplay enemy = villainObject.GetComponent<CharacterDisplay>();
                                enemy.ChangeHP(damageValue * -1);
                            }
                        }
                        else
                        {
                            GameObject[] heroObjects = GameObject.FindGameObjectsWithTag("Hero");
                            foreach (GameObject herobject in heroObjects)
                            {
                                CharacterDisplay hero = herobject.GetComponent<CharacterDisplay>();
                                hero.ChangeHP(damageValue * -1);
                            }
                        }
                    }
                    
                }
            }
            else
            {
                chatLog.SendHealReport(Caster.characterName, damageValue, Target.characterName);
                Target.ChangeHP(damageValue);
            }

            if (isCastMultiple && multipleTarget > 0)
            {
                normalAction++;
                multipleTarget--;
                int index = skills.FindIndex(a => a == selectedSkill);
                SelectTargetToCastSkill(index);
            }
            else if (isCastMultiple && multipleTarget <= 0)
            {
                isCastMultiple = false;
                multipleTarget = 0;
            }
        }
    }

    private void CastBuff(int index)
    {
        int enemyBuffIndex = -1;
        if (PhotonNetwork.IsMasterClient)
        {
            enemyBuffIndex = enemyListBuffs.FindIndex(a => a.enemyName == characterTurnName);
            activeBuffs = enemyListBuffs[enemyBuffIndex].buff;
        }

        selectedSkill = skills[index];
        string skillName = selectedSkill.name;

        if (normalAction > 0) 
        {
            normalAction--;

            if (skillName == "Rage")
            {

                Buff newBuff = new Buff(skillName, 2);
                activeBuffs.Add(newBuff);

                // Str *=2
                int i = CasterDisplay.abilityScoreData.FindIndex(a => a.abilityScoreName == selectedSkill.associatedAbility[0].name);
                modifierBeforeRage = CasterDisplay.abilityScoreData[i].ablityModifierBonus;
                CasterDisplay.abilityScoreData[i].ablityModifierBonus *= 2;

                if (!PhotonNetwork.IsMasterClient)
                {
                    // more hit dice
                    hitDiceBuff++;
                }
                
            }
            else if (skillName == "Warrior’s sense")
            {
                CasterDisplay.GetWariorSense();
            }
            else if (skillName == "Focus")
            {
                addDamageDices.Add(selectedSkill.damageDices[0].dice);
            }
            else if (skillName == "Ancestor Guidance")
            {

                Buff newBuff = new Buff(skillName, 2);
                activeBuffs.Add(newBuff);

                // Dex points +2
                int i = CasterDisplay.abilityScoreData.FindIndex(a => a.abilityScoreName == selectedSkill.associatedAbility[0].name);
                CasterDisplay.abilityScoreData[i].abilityScorePoint += 2;

            }
            else if (skillName == "Amplify Magic")
            {
                Buff newBuff = new Buff(skillName, 2);
                activeBuffs.Add(newBuff);

                // INT points +2
                int i = CasterDisplay.abilityScoreData.FindIndex(a => a.abilityScoreName == selectedSkill.associatedAbility[0].name);
                CasterDisplay.abilityScoreData[i].abilityScorePoint += 2;
            }
            else if (skillName == "Blessing")
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    foreach (EnemyBuff enemyBuff in enemyListBuffs)
                    {
                        Buff newBuff = new Buff(skillName, 2);
                        enemyBuff.buff.Add(newBuff);
                    }

                    GameObject[] villainObjects = GameObject.FindGameObjectsWithTag("Villain");
                    foreach (GameObject villainObject in villainObjects)
                    {
                        CharacterDisplay enemy = villainObject.GetComponent<CharacterDisplay>();
                        foreach (AbilityScore ability in selectedSkill.associatedAbility)
                        {
                            int i = enemy.abilityScoreData.FindIndex(a => a.abilityScoreName == ability.name);
                            enemy.abilityScoreData[i].abilityScorePoint++;
                        }
                    }
                }
                else
                {
                    GetComponent<PhotonView>().RPC("RPC_PlayerApplyBlessing", RpcTarget.All);
                }

            }

            CasterDisplay.UpdateAbilityFromBuff();
            chatLog.SendSkillBuffReport(CasterDisplay.characterName, skillName);
            characterDisplayPopUp.UpdateCharacterDisplay();
        }
        else
        {
            Debug.Log("Not enough normal action point");
        }

        if (PhotonNetwork.IsMasterClient && enemyBuffIndex != -1)
        {
            enemyListBuffs[enemyBuffIndex].buff = activeBuffs;
        }

    }
    
    private void UpdateBuff()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int index = enemyListBuffs.FindIndex(a => a.enemyName == characterTurnName);
            UpdateAbilityBuff(enemyListBuffs[index].buff);
        }
        else
        {
            UpdateAbilityBuff(activeBuffs);
        }
    }
    private void UpdateAbilityBuff(List<Buff> buffs)
    {

        for(int i = 0; i < buffs.Count; i++)
        {
            if(buffs[i].duration <= 0)
            {
                RemoveAbilityBuff(buffs[i].buffName);
                buffs.RemoveAt(i);
                continue; 
            }
            buffs[i].duration--;
        }
        
    }

    private void RemoveAbilityBuff(string name)
    {
        Skill skill = Resources.Load<Skill>("Skills/" + name);
        if (skill.name == "Rage")
        {
            int index = CasterDisplay.abilityScoreData.FindIndex(a => a.abilityScoreName == selectedSkill.associatedAbility[0].name);
            CasterDisplay.abilityScoreData[index].ablityModifierBonus = modifierBeforeRage;
            if (PhotonNetwork.IsMasterClient)
            {
                hitDiceBuff--;
            }
        }
        else if (skill.name == "Focus")
        {
            addDamageDices.Remove(skill.damageDices[0].dice);
        }
        else if (skill.name == "Ancestor Guidance")
        {
            int index = CasterDisplay.abilityScoreData.FindIndex(a => a.abilityScoreName == selectedSkill.associatedAbility[0].name);
            CasterDisplay.abilityScoreData[index].abilityScorePoint -= 2;
        }
        else if (skill.name == "Amplify Magic")
        {
            int index = CasterDisplay.abilityScoreData.FindIndex(a => a.abilityScoreName == selectedSkill.associatedAbility[0].name);
            CasterDisplay.abilityScoreData[index].abilityScorePoint -= 2;
        }
        else if (skill.name == "Blessing")
        {
            for(int i = 0; i < CasterDisplay.abilityScoreData.Count; i++)
            {
                CasterDisplay.abilityScoreData[i].abilityScorePoint--;
            }
        }

        CasterDisplay.UpdateAbilityFromBuff();
        characterDisplayPopUp.UpdateCharacterDisplay();
    }

    private void GetCasterCharacterDisplay()
    {
        List<CharacterDisplay> characterList = new List<CharacterDisplay>();
        GameObject[] heroObjects = GameObject.FindGameObjectsWithTag("Hero");
        foreach (GameObject heroObject in heroObjects)
        {
            characterList.Add(heroObject.GetComponent<CharacterDisplay>());
        }

        GameObject[] villainObjects = GameObject.FindGameObjectsWithTag("Villain");
        foreach (GameObject villainObject in villainObjects)
        {
            characterList.Add(villainObject.GetComponent<CharacterDisplay>());
        }

        CasterDisplay = characterList.Find(a => a.characterName == characterTurnName && a.photonView.IsMine);
    }
    
    void HandleDiceRollResult(int result, List<Dice> dices)
    {
        if (getHitValue)
        {
            hitValue = result;
            getHitValue = false;
        }
        else if (getDamageValue)
        {
            damageValue = result;
            getDamageValue = false;
        }
    }

    private bool IsThisMiss(int hitVal)
    {

        return false;
    }

    public void BeginTurnSetUp(string characterName)
    {
        isMyturn = true;
        characterTurnName = characterName;
        GetCasterCharacterDisplay();
        normalAction = 1;
        UpdateBuff();
    }

    public void setupCombat()
    {
        activeBuffs = new List<Buff>();

        enemyListBuffs = new List<EnemyBuff>();

        List<CharacterDisplay> enemyList = new List<CharacterDisplay>();
        GameObject[] villainObjects = GameObject.FindGameObjectsWithTag("Villain");
        foreach (GameObject villainObject in villainObjects)
        {
            enemyListBuffs.Add(new EnemyBuff(villainObject.GetComponent<CharacterDisplay>().characterName, new List<Buff>()));
        }

        hitDiceBuff = 0;
        damageDiceBuff = 0;
        addDamageDices = new List<Dice>();

        multipleTargetlist = new List<CharacterDisplay>();
    }

    [PunRPC]
    public void RPC_PlayerApplyBlessing()
    {
        if (PhotonNetwork.IsMasterClient) return;

        Buff newBuff = new Buff("Blessing", 2);
        activeBuffs.Add(newBuff);

        foreach (var ability in Caster.abilityScorepoints)
        {
            ability.abilityScorePoint++;
        }

        CasterDisplay.SetCharacterData(Caster);
        characterDisplayPopUp.UpdateCharacterDisplay();

    }

    public void RemoveAllBuff()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            GameObject[] villainObjects = GameObject.FindGameObjectsWithTag("Villain");
            foreach (GameObject villainObject in villainObjects)
            {
                CasterDisplay = villainObject.GetComponent<CharacterDisplay>();

                int index = enemyListBuffs.FindIndex(a => a.enemyName == CasterDisplay.characterName);
                foreach (Buff buff in enemyListBuffs[index].buff)
                {
                    RemoveAbilityBuff(buff.buffName);
                    enemyListBuffs[index].buff.Remove(buff);
                }
            }

        }
        else
        {
            foreach(Buff buff in activeBuffs)
            {
                RemoveAbilityBuff(buff.buffName);
                activeBuffs.Remove(buff);
            }
        }
    }
}
