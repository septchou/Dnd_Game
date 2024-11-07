using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityScore", menuName = "Character Creation/AbilityScore")]
public class AbilityScore : ScriptableObject
{
    public string abilityName; // e.g., "Strength", "Dexterity", etc.
}
