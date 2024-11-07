using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Serialization;
using static Character;
using System;

public class CharacterCreation : MonoBehaviour
{
    // Default
    [SerializeField] Character defaultCharacter;

    // UI Element
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_Dropdown classDropdown;
    [SerializeField] TMP_Dropdown raceDropdown;
    [SerializeField] Button levelUpButton;
    [SerializeField] Button levelDownButton;
    [SerializeField] Button resetButton;
    [SerializeField] Button createCharacterButton;
    [SerializeField] TMP_Text abilityScorePointText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text hpText;
    [SerializeField] List<abilityScoreUI> abilityScores;

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

    [SerializeField] int selectedLevel;
    [SerializeField] int selectedAbilityPoint;
    [SerializeField] Race selectedRace;
    [SerializeField] CharacterClass selectedClass;

    // Scriptable Objects
    [SerializeField] List<Race> availableRaces;  // List of available races 
    [SerializeField] List<CharacterClass> availableClasses;  // List of available classes 

    //CharacterSelection
    [SerializeField] TMP_Dropdown characterDropdown;
    [SerializeField] GameObject createButton, saveButton;

    private void Start()
    {
        InitializeDropdowns();
        InitialUI(0);
        PopulateCharacterDropdown();

        // Button
        createCharacterButton.onClick.AddListener(CreateCharacter);
        levelUpButton.onClick.AddListener(LevelUP);
        levelDownButton.onClick.AddListener(LevelDOWN);
        resetButton.onClick.AddListener(ResetAbilityPoints);
        raceDropdown.onValueChanged.AddListener(OnRaceChange);
        characterDropdown.onValueChanged.AddListener(SelectCharacter);

        // Adding listeners for ability point buttons (increase and decrease)
        foreach (var abilityUI in abilityScores)
        {
            abilityUI.increaseButton.onClick.AddListener(() => AssignAbilityPoint(abilityUI, true));
            abilityUI.decreaseButton.onClick.AddListener(() => AssignAbilityPoint(abilityUI, false));
        }

    }

    private void InitializeDropdowns()
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

    private void InitialUI(int isSkipRace)
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
                Debug.LogWarning($"Ability '{abilityUI.ability.abilityName}' not found in default character.");
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
        return (score - 10) / 2;
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
        //  a new Character instance
        Character newCharacter = ScriptableObject.CreateInstance<Character>();

        // Assign the character's name from the input field
        newCharacter.characterName = nameInputField.text;

        // Assign the selected race and class
        int selectedRaceIndex = raceDropdown.value;
        newCharacter.race = availableRaces[selectedRaceIndex];

        int selectedClassIndex = classDropdown.value;
        newCharacter.characterClass = availableClasses[selectedClassIndex];

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

        CharacterManager.Instance.AddCharacter(newCharacter);
        PopulateCharacterDropdown();

        int characterIndex = CharacterManager.Instance.GetCharacterList().IndexOf(newCharacter);
        if (characterIndex >= 0)
        {
            characterDropdown.value = characterIndex;
        }

        Debug.Log("Character Created: " + newCharacter.characterName);
    }

    private void PopulateCharacterDropdown()
    {
        List<string> characterNames = new List<string>();

        // Get the list of characters from the CharacterManager
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

    private void SelectCharacter(int characterIndex)
    {
        // Retrieve the list of characters from the CharacterManager singleton
        List<Character> characterList = CharacterManager.Instance.GetCharacterList();

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
                        Debug.LogWarning($"Ability '{abilityUI.ability.abilityName}' not found in selected character.");
                    }
                }

                // Update level, ability points, and HP display
                levelText.text = $"Level : {selectedLevel}";
                abilityScorePointText.text = $"Ability point : {selectedAbilityPoint}";
                CalculateHP();
            }
            else
            {
                InitialUI(0);
            }
             
        }
        else
        {
            Debug.LogWarning("Character index out of range.");
        }
    }


}
