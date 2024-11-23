using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class CharacterInitiative
{
    public string CharacterName;
    public int InitiativeRoll;
    public int ActorNumber;

    public CharacterInitiative(string name, int roll, int actorNumber)
    {
        CharacterName = name;
        InitiativeRoll = roll;
        ActorNumber = actorNumber;
    }
}

public class TurnManager : MonoBehaviourPunCallbacks
{
    [Header("Turn")]
    [SerializeField] int turnNow = -1;
    [SerializeField] List<Dice> initiativeDices;
    [SerializeField] List<CharacterDisplay> characterList = new List<CharacterDisplay>();

    [SerializeField] List<CharacterInitiative> initiativeResults = new List<CharacterInitiative>();
    [SerializeField] bool isCombatActive = false;
    [SerializeField] bool isTurnListOn = false;

    [Header("UI")]
    [SerializeField] GameObject enemyListUI;
    [SerializeField] GameObject combatPhaseUI;
    [SerializeField] GameObject turnList;
    [SerializeField] GameObject turnListItemPrefab;
    [SerializeField] Transform turnListContent;
    [SerializeField] GameObject changePhaseButtonGameObject;
    [SerializeField] GameObject endTurnButtonGameObject;
    [SerializeField] List<TurnListItem> existTurnList = new List<TurnListItem>();

    [Header("Combat")]
    [SerializeField] Combat combat;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            changePhaseButtonGameObject.SetActive(true);
        }
    }

    
    public void SwitchTurnList()
    {
        isTurnListOn = !isTurnListOn;
        turnList.SetActive(!turnList.activeSelf);
        if (turnList.activeSelf)
        {
            enemyListUI.SetActive(false);
        }
    }
    public void ChangePhase()
    {
        isCombatActive = !isCombatActive;
        if (isCombatActive == false)
        {
            EndCombat();
            changePhaseButtonGameObject.GetComponentInChildren<TMP_Text>().text = "Phase: Exploration";
        }
        else
        {
            StartTurn();
            changePhaseButtonGameObject.GetComponentInChildren<TMP_Text>().text = "Phase: Combat";
        }
        
    }
    private void StartTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isCombatActive = true;
            GetCharacterList();
            foreach (var character in characterList)
            {
                if (character == null || character.photonView == null || character.photonView.Owner == null)
                    continue;

                photonView.RPC("RPC_RequestInitiativeRoll", character.photonView.Owner, character.characterName);
            }
        }
    }

    private void GetCharacterList()
    {
        GameObject[] heroObjects = GameObject.FindGameObjectsWithTag("Hero");
        foreach (GameObject heroObject in heroObjects)
        {
            characterList.Add(heroObject.GetComponent<CharacterDisplay>()); 
        }

        GameObject[] villainObjects = GameObject.FindGameObjectsWithTag("Villain");
        foreach (GameObject villainObject in villainObjects)
        {
            characterList.Add(villainObject.GetComponent<CharacterDisplay>()); 
        }
    }

    [PunRPC]
    public void RPC_RequestInitiativeRoll(string characterName)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            GetCharacterList();
        }
        CharacterDisplay character = characterList.Find(c => c.characterName == characterName);
        if (character != null)
            RollInitiativeLocally(character);
    }
    private void RollInitiativeLocally(CharacterDisplay character)
    {
        GameSceneController.Instance.RollAnimation(initiativeDices, (result, dices) =>
        {
            photonView.RPC("RPC_SendInitiativeResult", RpcTarget.MasterClient, character.characterName, result, PhotonNetwork.LocalPlayer.ActorNumber);
        });
    }

    [PunRPC]
    public void RPC_SendInitiativeResult(string characterName, int initiativeRoll , int actorNumber, PhotonMessageInfo info)
    {
        // Only the MasterClient handles collecting all initiative results
        if (!PhotonNetwork.IsMasterClient) return;

        CharacterInitiative newInitiative = new CharacterInitiative(characterName, initiativeRoll, actorNumber);
        initiativeResults.Add(newInitiative);

        // If all characters have rolled, sort and broadcast the turn order
        if (initiativeResults.Count == characterList.Count)
        {
            SetupTurnOrder();
        }
    }


    private void SetupTurnOrder()
    {
        initiativeResults.Sort((a, b) => b.InitiativeRoll.CompareTo(a.InitiativeRoll));

        // Separate into serializable arrays
        string[] names = new string[initiativeResults.Count];
        int[] rolls = new int[initiativeResults.Count];
        int[] actorNumbers = new int[initiativeResults.Count];

        for (int i = 0; i < initiativeResults.Count; i++)
        {
            names[i] = initiativeResults[i].CharacterName;
            rolls[i] = initiativeResults[i].InitiativeRoll;
            actorNumbers[i] = initiativeResults[i].ActorNumber;
        }

        photonView.RPC("RPC_SetTurnOrder", RpcTarget.All, names, rolls, actorNumbers);

    }

    [PunRPC]
    public void RPC_SetTurnOrder(string[] names, int[] rolls, int[] actorNumbers)
    {
        for (int i = 0; i < names.Length; i++)
        {
            CharacterInitiative initiative = new CharacterInitiative(names[i], rolls[i], actorNumbers[i]);
            //orderedChracterList.Add(names[i]);

            if (!PhotonNetwork.IsMasterClient)
            {
                initiativeResults.Add(initiative);
            }
        }

        combat.setupCombat();
        isCombatActive = true;
        turnNow = -1;

        CreateTurnListUI();
        enemyListUI.SetActive(false);
        combatPhaseUI.SetActive(true);
        isTurnListOn = true;

        if (PhotonNetwork.IsMasterClient)
        {
            StartNextTurn();
        }

    }

    public void StartNextTurn()
    {
        
        if (!isCombatActive) return;

        photonView.RPC("RPC_NotifyNextTurn", RpcTarget.All);

    }

    [PunRPC]
    public void RPC_NotifyNextTurn()
    {
        combat.isMyturn = false;

        turnNow++;
        if (turnNow >= initiativeResults.Count) turnNow = 0;

        endTurnButtonGameObject.SetActive(false);

        int targetActorNumber = initiativeResults[turnNow].ActorNumber;
        Photon.Realtime.Player targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(targetActorNumber);

        photonView.RPC("RPC_LetPlayerEndTurn", targetPlayer, initiativeResults[turnNow].CharacterName);
        //Debug.Log(turnNow);
        UpdateTurnListUI();
    }

    [PunRPC]
    public void RPC_LetPlayerEndTurn(string characterName)
    {
        combat.BeginTurnSetUp(characterName);
        endTurnButtonGameObject.SetActive(true);
    }
    public void EndCombat()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_EndCombat", RpcTarget.All);
            Debug.Log("Combat has ended.");
        }
    }

    [PunRPC]
    public void RPC_EndCombat()
    {
        combat.isMyturn = false;

        combat.RemoveAllBuff();

        isTurnListOn = false;
        combatPhaseUI.SetActive(false);
        endTurnButtonGameObject.SetActive(false);

        isCombatActive = false;
        characterList.Clear();
        initiativeResults.Clear();
        for (int i = 0; i < existTurnList.Count; i++)
        {
            existTurnList[i].DestroyListItem();
        }
        existTurnList.Clear();
        Debug.Log("Combat has ended for all players.");
    }

    private void CreateTurnListUI()
    {
        for (int i = 0; i < initiativeResults.Count; i++)
        {
            existTurnList.Add(Instantiate(turnListItemPrefab, turnListContent).GetComponent<TurnListItem>());
            existTurnList[i].nameText.text = $"{i+1}. {initiativeResults[i].CharacterName}";

        }
    }
    private void UpdateTurnListUI()
    {
        existTurnList[turnNow].nameText.text = $"{turnNow + 1}. {initiativeResults[turnNow].CharacterName} (Current turn)";
        existTurnList[turnNow].nameText.color = Color.black;
        existTurnList[turnNow].highlightBackGround.SetActive(true);

        int previousTurn = turnNow - 1;
        if (turnNow == 0) previousTurn = initiativeResults.Count-1;
        existTurnList[previousTurn].nameText.text = $"{previousTurn+1}. {initiativeResults[previousTurn].CharacterName}";
        existTurnList[previousTurn].nameText.color = Color.white;
        existTurnList[previousTurn].highlightBackGround.SetActive(false);
    }
}
