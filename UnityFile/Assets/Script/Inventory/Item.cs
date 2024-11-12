using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Character Creation/Item")]

[System.Serializable]
public class Item : ScriptableObject
{
    public string itemID;        
    public string itemName;      
    public string description;   
    public int quantity;         
    public Sprite icon;          
    public ItemType type;        

    // Constructor
    public Item(string id, string name, string desc, int qty, ItemType itemType, Sprite itemIcon)
    {
        itemID = id;
        itemName = name;
        description = desc;
        quantity = qty;
        type = itemType;
        icon = itemIcon;
    }
}

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Material
}
