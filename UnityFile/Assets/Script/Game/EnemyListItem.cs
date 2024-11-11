using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class EnemyListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] Button spawnEnemyButton;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] TMP_Text enemyNameText, enemyLevelText, enemyHPText;
    [SerializeField] Character enemy;

    private bool isSpawnModeActive = false;

    void Start()
    {
        // Set up the button to start spawn mode when clicked
        spawnEnemyButton.onClick.AddListener(EnableSpawnMode);
    }

    public void SetUp(Character character)
    {
        enemy = character;
        enemyNameText.text = character.characterName;
        enemyLevelText.text = $"LV : {character.level}";
        enemyHPText.text = $"HP : {character.HP}";
    }

    private void EnableSpawnMode()
    {
        isSpawnModeActive = true;
    }

    void Update()
    {
        // Check if "spawn mode" is active and the player left-clicked
        if (isSpawnModeActive && Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Spawn mode activated.");
            // Get the mouse position in world coordinates
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Perform a raycast to check if the click was on an empty space
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider == null)
            {
                // Spawn the enemy at the mouse position
                SpawnEnemyCharacter(mousePos);
                isSpawnModeActive = false;  // Disable "spawn mode" after spawning
            }
        }

        // Check if "spawn mode" is active and the player right-clicked to cancel
        if (isSpawnModeActive && Input.GetMouseButtonDown(1))
        {
            isSpawnModeActive = false;  // Exit spawn mode
            Debug.Log("Spawn mode exited.");
        }
    }

    public void SpawnEnemyCharacter(Vector2 position)
    {
        if (enemy == null)
        {
            Debug.LogError("No character selected!");
            return;
        }

        // Spawn the enemy character at the specified position
        GameObject enemyCharacter = PhotonNetwork.Instantiate(characterPrefab.name, position, Quaternion.identity);

        // Set character details (name, HP) on the spawned character
        var characterDisplay = enemyCharacter.GetComponent<CharacterDisplay>();
        if (characterDisplay != null)
        {
            characterDisplay.SetCharacterData(enemy);
        }
    }
}
