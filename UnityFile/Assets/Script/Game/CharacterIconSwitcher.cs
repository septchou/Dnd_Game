using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterIconSwitcher : MonoBehaviour
{
    public static CharacterIconSwitcher Instance;
    [SerializeField] List<Race> avaliableRace;
    [SerializeField] List<CharacterClass> avaliableClass;

    private Dictionary<string, Sprite> imageCircleDictionary;
    private Dictionary<string, Texture> imageSquareDictionary;
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadCharacterIcon();
    }

    private void LoadCharacterIcon()
    {
        imageCircleDictionary = new Dictionary<string, Sprite>();
        imageSquareDictionary = new Dictionary<string, Texture>();
        foreach (Race race in avaliableRace)
        {
            foreach (CharacterClass characterClass in avaliableClass)
            {
                string path = $"CharacterImages/{race.raceName}/{race.raceName}_{characterClass.className}_Circle";
                if (race.raceName == "Zombie")
                {
                    path = $"CharacterImages/{race.raceName}/{race.raceName}_Circle";
                }
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    imageCircleDictionary.Add($"{race.raceName}_{characterClass.className}", sprite);
                }
                else
                {
                    Debug.Log(path + " not found");
                }

                path = $"CharacterImages/{race.raceName}/{race.raceName}_{characterClass.className}_Square";
                if (race.raceName == "Zombie")
                {
                    path = $"CharacterImages/{race.raceName}/{race.raceName}_Square";
                }
                Texture texture = Resources.Load<Texture>(path);
                if (texture != null)
                {
                    imageSquareDictionary.Add($"{race.raceName}_{characterClass.className}", texture);
                }
                else
                {
                    Debug.Log(path + " not found");
                }

            }
        }


    }

    public void SetCharacterIcon(string characterClass, string characterRace, RawImage rawImage, SpriteRenderer image, bool isCircle)
    {
        //Debug.Log($"Loaded {imageCircleDictionary.Count} circle icons and {imageSquareDictionary.Count} square textures.");

        string key = $"{characterRace}_{characterClass}";

        if (!isCircle)
        {
            if (imageSquareDictionary == null) LoadCharacterIcon();

            if (rawImage != null && imageSquareDictionary.TryGetValue(key, out Texture texture))
            {
                rawImage.texture = texture;
            }
            else
            {
                Debug.LogWarning($"No texture found for {key}, or RawImage is null.");
            }
        }
        else
        {
            if (imageCircleDictionary == null) LoadCharacterIcon();

            if (image != null && imageCircleDictionary.TryGetValue(key, out Sprite sprite))
            {
                image.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"No sprite found for {key}, or SpriteRenderer is null.");
            }
        }
    }

}
