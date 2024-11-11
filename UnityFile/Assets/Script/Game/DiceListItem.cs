using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceListItem : MonoBehaviour
{
    [SerializeField] Dice diceRoll;
    [SerializeField] TMP_Text sideText;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUp(Dice dice)
    {
        diceRoll = dice;
        sideText.text = $"D{dice.diceType}";
    }

    public void RollDice()
    {
        List<Dice> dices = new List<Dice>();
        dices.Add(diceRoll);
        GameSceneController.Instance.RollAnimation(dices, HandleDiceRollResult);
    }

    private void HandleDiceRollResult(int result, List<Dice> dices)
    {

    }
}
