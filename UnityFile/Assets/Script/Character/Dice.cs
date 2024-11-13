using UnityEngine;

[CreateAssetMenu(fileName = "NewDice", menuName = "Character Creation/Dice")]
public class Dice : ScriptableObject
{
    public int diceType;   // The type of dice (e.g., d4, d6, d8)

    public Dice(int type)
    {
        this.diceType = type;
    }
}
