using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Data")]
    [SerializeField] public List<InventoryItemInstance> items = new List<InventoryItemInstance>();

    [Header("UI")]
    [SerializeField] public Transform slotsParent;
    [SerializeField] public GameObject slotPrefab;

    public List<InventorySlotUI> spawnedSlots = new List<InventorySlotUI>();
    
    [Header("Item Description")]
    [SerializeField] public GameObject itemDescriptionPrefab;
    [SerializeField] public Transform descriptionParent;

    
    public ItemDescriptionUI currentDescriptionUI;
    public InventorySlotUI currentHoveredSlot;
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        CacheExistingSlots();
        RefreshInventoryUI();
    }
    private void CacheExistingSlots()
    {
        spawnedSlots.Clear();

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            InventorySlotUI slot = slotsParent.GetChild(i).GetComponent<InventorySlotUI>();
            if (slot != null)
            {
                spawnedSlots.Add(slot);
            }
        }
    }
    public void AddItem(InventoryItemData itemData, int quantity = 1)
    {
        if (itemData == null)
            return;

        InventoryItemInstance existingItem = items.FirstOrDefault(i => i.itemData == itemData);

        if (existingItem != null)
        {
            existingItem.quantity += quantity;
        }
        else
        {
            items.Add(new InventoryItemInstance(itemData, quantity));
        }

        RefreshInventoryUI();
    }

    public void RemoveItem(InventoryItemData itemData, int quantity = 1)
    {
        if (itemData == null)
            return;

        InventoryItemInstance existingItem = items.FirstOrDefault(i => i.itemData == itemData);

        if (existingItem == null)
            return;

        existingItem.quantity -= quantity;

        if (existingItem.quantity <= 0)
        {
            items.Remove(existingItem);
        }

        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        ClearSpawnedSlots();

        for (int i = 0; i < items.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsParent);
            InventorySlotUI slot = slotGO.GetComponent<InventorySlotUI>();
            slot.Setup(items[i]);
            spawnedSlots.Add(slot);
        }
    }

    public void ClearSpawnedSlots()
    {
        Debug.Log("Clearing spawned slots");
        Debug.Log("It had "+spawnedSlots.Count+" items");
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
            {
                Destroy(spawnedSlots[i].gameObject);
            }
        }

        spawnedSlots.Clear();
    }

    public List<InventoryItemInstance> FilterByName(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<InventoryItemInstance>(items);

        searchText = searchText.ToLower();

        return items
            .Where(i => i.itemData != null && i.itemData.itemName.ToLower().Contains(searchText))
            .ToList();
    }

    public List<InventoryItemInstance> FilterByWeight(float minWeight, float maxWeight)
    {
        return items
            .Where(i => i.itemData != null &&
                        i.itemData.weight >= minWeight &&
                        i.itemData.weight <= maxWeight)
            .ToList();
    }

    public List<InventoryItemInstance> FilterSpoilableOnly()
    {
        return items
            .Where(i => i.itemData != null && i.itemData.spoilDuration > 0f)
            .ToList();
    }

    public void ApplyFilter(List<InventoryItemInstance> filteredItems)
    {
        ClearSpawnedSlots();

        for (int i = 0; i < filteredItems.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsParent);
            InventorySlotUI slot = slotGO.GetComponent<InventorySlotUI>();
            slot.Setup(filteredItems[i]);
            spawnedSlots.Add(slot);
        }
    }

    public void ShowAllItems()
    {
        RefreshInventoryUI();
    }
    

    public void ShowItemDescription(InventoryItemInstance item, InventorySlotUI slot)
    {
        if (item == null || item.itemData == null || itemDescriptionPrefab == null)
            return;

        if (currentHoveredSlot == slot && currentDescriptionUI != null)
            return;

        HideItemDescription();

        GameObject descriptionObject = Instantiate(itemDescriptionPrefab, descriptionParent);
        currentDescriptionUI = descriptionObject.GetComponent<ItemDescriptionUI>();

        if (currentDescriptionUI != null)
        {
            currentDescriptionUI.Setup(item);
        }

        currentHoveredSlot = slot;
    }

    public void HideItemDescription(InventorySlotUI slot = null)
    {
        if (slot != null && currentHoveredSlot != slot)
            return;

        if (currentDescriptionUI != null)
        {
            Destroy(currentDescriptionUI.gameObject);
            currentDescriptionUI = null;
        }

        currentHoveredSlot = null;
    }
}