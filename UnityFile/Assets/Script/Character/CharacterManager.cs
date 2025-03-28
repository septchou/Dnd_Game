using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;

public class CharacterManager : MonoBehaviourPunCallbacks
{
    // List to store all created characters
    [SerializeField] List<Character> characterList = new List<Character>();

    [SerializeField] List<Character> enemyList = new List<Character>();

    [SerializeField] CharacterCreation characterSelectPanel;

    // Singleton pattern to access this from anywhere in the game
    public static CharacterManager Instance;
    [SerializeField] Character templateCharacter, templateEnemy;
    public Character selectedCharacter;

    [SerializeField] GameObject playerCharacterPrefab;

    [Header("Gameplay")]
    public List<int> enemyListNumber;

    [Header("Scene")]
    [SerializeField] bool isSwitchToMap;
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

    private void Start()
    {
        InitList();
    }
    // Method to add a new character to the list
    public void AddCharacter(Character newCharacter)
    {
        characterList.Add(newCharacter);
        //Debug.Log($"Character {newCharacter.characterName} has been added.");
    }

    // Method to get all characters (if needed for the character selection page)
    public List<Character> GetCharacterList()
    {
        return characterList;
    }

    public void AddEnemy(Character newCharacter)
    {
        enemyList.Add(newCharacter);
        //Debug.Log($"Enemy {newCharacter.characterName} has been add to the list.");
    }

    public List<Character> GetEnemyList()
    {
        return enemyList;
    }

    public void RemoveCharacterByName(string characterName)
    {
        Character characterToRemove = characterList.Find(character => character.characterName == characterName);

        if (characterToRemove != null)
        {
            characterList.Remove(characterToRemove);
            //Debug.Log($"Character {characterName} has been removed from the list.");
        }
        else
        {
            Debug.LogWarning($"Character {characterName} not found in the list.");
        }
    }
    public void RemoveEnemyByName(string enemyName)
    {
        Character enemyToRemove = enemyList.Find(character => character.characterName == enemyName);

        if (enemyToRemove != null)
        {
            enemyList.Remove(enemyToRemove);
            //Debug.Log($"Enemy {enemyName} has been removed from the list.");
        }
        else
        {
            Debug.LogWarning($"Enemy {enemyName} not found in the list.");
        }
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
            characterDisplay.SetCharacterData(selectedCharacter);
        }
    }

    public void SetEnemyList()
    {
        for(int i = 0; i < enemyList.Count; i++)
        {
            enemyListNumber.Add(0);
        }
    }

    public void InitList()
    {
        enemyList = new List<Character>();
        enemyList.Add(templateEnemy);
        characterList = new List<Character>();
        characterList.Add(templateCharacter);
    }

}
