using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class CharacterManager : MonoBehaviour
{
    // List to store all created characters
    [SerializeField] List<Character> characterList = new List<Character>();

    // Singleton pattern to access this from anywhere in the game
    public static CharacterManager Instance;

    private void Awake()
    {
        // Ensure the instance of this manager is accessible throughout the game
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to add a new character to the list
    public void AddCharacter(Character newCharacter)
    {
        characterList.Add(newCharacter);
        Debug.Log($"Character {newCharacter.characterName} has been added.");
    }

    // Method to get all characters (if needed for the character selection page)
    public List<Character> GetCharacterList()
    {
        return characterList;
    }

    public void RemoveCharacterByName(string characterName)
    {
        Character characterToRemove = characterList.Find(character => character.characterName == characterName);

        if (characterToRemove != null)
        {
            characterList.Remove(characterToRemove);
            Debug.Log($"Character {characterName} has been removed from the list.");
        }
        else
        {
            Debug.LogWarning($"Character {characterName} not found in the list.");
        }
    }
}
