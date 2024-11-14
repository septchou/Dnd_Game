using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Xml.Serialization;
using static Character;
using System;
using UnityEngine.TextCore.Text;
using Unity.VisualScripting;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using System.Drawing;


public class CharacterCreation : MonoBehaviourPun
{
    // Default
    [Header("Character or Enemy")]
    public Character defaultCharacter;

    [Header("UI Element")]
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_Dropdown classDropdown;
    [SerializeField] TMP_Dropdown raceDropdown;
    [SerializeField] Button levelUpButton;
    [SerializeField] Button levelDownButton;
    [SerializeField] Button resetPointButton;
    [SerializeField] GameObject resetButton;
    [SerializeField] Button createCharacterButton, saveCharacterButton, deleteCharacterButton;
    [SerializeField] TMP_Text abilityScorePointText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text hpText;
    [SerializeField] List<abilityScoreUI> abilityScores;

    [Header("DataBase")]
    private DatabaseReference databaseReference;
    private FirebaseAuth auth;

    [System.Serializable]
    public class abilityScoreUI
    {
        public AbilityScore ability;
        public int abilityPoint;
        public int abilityModifier;
        public int abilityModifierBonus;
        public TMP_Text abilityScorePointText;
        public TMP_Text abilityScoreModifierText;
        public Button increaseButton, decreaseButton;

    }

    [Header("Data")]
    [SerializeField] int selectedLevel;
    [SerializeField] int selectedAbilityPoint;
    [SerializeField] Race selectedRace;
    [SerializeField] CharacterClass selectedClass;

    [Header("Scriptable Object")]
    [SerializeField] List<Race> availableRaces;  
    [SerializeField] List<CharacterClass> availableClasses;  

    [Header("Character Selection")]
    [SerializeField] TMP_Dropdown characterDropdown;
    [SerializeField] GameObject characterCreate, characterSave, characterDelete, characterSelect;
    [SerializeField] GameObject enemyCreate, enemyDelete, enemyEdit;
    [SerializeField] GameObject enemySelectionDrop, characterSelectionDrop;

    // For DM
    [Header("Enemy Creation (For DM)")]
    [SerializeField] TMP_Dropdown enemySelectionDropDown;
    [SerializeField] Button enemyCreateButton, enemyDeleteButton, enemyEditButton;
    

    private void Start()
    {
        CharacterMenuSetup();

        //Conect to Firebase Auth and Realtime Database
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                // Check if the user is logged in and load the character data
                if (auth != null && databaseReference != null && auth.CurrentUser != null)
                {
                    LoadCharacterFromFile();
                    LoadEnemyFromFile();
                }
                else
                {
                    Debug.LogError("Firebase authentication or database reference is null.");
                }
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });

        // Button
        createCharacterButton.onClick.AddListener(CreateCharacter);
        saveCharacterButton.onClick.AddListener(EditCharacter);
        deleteCharacterButton.onClick.AddListener(DeleteCharacter);
        characterDropdown.onValueChanged.AddListener(SelectCharacter);

        enemyCreateButton.onClick.AddListener(CreateCharacter);
        enemyEditButton.onClick.AddListener(EditCharacter);
        enemyDeleteButton.onClick.AddListener(DeleteCharacter);
        enemySelectionDropDown.onValueChanged.AddListener(SelectCharacter);

        levelUpButton.onClick.AddListener(LevelUP);
        levelDownButton.onClick.AddListener(LevelDOWN);
        resetPointButton.onClick.AddListener(ResetAbilityPoints);
        raceDropdown.onValueChanged.AddListener(OnRaceChange);

        // Adding listeners for ability point buttons (increase and decrease)
        foreach (var abilityUI in abilityScores)
        {
            abilityUI.increaseButton.onClick.AddListener(() => AssignAbilityPoint(abilityUI, true));
            abilityUI.decreaseButton.onClick.AddListener(() => AssignAbilityPoint(abilityUI, false));
        }
    }
    public void CharacterMenuSetup()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            List<Character> characters = CharacterManager.Instance.GetCharacterList();
            defaultCharacter = characters[0];
        }
        else
        {
            List<Character> characters = CharacterManager.Instance.GetEnemyList();
            defaultCharacter = characters[0];
        }
        PopulateCharacterDropdown();
        InitializeDropdowns();
        InitialUI(0);
        ChangeMode(PhotonNetwork.IsMasterClient, false);




        
    }
    public void InitializeDropdowns()
    {
        // Populate race dropdown
        List<string> raceNames = new List<string>();
        foreach (var race in availableRaces)
        {
            raceNames.Add(race.raceName);
        }
        raceDropdown.ClearOptions();
        raceDropdown.AddOptions(raceNames);

        // Select default character's race if set
        if (defaultCharacter != null && defaultCharacter.race != null)
        {
            selectedRace = defaultCharacter.race;
            int raceIndex = availableRaces.IndexOf(defaultCharacter.race);
            if (raceIndex >= 0)
            {
                raceDropdown.value = raceIndex;
            }
        }

        // Populate class dropdown
        List<string> classNames = new List<string>();
        foreach (var characterClass in availableClasses)
        {
            classNames.Add(characterClass.className);
        }
        classDropdown.ClearOptions();
        classDropdown.AddOptions(classNames);

        // Select default character's class if set
        if (defaultCharacter != null && defaultCharacter.characterClass != null)
        {
            selectedClass = defaultCharacter.characterClass;
            int classIndex = availableClasses.IndexOf(defaultCharacter.characterClass);
            if (classIndex >= 0)
            {
                classDropdown.value = classIndex;
            }
        }

        // Refresh the dropdowns to show the selected values
        raceDropdown.RefreshShownValue();
        classDropdown.RefreshShownValue();
    }

    public void InitialUI(int isSkipRace)
    {

        nameInputField.text = defaultCharacter.characterName;

        if (defaultCharacter == null)
        {
            Debug.LogWarning("Default Character ScriptableObject not set!");
            return;
        }

        // Loop through each ability UI and set values based on default character's abilities
        foreach (var abilityUI in abilityScores)
        {
            var defaultAbility = defaultCharacter.abilityScorepoints.Find(a => a.abilityScore == abilityUI.ability);
            if (defaultAbility != null)
            {
                abilityUI.abilityPoint = defaultAbility.abilityScorePoint;
                abilityUI.abilityModifier = 0;
                abilityUI.abilityModifierBonus = defaultAbility.ablityModifierBonus;

                abilityUI.abilityScorePointText.text = abilityUI.abilityPoint.ToString();
                int modifier = CalculateModifier(abilityUI.abilityPoint);
                if (modifier > 0) abilityUI.abilityScoreModifierText.text = $"+{modifier}";
                else abilityUI.abilityScoreModifierText.text = modifier.ToString();
            }
            else
            {
                Debug.LogWarning($"Ability '{abilityUI.ability.abilityName}' not found in  {defaultCharacter.characterName}");
            }
        }

        if (isSkipRace == 0)
        {
            // Apply each modifier from the selected race to the appropriate ability
            foreach (var modifier in selectedRace.abilityScoreModifiers)
            {
                // Find the corresponding ability in the UI and apply the modifier
                var abilityUI = abilityScores.Find(a => a.ability == modifier.abilityScore);
                if (abilityUI != null)
                {
                    // Apply modifier points
                    abilityUI.abilityModifierBonus += modifier.modifierPoints;
                    int modifiedScore = abilityUI.abilityModifier + abilityUI.abilityModifierBonus;

                    // Update UI
                    if (modifiedScore > 0) abilityUI.abilityScoreModifierText.text = $"+{modifiedScore}";
                    else abilityUI.abilityScoreModifierText.text = modifiedScore.ToString();
                }
            }
        }

        selectedAbilityPoint = defaultCharacter.abilityPoint;
        abilityScorePointText.text = $"Ability point : {defaultCharacter.abilityPoint}";

        selectedLevel = defaultCharacter.level;
        levelText.text = $"Level : {defaultCharacter.level}";

        CalculateHP();

    }

    private int CalculateModifier(int score)
    {
        return (int)Math.Floor((score - 10) / 2.0);
    }

    private void ApplyRaceBonuses()
    {
        //Apply race-specific ability score bonuses
        foreach (var modifier in selectedRace.abilityScoreModifiers)
        {
            var abilityUI = abilityScores.Find(a => a.ability == modifier.abilityScore);
            if (abilityUI != null)
            {
                abilityUI.abilityModifierBonus += modifier.modifierPoints;
                int modifiedScore = CalculateModifier(abilityUI.abilityPoint) + abilityUI.abilityModifierBonus;

                // Update UI with new modifiers
                abilityUI.abilityScoreModifierText.text = (modifiedScore > 0) ? $"+{modifiedScore}" : modifiedScore.ToString();
            }
        }
    }

    private void OnRaceChange(int index)
    {
        // Reset modifier bonuses before applying new race bonuses
        foreach (var abilityUI in abilityScores)
        {
            var defaultAbility = defaultCharacter.abilityScorepoints.Find(a => a.abilityScore == abilityUI.ability);
            abilityUI.abilityModifierBonus = defaultAbility.ablityModifierBonus; // Reset the bonus
            int modifiedScore = CalculateModifier(abilityUI.abilityPoint) + abilityUI.abilityModifierBonus;
            abilityUI.abilityScoreModifierText.text = (modifiedScore > 0) ? $"+{modifiedScore}" : modifiedScore.ToString();
        }

        // Update selected race based on the index
        selectedRace = availableRaces[index];

        // Apply race bonuses to ability scores
        ApplyRaceBonuses();

        // Recalculate HP and update
        CalculateHP();
    }

    private void CalculateHP()
    {

        // Base HP for the class
        int baseHP = selectedClass.baseHP;

        var constitutionAbility = abilityScores.Find(a => a.ability.abilityName == "Constitution");

        if (constitutionAbility != null)
        {
            // Calculate Constitution modifier
            int constitutionModifier = constitutionAbility.abilityModifier + constitutionAbility.abilityModifierBonus;

            // HP calculation: baseHP + Constitution modifier per level (multiply by level)
            int totalHP = baseHP + (4 * selectedLevel) + (constitutionModifier * selectedLevel);

            hpText.text = $"HP : {totalHP}";
        }
        else
        {
            Debug.LogWarning("Constitution ability not found!");
            hpText.text = $"HP : {baseHP}";
        }
    }

    private void LevelUP()
    {
        selectedLevel++;
        levelText.text = $"Level : {selectedLevel}";
        CalculateHP();
    }
    private void LevelDOWN()
    {
        if (selectedLevel > 1)
        {
            selectedLevel--;
        }

        // Update UI level text if necessary
        levelText.text = $"Level : {selectedLevel}";

        // Recalculate HP after leveling down
        CalculateHP();
    }

    private void ResetAbilityPoints()
    {
        InitialUI(0);
        levelText.text = $"Level : {selectedLevel}";
        CalculateHP();
    }

    private void AssignAbilityPoint(abilityScoreUI abilityUI, bool increase)
    {
        if (increase && selectedAbilityPoint > 0) // Check if we have points to assign
        {
            abilityUI.abilityPoint++;
            selectedAbilityPoint--;
        }
        else if (!increase && abilityUI.abilityPoint > 0) // Check if we have points to remove
        {
            abilityUI.abilityPoint--;
            selectedAbilityPoint++;
        }

        // Update the ability point and modifier UI
        abilityUI.abilityScorePointText.text = abilityUI.abilityPoint.ToString();

        int modifier = CalculateModifier(abilityUI.abilityPoint);
        abilityUI.abilityModifier = modifier;
        modifier += abilityUI.abilityModifierBonus;
        if (modifier > 0)
            abilityUI.abilityScoreModifierText.text = $"+{modifier}";
        else
            abilityUI.abilityScoreModifierText.text = modifier.ToString();

        // Update remaining ability points UI
        abilityScorePointText.text = $"Ability point : {selectedAbilityPoint}";
        CalculateHP();
    }

    private void CreateCharacter()
    {
        if (nameInputField.text != "New character")
        {
            //  a new Character instance
            Character newCharacter = ScriptableObject.CreateInstance<Character>();

            // Assign the character's name from the input field
            newCharacter.characterName = nameInputField.text;

            // Assign the selected race and class
            int selectedRaceIndex = raceDropdown.value;
            newCharacter.race = availableRaces[selectedRaceIndex];

            int selectedClassIndex = classDropdown.value;
            newCharacter.characterClass = availableClasses[selectedClassIndex];
            newCharacter.skills = new List<Skill>(newCharacter.characterClass.classSkills);

            // Assign ability scores and modifiers
            List<Character.AbilityScorePoints> abilityScoresList = new List<Character.AbilityScorePoints>();

            foreach (var abilityUI in abilityScores)
            {
                // Create a new AbilityScorePoints object for each ability score UI element
                Character.AbilityScorePoints newAbilityScorePoint = new Character.AbilityScorePoints
                {
                    abilityScore = abilityUI.ability,  // Assign the AbilityScore ScriptableObject
                    abilityScorePoint = abilityUI.abilityPoint,  // Assign the points
                    ablityModifierBonus = abilityUI.abilityModifierBonus  // Assign the modifier bonus (from race, etc.)
                };

                abilityScoresList.Add(newAbilityScorePoint);
            }

            newCharacter.abilityScorepoints = abilityScoresList;

            // Assign the character's level and ability points
            newCharacter.level = selectedLevel;
            newCharacter.abilityPoint = selectedAbilityPoint;

            // Calculate HP based on class and Constitution modifier
            int baseHP = newCharacter.characterClass.baseHP;
            var constitutionAbility = abilityScores.Find(a => a.ability.abilityName == "Constitution");
            int constitutionModifier = constitutionAbility.abilityModifier + constitutionAbility.abilityModifierBonus;

            // Apply Constitution modifier and level-based HP calculation
            newCharacter.HP = baseHP + (4 * newCharacter.level) + (constitutionModifier * newCharacter.level);

            
            if(!PhotonNetwork.IsMasterClient) CharacterManager.Instance.AddCharacter(newCharacter);
            else CharacterManager.Instance.AddEnemy(newCharacter);

            PopulateCharacterDropdown();

            if (!PhotonNetwork.IsMasterClient)
            {
                int characterIndex = CharacterManager.Instance.GetCharacterList().IndexOf(newCharacter);

                // Change selection character dropdown into new created one
                if (characterIndex >= 0)
                {
                    characterDropdown.value = characterIndex;
                }

                SaveCharacterToFile(newCharacter);

                Debug.Log("Character Created: " + newCharacter.characterName);
            }
            else
            {
                int characterIndex = CharacterManager.Instance.GetEnemyList().IndexOf(newCharacter);
                if (characterIndex >= 0)
                {
                    enemySelectionDropDown.value = characterIndex;
                }

                // SaveEnemytoFile()
                SaveEnemyToFile(newCharacter);

                Debug.Log("Enemy Created: " + newCharacter.characterName);
            }

           
        }

        else
        {
            Debug.LogWarning("Character not created please change the character name");
        }
    }

    public void PopulateCharacterDropdown()
    {
        List<string> characterNames = new List<string>();

        // Get the list of characters from the CharacterManager
        if (!PhotonNetwork.IsMasterClient)
        {
            List<Character> characters = CharacterManager.Instance.GetCharacterList();
            // Populate the dropdown with the names of the characters
            foreach (var character in characters)
            {
                characterNames.Add(character.characterName);
            }
            // Clear existing options and add the new ones
            characterDropdown.ClearOptions();
            characterDropdown.AddOptions(characterNames);
        }
        else
        {
            List<Character> characters = CharacterManager.Instance.GetEnemyList();
            foreach (var character in characters)
            {
                characterNames.Add(character.characterName);
            }

            enemySelectionDropDown.ClearOptions();
            enemySelectionDropDown.AddOptions(characterNames);
        }

    }

    private void SelectCharacter(int characterIndex)
    {
        // Retrieve the list of characters from the CharacterManager singleton
        List<Character> characterList = !PhotonNetwork.IsMasterClient ?
        CharacterManager.Instance.GetCharacterList() :
        CharacterManager.Instance.GetEnemyList();
        
        // Check if the index is within the bounds of the character list
        if (characterIndex >= 0 && characterIndex < characterList.Count)
        {
            // Set the default character to the selected character from the list
            defaultCharacter = characterList[characterIndex];

            // Update selected level and ability points from the chosen character
            selectedLevel = defaultCharacter.level;
            selectedAbilityPoint = defaultCharacter.abilityPoint;

            // Update the dropdowns and UI fields based on the selected character's properties
            InitializeDropdowns();

            if (characterIndex != 0)
            {
                //change to edit mode
                ChangeMode(PhotonNetwork.IsMasterClient, true);

                InitialUI(1);  // Initialize the UI without reapplying race bonuses

                // Apply the selected character’s ability scores to the UI
                foreach (var abilityUI in abilityScores)
                {
                    var selectedAbility = defaultCharacter.abilityScorepoints.Find(a => a.abilityScore == abilityUI.ability);

                    if (selectedAbility != null)
                    {
                        // Set the ability point and modifier bonus from the selected character
                        abilityUI.abilityPoint = selectedAbility.abilityScorePoint;
                        abilityUI.abilityModifierBonus = selectedAbility.ablityModifierBonus;

                        // Calculate the modifier based on the ability score
                        int modifier = CalculateModifier(abilityUI.abilityPoint);
                        abilityUI.abilityModifier = modifier;

                        // Update UI elements for each ability score
                        abilityUI.abilityScorePointText.text = abilityUI.abilityPoint.ToString();
                        int totalModifier = modifier + abilityUI.abilityModifierBonus;
                        abilityUI.abilityScoreModifierText.text = (totalModifier > 0) ? $"+{totalModifier}" : totalModifier.ToString();
                    }
                    else
                    {
                        Debug.LogWarning($"Ability '{abilityUI.ability.abilityName}' not found in {defaultCharacter.characterName}");
                    }
                }

                // Update level, ability points, and HP display
                levelText.text = $"Level : {selectedLevel}";
                abilityScorePointText.text = $"Ability point : {selectedAbilityPoint}";
                CalculateHP();
            }
            else
            {
                //change to create mode
                ChangeMode(PhotonNetwork.IsMasterClient, false);
                InitialUI(0);
            }
             
        }
        else
        {
            Debug.LogWarning("Character index out of range.");
        }
    }

    private void EditCharacter()
    {
        Character existingCharacter = !PhotonNetwork.IsMasterClient ?
        CharacterManager.Instance.GetCharacterList()[characterDropdown.value] :
        CharacterManager.Instance.GetEnemyList()[enemySelectionDropDown.value];

        existingCharacter.characterName = nameInputField.text;
        existingCharacter.race = availableRaces[raceDropdown.value];
        existingCharacter.characterClass = availableClasses[classDropdown.value];
        existingCharacter.level = selectedLevel;
        existingCharacter.abilityPoint = selectedAbilityPoint;
        existingCharacter.skills = new List<Skill>(availableClasses[classDropdown.value].classSkills);

        // Set ability scores
        existingCharacter.abilityScorepoints = new List<Character.AbilityScorePoints>();
        foreach (var abilityUI in abilityScores)
        {
            Character.AbilityScorePoints abilityScorePoints = new Character.AbilityScorePoints
            {
                abilityScore = abilityUI.ability,
                abilityScorePoint = abilityUI.abilityPoint,
                ablityModifierBonus = abilityUI.abilityModifierBonus
            };
            existingCharacter.abilityScorepoints.Add(abilityScorePoints);
        }

        // Calculate HP
        int baseHP = existingCharacter.characterClass.baseHP;
        var constitutionAbility = abilityScores.Find(a => a.ability.abilityName == "Constitution");
        int constitutionModifier = constitutionAbility.abilityModifier + constitutionAbility.abilityModifierBonus;
        existingCharacter.HP = baseHP + (4 * existingCharacter.level) + (constitutionModifier * existingCharacter.level);

        if (!PhotonNetwork.IsMasterClient)
        {
            int characterIndex = CharacterManager.Instance.GetCharacterList().IndexOf(existingCharacter);

            PopulateCharacterDropdown();
            // Change selection character dropdown into new created one
            if (characterIndex >= 0)
            {
                characterDropdown.value = characterIndex;
            }

            SaveCharacterToFile(existingCharacter);

            Debug.Log("Character Edited: " + existingCharacter.characterName);
        }
        else
        {
            int characterIndex = CharacterManager.Instance.GetEnemyList().IndexOf(existingCharacter);

            PopulateCharacterDropdown();
            // Change selection character dropdown into new created one
            if (characterIndex >= 0)
            {
                enemySelectionDropDown.value = characterIndex;
            }

            // Remove the old character and add the new one
            SaveEnemyToFile(existingCharacter);


            Debug.Log("Enemy Edited: " + existingCharacter.characterName);
        }
        
    }

    private void DeleteCharacter()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is currently logged in.");
            return;
        }

        string userId = auth.CurrentUser.UserId;
        string characterNameToDelete = defaultCharacter.characterName;

        if (!PhotonNetwork.IsMasterClient)
        {
            // delete character data from Firebase
            databaseReference.Child("character_data").Child(userId).OrderByChild("characterName").EqualTo(characterNameToDelete).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        foreach (DataSnapshot characterSnapshot in snapshot.Children)
                        {
                            // Delete data character
                            characterSnapshot.Reference.RemoveValueAsync().ContinueWithOnMainThread(removeTask =>
                            {
                                if (removeTask.IsCompleted)
                                {
                                    Debug.Log($"Character {characterNameToDelete} has been deleted from Firebase.");

                                    // Remove character from the CharacterManager
                                    CharacterManager.Instance.RemoveCharacterByName(characterNameToDelete);

                                    // Delete character from the dropdown
                                    List<Character> characterList = CharacterManager.Instance.GetCharacterList();
                                    if (characterList.Count > 0)
                                    {
                                        defaultCharacter = characterList[0];
                                        characterDropdown.value = 0;
                                    }
                                    else
                                    {
                                        defaultCharacter = null;
                                        Debug.Log("No characters left after deletion.");
                                    }
                                    PopulateCharacterDropdown();
                                }
                                else
                                {
                                    Debug.LogError($"Failed to delete character {characterNameToDelete} from Firebase: " + removeTask.Exception);
                                }
                            });
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Character {characterNameToDelete} not found in Firebase to delete.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to retrieve character data for deletion: " + task.Exception);
                }
            });
        }
        else
        {
            //delete enemy data from Firebase
            databaseReference.Child("enemy_data").Child(userId).OrderByChild("characterName").EqualTo(characterNameToDelete).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        foreach (DataSnapshot characterSnapshot in snapshot.Children)
                        {
                            // Delete data character
                            characterSnapshot.Reference.RemoveValueAsync().ContinueWithOnMainThread(removeTask =>
                            {
                                if (removeTask.IsCompleted)
                                {
                                    Debug.Log($"Enemy {characterNameToDelete} has been deleted from Firebase.");

                                    // Remove character from the CharacterManager
                                    CharacterManager.Instance.RemoveEnemyByName(characterNameToDelete);

                                    // Delete character from the dropdown
                                    List<Character> characterList = CharacterManager.Instance.GetEnemyList();
                                    if (characterList.Count > 0)
                                    {
                                        defaultCharacter = characterList[0];
                                        enemySelectionDropDown.value = 0;
                                    }
                                    else
                                    {
                                        defaultCharacter = null;
                                        Debug.Log("No enemy left after deletion.");
                                    }
                                    PopulateCharacterDropdown();
                                }
                                else
                                {
                                    Debug.LogError($"Failed to delete enemy {characterNameToDelete} from Firebase: " + removeTask.Exception);
                                }
                            });
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Enemy {characterNameToDelete} not found in Firebase to delete.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to retrieve enemy data for deletion: " + task.Exception);
                }
            });
        }
    }


    public void SaveCharacterToFile(Character character)
    {
        if (character == null)
        {
            Debug.LogError("Character is null in SaveCharacterToFile!");
            return;
        }

        //check user is logged in
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is currently logged in.");
            return;
        }

        // convert Character to CharacterData and serialize to JSON
        CharacterData characterData = ConvertCharacterToData(character);
        string json = JsonUtility.ToJson(characterData, true);

        // save the JSON string to a file in realtime database with the user's ID
        string userId = auth.CurrentUser.UserId;

        Debug.Log("Character firebaseKey: " + character.firebaseKey);
        //Check that it new character or existing character
        if ( string.IsNullOrEmpty(character.firebaseKey)) {
            DatabaseReference newCharacterRef = databaseReference.Child("character_data").Child(userId).Push();
            newCharacterRef.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    character.firebaseKey = newCharacterRef.Key;
                    Debug.Log("Character data saved successfully to Firebase.");
                }
                else
                {
                    Debug.LogError("Failed to save character data: " + task.Exception);
                }
            });
        }
        else
        {
            databaseReference.Child("character_data").Child(userId).Child(character.firebaseKey).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Character data saved(edit) successfully to Firebase.");
                }
                else
                {
                    Debug.LogError("Failed to save character data: " + task.Exception);
                }
            });
        }
    }

    // Save Enemy to Firebase
    public void SaveEnemyToFile(Character character)
    {
        if (character == null)
        {
            Debug.LogError("Character is null in SaveCharacterToFile!");
            return;
        }

        //check user is logged in
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is currently logged in.");
            return;
        }

        // convert Character(Enemy) to CharacterData and serialize to JSON
        CharacterData characterData = ConvertCharacterToData(character);
        string json = JsonUtility.ToJson(characterData, true);

        // save the JSON string to a file in realtime database with the user's ID
        string userId = auth.CurrentUser.UserId;

        //check that it new character or existing character
        if (string.IsNullOrEmpty(character.firebaseKey))
        {
            DatabaseReference newCharacterRef = databaseReference.Child("enemy_data").Child(userId).Push();
            newCharacterRef.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    character.firebaseKey = newCharacterRef.Key;
                    Debug.Log("Enemy data saved successfully to Firebase.");
                }
                else
                {
                    Debug.LogError("Failed to save enemy data: " + task.Exception);
                }
            });
        }
        else
        {
            databaseReference.Child("enemy_data").Child(userId).Child(character.firebaseKey).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Enemy data saved(edit) successfully to Firebase.");
                }
                else
                {
                    Debug.LogError("Failed to save enemy data: " + task.Exception);
                }
            });
        }
    }

    private CharacterData ConvertCharacterToData(Character character)
    {

        CharacterData data = new CharacterData
        {
            firebaseKey = character.firebaseKey,

            characterName = character.characterName,
            raceName = character.race != null ? character.race.name : "",
            characterClassName = character.characterClass != null ? character.characterClass.className : "",
            level = character.level,
            HP = character.HP,
            abilityPoint = character.abilityPoint,
            abilityScorePoints = new List<CharacterData.AbilityScorePointsData>(),
            skillData = new List<CharacterData.SkillData>()
        };

        // Convert ability score points
        if (character.abilityScorepoints != null) 
        {
            foreach (var abilityScorePoint in character.abilityScorepoints)
            {
                CharacterData.AbilityScorePointsData abilityData = new CharacterData.AbilityScorePointsData
                {
                    abilityScoreName = abilityScorePoint.abilityScore != null ? abilityScorePoint.abilityScore.abilityName : "",
                    abilityScorePoint = abilityScorePoint.abilityScorePoint,
                    ablityModifierBonus = abilityScorePoint.ablityModifierBonus
                };
                data.abilityScorePoints.Add(abilityData);
            }
        }

        // Convert skills
        if (character.skills != null) 
        {
            foreach (var skill in character.skills)
            {
                CharacterData.SkillData skillData = new CharacterData.SkillData
                {
                    skillName = skill.skillName,
                    skillType = skill.skillType.ToString()
                };
                data.skillData.Add(skillData);
            }
        }

        return data;
    }

    // Load a character from a JSON file
    public void LoadCharacterFromFile()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is currently logged in.");
            return;
        }

        // Get the user's ID and load the character data from the database
        string userId = auth.CurrentUser.UserId;
        databaseReference.Child("character_data").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists || snapshot.ChildrenCount == 0)
                {
                    Debug.LogWarning("No characters found for this user.");
                    Debug.Log("This user has no characters. Please create a new character.");
                    return;
                }
                foreach (DataSnapshot characterSnapshot in snapshot.Children)
                {
                    string key = characterSnapshot.Key;

                    string json = characterSnapshot.GetRawJsonValue();
                    CharacterData characterData = JsonUtility.FromJson<CharacterData>(json);

                    // Convert CharacterData to Character
                    Character character = ConvertDataToCharacter(characterData);
                    character.firebaseKey = key;
                    Debug.Log("Loaded character: " + character.characterName);

                    // Add the loaded character to the CharacterManager
                    CharacterManager.Instance.AddCharacter(character);
                }
                PopulateCharacterDropdown();
            }
            else
            {
                Debug.LogError("Failed to load character data: " + task.Exception);
            }
        });
    }

    //Load Enemy from Firebase
    public void LoadEnemyFromFile()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is currently logged in.");
            return;
        }

        // Get the user's ID and load the character data from the database
        string userId = auth.CurrentUser.UserId;
        databaseReference.Child("enemy_data").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists || snapshot.ChildrenCount == 0)
                {
                    Debug.LogWarning("No enemy found for this user.");
                    Debug.Log("This user has no enemy. Please create a new enemy.");
                    return;
                }
                foreach (DataSnapshot characterSnapshot in snapshot.Children)
                {
                    string key = characterSnapshot.Key;

                    string json = characterSnapshot.GetRawJsonValue();
                    CharacterData characterData = JsonUtility.FromJson<CharacterData>(json);

                    // Convert CharacterData to Character
                    Character character = ConvertDataToCharacter(characterData);
                    character.firebaseKey = key;
                    Debug.Log("Loaded enemy: " + character.characterName);

                    // Add the loaded character to the CharacterManager
                    CharacterManager.Instance.AddEnemy(character);
                }
                PopulateCharacterDropdown();
            }
            else
            {
                Debug.LogError("Failed to load enemy data: " + task.Exception);
            }
        });
    }


    // Convert CharacterData to Character
    private Character ConvertDataToCharacter(CharacterData data)
    {
        Character character = ScriptableObject.CreateInstance<Character>();

        character.firebaseKey = data.firebaseKey;

        character.characterName = data.characterName;
        character.race = Resources.Load<Race>("Races/" + data.raceName);
        character.characterClass = Resources.Load<CharacterClass>("Classes/" + data.characterClassName);
        character.level = data.level;
        character.HP = data.HP;
        character.abilityPoint = data.abilityPoint;

        character.abilityScorepoints = new List<Character.AbilityScorePoints>();
        foreach (var abilityData in data.abilityScorePoints)
        {
            Character.AbilityScorePoints abilityScorePoint = new Character.AbilityScorePoints
            {
                abilityScore = Resources.Load<AbilityScore>("AbilityScores/" + abilityData.abilityScoreName),
                abilityScorePoint = abilityData.abilityScorePoint,
                ablityModifierBonus = abilityData.ablityModifierBonus
            };
            character.abilityScorepoints.Add(abilityScorePoint);
        }

        character.skills = new List<Skill>();
        foreach (var skillData in data.skillData)
        {
            Skill skill = Resources.Load<Skill>("Skills/" + skillData.skillName);
            character.skills.Add(skill);
        }
        
        return character;
    }

    public void DisplayCharacterData(Character character)
    {
        if (character == null)
        {
            Debug.LogError("Character is not assigned!");
            return;
        }

        // Display character basic information
        Debug.Log("Character Name: " + character.characterName);
        Debug.Log("Race: " + (character.race != null ? character.race.name : "None"));
        Debug.Log("Class: " + (character.characterClass != null ? character.characterClass.className : "None"));
        Debug.Log("Level: " + character.level);
        Debug.Log("HP: " + character.HP);
        Debug.Log("Ability Points: " + character.abilityPoint);

        // Display ability score points and modifiers
        Debug.Log("Ability Scores:");
        foreach (var abilityScorePoint in character.abilityScorepoints)
        {
            Debug.Log($" - {(abilityScorePoint.abilityScore != null ? abilityScorePoint.abilityScore.abilityName : "Unknown Ability")}: " +
                     $"Points: {abilityScorePoint.abilityScorePoint}, " +
                     $"Modifier Bonus: {abilityScorePoint.ablityModifierBonus}");
        }

        // Display skills
        if (character.skills != null && character.skills.Count > 0)
        {
            Debug.Log("Skills:");
            foreach (var skill in character.skills)
            {
                Debug.Log($" - {skill.skillName} (Type: {skill.skillType})");
            }
        }
        else
        {
            Debug.Log("No skills available.");
        }
    }

    public void ChangeMode(bool isMaster, bool isEditMode)
    {
        if (!isMaster)
        {
            characterSelectionDrop.SetActive(true);
            enemySelectionDrop.SetActive(false);
            if (!isEditMode)
            {
                characterSave.SetActive(false);
                characterCreate.SetActive(true);
                resetButton.SetActive(true);
                characterDelete.SetActive(false);
                characterSelect.SetActive(false);
            }
            else
            {
                characterSave.SetActive(true);
                characterCreate.SetActive(false);
                resetButton.SetActive(false);
                characterDelete.SetActive(true);
                characterSelect.SetActive(true);

            }
            enemyEdit.SetActive(false);
            enemyCreate.SetActive(false);
            enemyDelete.SetActive(false);
        }
        else
        {
            characterSelectionDrop.SetActive(false);
            enemySelectionDrop.SetActive(true);
            if (!isEditMode)
            {
                enemyEdit.SetActive(false);
                enemyCreate.SetActive(true);
                resetButton.SetActive(true);
                enemyDelete.SetActive(false);
            }
            else
            {
                enemyEdit.SetActive(true);
                enemyCreate.SetActive(false);
                resetButton.SetActive(false);
                enemyDelete.SetActive(true);

            }
            characterSave.SetActive(false);
            characterCreate.SetActive(false);
            characterDelete.SetActive(false);
            characterSelect.SetActive(false);
        }
        
    }


}

