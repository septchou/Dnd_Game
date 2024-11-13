using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Playables;
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

    [Header("Character")]
    public string characterTurnName;
    [SerializeField] string selectedEnemyName;
    [SerializeField] Character Caster;
    [SerializeField] CharacterDisplay Target;

    [Header("List")]
    [SerializeField] List<Skill> skills;
    [SerializeField] List<Button> skillButtons;
    [SerializeField] List<GameObject> skillButtonsGameobject;

    [Header("Stat")]
    [SerializeField] float delay = 3f;
    public bool isMyturn = false;
    public int normalAction = 1, bonusAction = 1;
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
        int isEnoughAction = 0;
        string skillName = selectedSkill.name;
        int FlatHit, FlatDmg;
        int levelScaledDice = (Caster.level / 5);
        hitValue = 0;
        damageValue = 0;

        if(skillName != "Heal")
        {
            getHitValue = true;
            GameSceneController.Instance.RollAnimation(selectedSkill.GetHitDiceLists(hitDiceBuff), HandleDiceRollResult);
        }
        
        List<Dice> totalDamageDices = new List<Dice>();
        totalDamageDices.AddRange(selectedSkill.GetDamageDiceLists(levelScaledDice + damageDiceBuff));
        totalDamageDices.AddRange(addDamageDices);

        getDamageValue = true;
        GameSceneController.Instance.RollAnimation(totalDamageDices, HandleDiceRollResult);

        yield return new WaitForSeconds(delay);

        if (skillName == "Slash")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            // Hit Dice: 1d20 + STR Modifier		
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            hitValue += FlatHit;

            // Damage: 1d10 + STR Modifier
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
        }
        else if (skillName == "Single shot")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            // Hit Dice: 1d20 + DEX Modifier		
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            hitValue += FlatHit;

            // Damage: 1d6 + DEX Modifier
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
        }
        else if (skillName == "Multi shots")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            if (!isCastMultiple)
            {
                normalAction++;
                isCastMultiple = true;
                multipleTarget = 3;
                multipleTargetlist.Add(Target);
            }

            // Hit Dice: 1d20 + DEX Modifier		
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            hitValue += FlatHit;

            // Damage: 1d4 + DEX Modifier
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
        }
        else if(skillName == "Fireball")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            // Hit Dice: 1d20 + INT Modifier		
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            hitValue += FlatHit;

            // Damage: 1d8 + INT Modifier
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
        }
        else if (skillName == "Lightning Stike")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            // Hit Dice: 1d20 + INT Modifier		
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            hitValue += FlatHit;

            // Damage: 1d8 + INT Modifier
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
        }
        else if (skillName == "Ice shot")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            // Hit Dice: 1d20 + INT Modifier		
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            hitValue += FlatHit;

            // Damage: 1d6 + INT Modifier
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
        }
        else if (skillName == "Heal")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            // Damage: 1d6 + INT Modifier
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
            damageValue *= Caster.level;
        }
        else if (skillName == "Turn undead")
        {
            //How ????
        }
        else if (skillName == "Holy Shot")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            // Hit Dice: 1d20 + INT Modifier 		
            FlatHit = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            hitValue += FlatHit;

            // Damage: 3d4 + INT Modifier
            FlatDmg = Caster.GetAbilityScoreModifier(selectedSkill.associatedAbility[0]);
            damageValue += FlatDmg;
        }

        if (isEnoughAction == 0)
        {
            if(skillName != "Heal")
            {
                bool isMiss = IsThisMiss(hitValue);
                chatLog.SendSkillReport(skillName, damageValue, hitValue, isMiss);
                if (!isMiss)
                {
                    Target.ChangeHP(damageValue * -1);
                }
            }
            else
            {
                chatLog.SendHealReport(damageValue,Target.characterName);
                Target.ChangeHP(damageValue);
            }

        }
        else if (isEnoughAction == 1)
        {
            Debug.Log("Not enough normal action point");
        }
        else if (isEnoughAction == 2)
        {
            Debug.Log("Not enough bonus action point");
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

    public void setupCombat()
    {
        activeBuffs = new List<Buff>();
        enemyListBuffs = new List<EnemyBuff>();
        hitDiceBuff = 0;
        damageDiceBuff = 0;
        addDamageDices = new List<Dice>();

        multipleTargetlist = new List<CharacterDisplay>();
    }
    private void CastBuff(int index)
    {
        selectedSkill = skills[index];
        string skillName = selectedSkill.name;
        int isEnoughAction = 0;

        if (skillName == "Rage")
        {
            if (bonusAction > 0) bonusAction--;
            else isEnoughAction = 2;

            Buff newBuff = new Buff(skillName, 2);
            activeBuffs.Add(newBuff);

            // STR mod +1
            int i = Caster.abilityScorepoints.FindIndex(a => a.abilityScore == selectedSkill.associatedAbility[0]);
            Caster.abilityScorepoints[i].ablityModifierBonus++;

            // more hit dice
            hitDiceBuff++;

        }
        else if (skillName == "Roar")
        {
            // How???
        }
        else if (skillName == "Warrior’s sense")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

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

            CharacterDisplay casterDisplay = characterList.Find(CharacterDisplay => CharacterDisplay.characterName == characterTurnName);
            casterDisplay.isDodge = true;
        }
        else if (skillName == "Focus")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            addDamageDices.Add(selectedSkill.damageDices[0].dice);
        }
        else if (skillName == "Ancestor Guidance")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            Buff newBuff = new Buff(skillName, 2);
            activeBuffs.Add(newBuff);

            // Dex mod +2
            int i = Caster.abilityScorepoints.FindIndex(a => a.abilityScore == selectedSkill.associatedAbility[0]);
            Caster.abilityScorepoints[i].ablityModifierBonus+=2;
        }
        else if (skillName == "Amplify Magic")
        {
            if (normalAction > 0) normalAction--;
            else isEnoughAction = 1;

            Buff newBuff = new Buff(skillName, 2);
            activeBuffs.Add(newBuff);

            // INT mod +2
            int i = Caster.abilityScorepoints.FindIndex(a => a.abilityScore == selectedSkill.associatedAbility[0]);
            Caster.abilityScorepoints[i].ablityModifierBonus += 2;
        }

        if (isEnoughAction == 0)
        {
            chatLog.SendSkillBuffReport(skillName);
        }
        else if (isEnoughAction == 1)
        {
            Debug.Log("Not enough normal action point");
        }
        else if (isEnoughAction == 2)
        {
            Debug.Log("Not enough bonus action point");
        }
        
    }
    
    public void UpdateBuff()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            int index = enemyListBuffs.FindIndex(a => a.enemyName == "characterTurnName");
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
            int index = Caster.abilityScorepoints.FindIndex(a => a.abilityScore == skill.associatedAbility[0]);
            Caster.abilityScorepoints[index].ablityModifierBonus--;
            hitDiceBuff--;
        }
        else if (skill.name == "Focus")
        {
            addDamageDices.Remove(skill.damageDices[0].dice);
        }
        else if (skill.name == "Ancestor Guidance")
        {
            int index = Caster.abilityScorepoints.FindIndex(a => a.abilityScore == skill.associatedAbility[0]);
            Caster.abilityScorepoints[index].ablityModifierBonus -= 2;
        }
        else if (skill.name == "Amplify Magic")
        {
            int index = Caster.abilityScorepoints.FindIndex(a => a.abilityScore == skill.associatedAbility[0]);
            Caster.abilityScorepoints[index].ablityModifierBonus -= 2;
        }
    }
    
}
