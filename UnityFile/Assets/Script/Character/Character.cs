using UnityEngine;
using System.Collections.Generic;
using static Race;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character Creation/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public Race race;                      // The race of the character
    public CharacterClass characterClass;  // The class of the character
    public int level;                      // Character's level
    public int HP;                         // Health points of the character
    public int abilityPoint;
    public List<AbilityScorePoints> abilityScorepoints;
    // Ability scores (using AbilityScore ScriptableObjects)
    [System.Serializable]
    public class AbilityScorePoints
    {
        public AbilityScore abilityScore;  
        public int abilityScorePoint;
        public int ablityModifierBonus;
    }

    // Skills available for the character
    public List<Skill> skills;

    // Default value for abilities
    const int defaultAbilityPoints = 20;

}
