using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRace", menuName = "Character Creation/Race")]
public class Race : ScriptableObject
{
    public string raceName;  // Name of the race (e.g., "Elf", "Dwarf", etc.)
    public List<AbilityScoreModifier> abilityScoreModifiers;  // List of AbilityScoreModifiers

    // Helper class to hold AbilityScore and its modifier points
    [System.Serializable]
    public class AbilityScoreModifier
    {
        public AbilityScore abilityScore;  // The AbilityScore being modified
        public int modifierPoints;         // The number of modifier points to add
    }
    
}
