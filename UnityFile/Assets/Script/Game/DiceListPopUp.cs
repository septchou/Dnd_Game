using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class DiceListPopUp : MonoBehaviour
{
    [SerializeField] List<Dice> diceList;
    [SerializeField] GameObject diceRollButton, diceListPanel;
    [SerializeField] Transform diceListContent;
    [SerializeField] bool isListopen = false;

    void Start()
    {
        ListDice();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopDiceList()
    {
        diceListPanel.SetActive(!isListopen);
        isListopen = !isListopen;
    }

    private void ListDice()
    {
        for(int i = 0;i < diceList.Count; i++)
        {
            Instantiate(diceRollButton, diceListContent).GetComponent<DiceListItem>().SetUp(diceList[i]);
        }
    }
}
