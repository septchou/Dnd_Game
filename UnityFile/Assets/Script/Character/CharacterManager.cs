using UnityEngine;
using System.Collections.Generic;

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


}
