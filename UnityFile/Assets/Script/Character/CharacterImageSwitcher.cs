using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterImageSwitcher : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage characterImage; // Reference to the RawImage UI component

    [Header("Default Texture")]
    public Texture defaultTexture; // Fallback image if class and race combo is missing

    [Header("Character Creation")]
    [SerializeField] CharacterCreation characterCreation;

    private Dictionary<string, Texture> imageDictionary;

    private void Start()
    {
        // Initialize the dictionary
        imageDictionary = new Dictionary<string, Texture>();

        // Preload textures (you can add these manually or load them dynamically)
        LoadCharacterImages();
    }

    // Method to set the character image based on class and race
    public void SetCharacterImage(string characterClass, string characterRace)
    {
        string key = $"{characterRace}_{characterClass}";

        if (imageDictionary.TryGetValue(key, out Texture texture))
        {
            // Set the RawImage's texture
            characterImage.texture = texture;
        }
        else
        {
            // Use a default texture if no match found
            characterImage.texture = defaultTexture;
            Debug.LogWarning($"No texture found for {key}, using default.");
        }
    }

    // Preload all character images into the dictionary
    private void LoadCharacterImages()
    {
        List<Race> avaliableRace = characterCreation.availableRaces;
        List<CharacterClass> avaliableClass = characterCreation.availableClasses;
        foreach (Race race in avaliableRace)
        {
            foreach(CharacterClass characterClass in avaliableClass)
            {
                string path = $"CharacterImages/{race.raceName}/{race.raceName}_{characterClass.className}";
                Texture texture = Resources.Load<Texture>(path);
                if (texture != null)
                {
                    imageDictionary.Add($"{race.raceName}_{characterClass.className}", texture);
                }
                else
                {
                    Debug.Log(path + " not found");
                }
            }
        }
        
    }
}
