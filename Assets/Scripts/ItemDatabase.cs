using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<InventoryItemData> allItems = new List<InventoryItemData>();

    public InventoryItemData GetItemById(string id)
    {
        return allItems.FirstOrDefault(item => item != null && item.itemId == int.Parse(id));
    }

    public InventoryItemData GetItemByName(string name)
    {
        return allItems.FirstOrDefault(item => item != null && item.itemName == name);
    }

    public List<InventoryItemData> GetItemsByType(ItemType type)
    {
        return allItems.Where(item => item != null && item.itemType == type).ToList();
    }
}