using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;

public class EnemyListPopUp : MonoBehaviourPun
{
    [SerializeField] GameObject enemyListButton, enemylistPanel, enemyList;
    [SerializeField] Transform enemyListContent;
    [SerializeField] bool isListopen = false;
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
        enemylistPanel.SetActive(!isListopen);
        isListopen = !isListopen;

    }

    private void ListEnemy()
    {
        List<Character> characters = CharacterManager.Instance.GetEnemyList();
        CharacterManager.Instance.SetEnemyList();

        for (int i = 1; i < characters.Count; i++) 
        {
            Instantiate(enemyList, enemyListContent).GetComponent<EnemyListItem>().SetUp(characters[i]);
        }
    }
}
