using System;

[Serializable]
public class InventoryItemInstance
{
    public InventoryItemData itemData;
    public int quantity;
    public float currentDurability;
    public float remainingSpoilTime;

    public InventoryItemInstance(InventoryItemData data, int quantity)
    {
        itemData = data;
        this.quantity = quantity;
        currentDurability = data.durability;
        remainingSpoilTime = data.spoilDuration;
    }
}