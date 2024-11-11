using UnityEngine;
using System.Collections.Generic;
using static Race;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character Creation/Character")]
[System.Serializable]
public class Character : ScriptableObject
{
    public string firebaseKey { get; set; }
       
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

    // Method to get the modifier of an ability score
    public int GetAbilityScoreModifier(AbilityScore ability)
    {
        // Find the AbilityScorePoints object that matches the given AbilityScore
        foreach (AbilityScorePoints score in abilityScorepoints)
        {
            if (score.abilityScore == ability)
            {
                // Calculate the modifier: (AbilityScorePoints - 10) / 2 (rounded down)
                return (score.abilityScorePoint - 10) / 2 + score.ablityModifierBonus;
            }
        }

        // If the ability is not found, return 0 (error handling or a default behavior)
        return 0;
    }
}
