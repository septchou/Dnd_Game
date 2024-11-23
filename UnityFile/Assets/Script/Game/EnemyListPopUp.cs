using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;

public class EnemyListPopUp : MonoBehaviourPun
{
    [SerializeField] GameObject enemyListButton, enemylistPanel, enemyList, turnListUI;
    [SerializeField] Transform enemyListContent;
    [SerializeField] bool isListopen = false;
    [SerializeField] OverloyOverMouse overlay;
    [SerializeField] GameObject overlayUI;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            enemyListButton.SetActive(true);
            ListEnemy();
        }
    }

    public void PopEnemeyList()
    {
        
        isListopen = !isListopen;
        enemylistPanel.SetActive(!enemylistPanel.activeSelf);
        if (enemylistPanel.activeSelf)
        {
            turnListUI.SetActive(false);
        }
    }

    private void ListEnemy()
    {
        List<Character> characters = CharacterManager.Instance.GetEnemyList();
        CharacterManager.Instance.SetEnemyList();

        for (int i = 1; i < characters.Count; i++) 
        {
            Instantiate(enemyList, enemyListContent).GetComponent<EnemyListItem>().SetUp(characters[i],overlay,overlayUI);
        }
    }
}
