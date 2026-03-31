using System.Linq;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    public InventoryManager inventoryManager;

    public InventoryItemInstance cookedMeatTest;

    public void AddItem(InventoryItemInstance item)
    {
        // Validate input and references
        if (inventoryManager == null || item == null || item.itemData == null)
            return;

        int remainingAmount = item.quantity;
        int maxStack = item.itemData.maxStack;

        // 1. Find existing stacks of the same item that are not full
        var existingStacks = inventoryManager.items
            .Where(i => i.itemData == item.itemData && i.quantity < i.itemData.maxStack)
            .ToList();

        // Try to fill existing stacks first
        for (int i = 0; i < existingStacks.Count; i++)
        {
            if (remainingAmount <= 0)
                break;

            InventoryItemInstance existingItem = existingStacks[i];
            int spaceLeft = maxStack - existingItem.quantity;

            if (spaceLeft <= 0)
                continue;

            // Add as much as possible to this stack
            int amountToAdd = Mathf.Min(remainingAmount, spaceLeft);
            existingItem.quantity += amountToAdd;
            remainingAmount -= amountToAdd;
        }

        // 2. If there is still remaining amount, create new stacks
        while (remainingAmount > 0)
        {
            int amountForNewSlot = Mathf.Min(remainingAmount, maxStack);

            // Create a new inventory item instance
            InventoryItemInstance newItem = new InventoryItemInstance(item.itemData, amountForNewSlot);

            // Copy runtime state (durability and spoil time)
            newItem.currentDurability = item.currentDurability;
            newItem.remainingSpoilTime = item.remainingSpoilTime;

            inventoryManager.items.Add(newItem);

            remainingAmount -= amountForNewSlot;
        }

        // Refresh UI after changes
        inventoryManager.RefreshInventoryUI();
    }

    public void AddCookedMeat()
    {
        AddItem(cookedMeatTest);
    }
}