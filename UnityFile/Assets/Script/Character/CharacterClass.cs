using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewClass", menuName = "Character Creation/Class")]
public class CharacterClass : ScriptableObject
{
    public string className;
    public int baseHP;
    public List<Skill> classSkills; 
}
