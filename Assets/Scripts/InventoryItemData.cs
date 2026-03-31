using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/Item")]
public class InventoryItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;
    [SerializeField] public string itemName;
    [SerializeField] public string description;
    [SerializeField] public GameObject itemPrefab;
    [SerializeField] public Sprite icon; // UI Image

    [Header("Type")]
    [SerializeField] public ItemType itemType;
    
    [Header("Stack / Inventory")]
    //[SerializeField] public int quantity = 1;
    [SerializeField] public int maxStack = 1;
    [SerializeField] public float weight = 1f;

    [Header("Item State")]
    [SerializeField] public float spoilDuration = 2f;
    [SerializeField] public float durability = 100f;
}