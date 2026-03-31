using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AddItemPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private InventoryHandler inventoryHandler;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown itemDropdown;
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private TMP_InputField quantityInputField;
    [SerializeField] private TMP_InputField durabilityInputField;
    [SerializeField] private TMP_InputField spoilTimerInputField;
    [SerializeField] private TMP_Text feedbackText;
    
    public DebugPanel debugPanel;

    public List<InventoryItemData> currentItems = new List<InventoryItemData>();
    public InventoryItemData selectedItem;

    private void Start()
    {
        PopulateDropdown();

        itemDropdown.onValueChanged.AddListener(OnItemSelected);

        if (currentItems.Count > 0)
        {
            OnItemSelected(0);
        }
    }

    private void PopulateDropdown()
    {
        itemDropdown.ClearOptions();
        currentItems.Clear();

        List<string> options = new List<string>();

        foreach (InventoryItemData item in itemDatabase.allItems)
        {
            if (item == null)
                continue;

            currentItems.Add(item);
            options.Add(item.itemName);
        }

        itemDropdown.AddOptions(options);
    }

    private void OnItemSelected(int index)
    {
        if (index < 0 || index >= currentItems.Count)
            return;

        selectedItem = currentItems[index];

        // Auto-fill ID field
        idInputField.text = selectedItem.itemId.ToString();

        // Optional default values
        quantityInputField.text = "1";
        durabilityInputField.text = selectedItem.durability >= 0f ? selectedItem.durability.ToString("0") : "-1";
        spoilTimerInputField.text = selectedItem.spoilDuration >= 0f ? selectedItem.spoilDuration.ToString("0") : "-1";

        //SetFeedback("");
    }

    public void CreateItem()
    {
        if (selectedItem == null)
        {
            SetFeedback("No item selected.");
            return;
        }

        if (!TryParseQuantity(out int quantity))
            return;

        if (!TryParseDurability(out float durability))
            return;

        if (!TryParseSpoilTimer(out float spoilTimer))
            return;

        InventoryItemInstance newItem = new InventoryItemInstance(selectedItem, quantity);

        newItem.currentDurability = durability;
        newItem.remainingSpoilTime = spoilTimer;

        inventoryHandler.AddItem(newItem);

        SetFeedback($"Added {quantity}x {selectedItem.itemName}.");
    }

    private bool TryParseQuantity(out int quantity)
    {
        quantity = 0;

        if (!int.TryParse(quantityInputField.text, out quantity))
        {
            SetFeedback("Quantity must be a valid number.");
            return false;
        }

        if (quantity <= 0 || quantity >= 9999)
        {
            SetFeedback("Quantity must be greater than 0 and less than 9999.");
            return false;
        }

        return true;
    }

    private bool TryParseDurability(out float durability)
    {
        durability = -1f;

        if (!float.TryParse(durabilityInputField.text, out durability))
        {
            SetFeedback("Durability must be a valid number.");
            return false;
        }

        if (durability != -1f && (durability < 0f || durability > 100f))
        {
            SetFeedback("Durability must be between 0 and 100, or -1 if not applicable.");
            return false;
        }

        return true;
    }

    private bool TryParseSpoilTimer(out float spoilTimer)
    {
        spoilTimer = -1f;

        if (!float.TryParse(spoilTimerInputField.text, out spoilTimer))
        {
            SetFeedback("Spoil Timer must be a valid number.");
            return false;
        }

        if (spoilTimer != -1f && (spoilTimer < 0f || spoilTimer > 9999f))
        {
            SetFeedback("Spoil Timer must be between 0 and 9999, or -1 if not applicable.");
            return false;
        }

        return true;
    }

    private void SetFeedback(string message)
    {
        debugPanel.SetFeedback(message);
        // if (feedbackText != null)
        // {
        //     feedbackText.text = message;
        // }
        // else if (!string.IsNullOrEmpty(message))
        // {
        //     Debug.Log(message);
        // }
    }
}