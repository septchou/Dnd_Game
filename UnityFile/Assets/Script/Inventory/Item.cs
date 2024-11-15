using System;
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
    public string iconPath;
    public ItemType type;

    public Sprite GetIcon()
    {
        return Resources.Load<Sprite>(iconPath);
    }
}

[System.Serializable]
public class ItemData
{
    public string itemID;
    public string itemName;
    public string description;
    public int quantity;
    public string iconPath;
    public string type;  

    // Constructor
    public ItemData(string id, string name, string desc, int qty, string icon, string itemType)
    {
        itemID = id;
        itemName = name;
        description = desc;
        quantity = qty;
        iconPath = icon;
        type = itemType;
    }

    // Convert Item to ItemData
    public static ItemData FromItem(Item item)
    {
        return new ItemData(
            item.itemID,
            item.itemName,
            item.description,
            item.quantity,
            item.iconPath,
            item.type.ToString() 
        );
    }

    // Convert ItemData to Item
    public Item ToItem()
    {
        Item item = ScriptableObject.CreateInstance<Item>();
        item.itemID = itemID;
        item.itemName = itemName;
        item.description = description;
        item.quantity = quantity;
        item.iconPath = iconPath;

        if (Enum.TryParse(type, out ItemType itemType))
        {
            item.type = itemType;
        }
        else
        {
            item.type = ItemType.Consumable; // Default type
        }

        return item;
    }
}

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Material
}
