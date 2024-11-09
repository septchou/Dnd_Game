using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;

public class CharacterManager : MonoBehaviourPunCallbacks
{
    // List to store all created characters
    [SerializeField] List<Character> characterList = new List<Character>();

    [SerializeField] CharacterCreation characterSelectPanel;

    // Singleton pattern to access this from anywhere in the game
    public static CharacterManager Instance;

    public Character selectedCharacter;

    [SerializeField] GameObject playerCharacterPrefab;

    private void Awake()
    {
        // Ensure the instance of this manager is accessible throughout the game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);  // Make persistent across scenes
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

    public void ConfirmSelectCharacter()
    {
        selectedCharacter = characterSelectPanel.defaultCharacter;
    }

    public void SpawnPlayerCharacter(Vector3 position, Quaternion rotation)
    {

        if (selectedCharacter == null)
        {
            Debug.LogError("No character selected!");
            return;
        }

        // Use PhotonNetwork.Instantiate to spawn the character at the specified position and rotation
        GameObject playerCharacter = PhotonNetwork.Instantiate(playerCharacterPrefab.name, position, rotation);

        // Set character details (name, HP) on the spawned character
        var characterDisplay = playerCharacter.GetComponent<CharacterDisplay>();
        if (characterDisplay != null)
        {
            characterDisplay.SetCharacterInfo(selectedCharacter.characterName, selectedCharacter.HP);
        }
    }
}
