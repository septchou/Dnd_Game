using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterData
{
    public string firebaseKey { get; set; }

    public string characterName;
    public string raceName;
    public string characterClassName;
    public int level;
    public int HP;
    public int abilityPoint;
    public List<AbilityScorePointsData> abilityScorePoints;
    public List<SkillData> skillData;

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
        public string skillType;
    }
}