using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDatabase", menuName = "Character Creation/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> allItems;
}
