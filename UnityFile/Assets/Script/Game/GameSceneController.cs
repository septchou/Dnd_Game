using Photon.Pun;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;
using static Skill;
using System.Reflection;

public class GameSceneController : MonoBehaviourPunCallbacks
{
    [Header("SpawnPlayerCharacter")]
    public Transform[] spawnPoints;
    public static GameSceneController Instance;

    [Header("Dice Rolling")]
    [SerializeField] int timesRollUntilFinish = 20;
    [SerializeField] float fistDelay = 0.01f, lastDelay = 0.05f;
    [SerializeField] GameObject diceRollUI;
    [SerializeField] GameObject dicePrefab;
    [SerializeField] Transform diceRollContent;
    [SerializeField] RectTransform diceRollRectTransform;
    [SerializeField] int activatedCoroutine = 0, diceRolling = 0;

    [Header("Chat")]
    [SerializeField] Chat chat;

    [Header("MapUI")]
    [SerializeField] GameObject mapUIButton;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // Each player has a unique ActorNumber in Photon

            if (CharacterManager.Instance != null)
            {
                Transform spawnPoint = GetSpawnPoint(playerIndex);
                CharacterManager.Instance.SpawnPlayerCharacter(spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                Debug.LogError("CharacterManager instance not found!");
            }

            mapUIButton.SetActive(false);
        }
        else
        {
            mapUIButton.SetActive(true);
        }

    }


    public Transform GetSpawnPoint(int index)
    {
        if (index >= 0 && index < spawnPoints.Length)
        {
            return spawnPoints[index];
        }
        else
        {
            Debug.LogWarning("Spawn point index out of range, returning default spawn point.");
            return spawnPoints[0]; // Default to the first spawn point
        }
        
    }
    public delegate void RollResultCallback(int result, List<Dice> dices);

    public void RollAnimation(List<Dice> dices, RollResultCallback callback)
    {
        StartCoroutine(RollDiceCoroutine(dices, callback));
    }

    private IEnumerator RollDiceCoroutine(List<Dice> dices, RollResultCallback callback)
    {
        activatedCoroutine++;
        List<GameObject> diceGameObjectList = new List<GameObject>();

        // Display Dice Roll UI
        for (int i = 0; i < dices.Count; i++)
        {
            diceRolling++;
            GameObject dice = Instantiate(dicePrefab, diceRollContent);

            // Set the dice name
            Transform diceNameTextTransform = dice.transform.Find("DiceNameText");
            TMP_Text diceNameText = diceNameTextTransform.GetComponent<TMP_Text>();
            diceNameText.text = $"D{dices[i].diceType}";

            diceGameObjectList.Add(dice);
        }

        float diceWidth = dicePrefab.GetComponent<RectTransform>().rect.width;
        diceRollRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (diceRolling * diceWidth) + (20 * (diceRolling - 1)) + 100);

        diceRollUI.SetActive(true);

        int result = 0;
        for (int i = 0; i < diceGameObjectList.Count; i++)
        {
            Transform sideTextTransform = diceGameObjectList[i].transform.Find("Side");
            TMP_Text sideText = sideTextTransform.GetComponent<TMP_Text>();

            // Animate the dice rolling
            for (int j = 0; j < timesRollUntilFinish; j++) //loops for the rolling effect
            {
                sideText.text = UnityEngine.Random.Range(1, dices[i].diceType + 1).ToString();

                // Start with a short delay and increase it gradually
                float delay = Mathf.Lerp(fistDelay, lastDelay, (float)j / timesRollUntilFinish);
                yield return new WaitForSeconds(delay);
            }

            // Set final dice result
            int tmp = UnityEngine.Random.Range(1, dices[i].diceType + 1);
            sideText.text = tmp.ToString();
            result += tmp;
        }



        activatedCoroutine--;
        yield return new WaitUntil(() => activatedCoroutine == 0);

        yield return new WaitForSeconds(1.5f);
        diceRollUI.SetActive(false);

        for (int i = 0; i < diceGameObjectList.Count; i++)
        {
            diceRolling--;
            Destroy(diceGameObjectList[i]);
        }

        // Send roll result to chat
        chat.SendRollResult(dices, result);

        // Call the callback with the final result
        callback?.Invoke(result, dices);


    }

}
