using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks
{
    [Header("Turn")]
    [SerializeField] int currentTurn = 0;
    [SerializeField] List<CharacterDisplay> characterList;
    [Header("UI")]
    [SerializeField] GameObject combatPhaseUI;
    [SerializeField] GameObject changePhaseButtonGameObject;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            changePhaseButtonGameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("TurnStart", RpcTarget.All);
        }
    }

    [PunRPC]
    public void NotifyTurnStart(string characterName)
    {
        

    }
}
