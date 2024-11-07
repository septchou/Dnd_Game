using UnityEngine;
using System.Collections.Generic;
public enum SkillType { Damage, Buff, Debuff }

[CreateAssetMenu(fileName = "NewSkill", menuName = "Character Creation/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public SkillType skillType;
    public List<hitDice> hitDices;
    public List<damageDice> damageDices;
    
    [System.Serializable]
    public class hitDice
    {
        public Dice dice;
        public int numberOfDice;
    }
    [System.Serializable]
    public class damageDice
    {
        public Dice dice; 
        public int numberOfDice;      
    }
    
    // Ability modifier applied to the skill instead of damage modifier
    public List<AbilityScore> associatedAbility; 
}
