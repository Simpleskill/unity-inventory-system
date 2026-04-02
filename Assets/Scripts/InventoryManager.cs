using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Data")]
    [SerializeField] public List<InventoryItemInstance> items = new List<InventoryItemInstance>();

    [Header("Sorting")]
    [SerializeField] public  InventorySortType currentSort = InventorySortType.NameAsc;

    [Header("UI")]
    [SerializeField] public Transform slotsParent;
    [SerializeField] public GameObject slotPrefab;
    
    [Header("Filters")]
    [SerializeField] public Transform sortByBtn;
    [SerializeField] public TMP_Dropdown sortByDropdown;

    public List<InventorySlotUI> spawnedSlots = new List<InventorySlotUI>();

    [Header("Item Description")]
    [SerializeField] public GameObject itemDescriptionPrefab;
    [SerializeField] public Transform descriptionParent;

    public ItemDescriptionUI currentDescriptionUI;
    public InventorySlotUI currentHoveredSlot;
    public InventorySlotUI currentSelectedSlot;
    
    [Header("Item Action UI")]
    [SerializeField] public GameObject itemActionPrefab;
    [SerializeField] public Transform actionParent;

    public RectTransform currentActionUI;

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
        CreateDescriptionInstance();
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
    private void CreateActionInstance()
    {
        if (itemActionPrefab == null || actionParent == null)
            return;

        if (currentActionUI != null)
            return;

        GameObject actionObject = Instantiate(itemActionPrefab, actionParent);
        currentActionUI = actionObject.GetComponent<RectTransform>();

        if (currentActionUI != null)
        {
            currentActionUI.gameObject.SetActive(false);
        }
    }
    public void ToggleInventoryUIAction(bool show)
    {
        if (!show)
        {
            HideInventoryUIAction();
            return;
        }

        if (currentSelectedSlot == null)
        {
            HideInventoryUIAction();
            return;
        }

        if (currentActionUI == null)
        {
            CreateActionInstance();
        }

        if (currentActionUI == null)
            return;

        PositionActionBelowSlot(currentSelectedSlot);

        if (!currentActionUI.gameObject.activeSelf)
        {
            currentActionUI.gameObject.SetActive(true);
        }
    }

    public void ToggleOnOffUIAction()
    {
        if (!currentActionUI)
        {
            ToggleInventoryUIAction(true);
            return;
        }

        if (!currentActionUI.gameObject.activeSelf)
        {
            
            ToggleInventoryUIAction(true);
            return;
        }
        
        ToggleInventoryUIAction(false);
    }
    
    public void HideInventoryUIAction()
    {
        if (currentActionUI != null)
        {
            currentActionUI.gameObject.SetActive(false);
        }
    }
    
    private void PositionActionBelowSlot(InventorySlotUI slot)
    {
        if (slot == null || currentActionUI == null)
            return;

        RectTransform slotRect = slot.GetComponent<RectTransform>();
        RectTransform actionRect = currentActionUI;

        if (slotRect == null)
            return;

        Vector3 slotPosition = slotRect.position;

        float offsetX = 3f;
        float offsetY = -slotRect.rect.height + 0;

        actionRect.position = new Vector3(
            slotPosition.x + offsetX,
            slotPosition.y + offsetY,
            0
        );
    }
    
    public void SwapSelected(InventorySlotUI slot)
    {
        if (slot == null)
            return;

        // Se clicou no slot já selecionado, desseleciona
        if (currentSelectedSlot == slot)
        {
            currentSelectedSlot.isSelected = false;
            currentSelectedSlot.HighlightSelectedItem(false);
            currentSelectedSlot = null;
            HideInventoryUIAction();
            return;
        }

        // Remove seleção anterior
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.isSelected = false;
            currentSelectedSlot.HighlightSelectedItem(false);
        }

        HideInventoryUIAction();

        // Seleciona o novo slot
        currentSelectedSlot = slot;
        currentSelectedSlot.isSelected = true;
        currentSelectedSlot.HighlightSelectedItem(true);
    }
    public void SelectSlot(InventorySlotUI slot)
    {
        if (slot == null)
            return;

        if (currentSelectedSlot == slot)
            return;

        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.isSelected = false;
            currentSelectedSlot.HighlightSelectedItem(false);
        }

        HideInventoryUIAction();

        currentSelectedSlot = slot;
        currentSelectedSlot.isSelected = true;
        currentSelectedSlot.HighlightSelectedItem(true);
    }

    public void DeselectCurrentSlot()
    {
        if (currentSelectedSlot == null)
            return;

        currentSelectedSlot.isSelected = false;
        currentSelectedSlot.HighlightSelectedItem(false);
        currentSelectedSlot = null;

        HideInventoryUIAction();
    }
    
    public void SelectNextSlot()
    {
        if (currentSelectedSlot == null)
            return;

        if (spawnedSlots == null || spawnedSlots.Count <= 1)
            return;

        int currentIndex = spawnedSlots.IndexOf(currentSelectedSlot);

        if (currentIndex < 0)
            return;

        int nextIndex = (currentIndex + 1) % spawnedSlots.Count;

        currentSelectedSlot.isSelected = false;
        currentSelectedSlot.HighlightSelectedItem(false);
        HideInventoryUIAction();

        currentSelectedSlot = spawnedSlots[nextIndex];
        currentSelectedSlot.isSelected = true;
        currentSelectedSlot.HighlightSelectedItem(true);
    }

    public void SelectPreviousSlot()
    {
        if (currentSelectedSlot == null)
            return;

        if (spawnedSlots == null || spawnedSlots.Count <= 1)
            return;

        int currentIndex = spawnedSlots.IndexOf(currentSelectedSlot);

        if (currentIndex < 0)
            return;

        int previousIndex = currentIndex - 1;

        if (previousIndex < 0)
            previousIndex = spawnedSlots.Count - 1;

        currentSelectedSlot.isSelected = false;
        currentSelectedSlot.HighlightSelectedItem(false);
        HideInventoryUIAction();

        currentSelectedSlot = spawnedSlots[previousIndex];
        currentSelectedSlot.isSelected = true;
        currentSelectedSlot.HighlightSelectedItem(true);
    }
    
    private InventorySlotUI GetSlotUIByItem(InventoryItemInstance item)
    {
        foreach (InventorySlotUI slot in spawnedSlots)
        {
            if (slot != null && slot.CurrentItem == item)
                return slot;
        }

        return null;
    }

    private void CreateDescriptionInstance()
    {
        if (itemDescriptionPrefab == null || descriptionParent == null)
            return;

        if (currentDescriptionUI != null)
            return;

        GameObject descriptionObject = Instantiate(itemDescriptionPrefab, descriptionParent);
        currentDescriptionUI = descriptionObject.GetComponent<ItemDescriptionUI>();

        if (currentDescriptionUI != null)
        {
            currentDescriptionUI.gameObject.SetActive(false);
        }
    }

    public void RefreshInventoryUI()
    {
        SortItems(currentSort);
        DrawItems(items);
    }

    private void DrawItems(List<InventoryItemInstance> itemList)
    {
        ClearSpawnedSlots();

        for (int i = 0; i < itemList.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsParent);
            InventorySlotUI slot = slotGO.GetComponent<InventorySlotUI>();
            slot.Setup(itemList[i]);
            spawnedSlots.Add(slot);
        }

        RestoreSelectionAfterRedraw();
    }

    private void RestoreSelectionAfterRedraw()
    {
        if (spawnedSlots == null || spawnedSlots.Count == 0)
        {
            currentSelectedSlot = null;
            return;
        }

        if (currentSelectedSlot == null)
        {
            return;
        }

        InventoryItemInstance selectedItem = currentSelectedSlot.CurrentItem;

        foreach (InventorySlotUI slot in spawnedSlots)
        {
            if (slot != null && slot.CurrentItem == selectedItem)
            {
                currentSelectedSlot = slot;
                currentSelectedSlot.HighlightSelectedItem(true);
                return;
            }
        }

        // // Se o item selecionado deixou de existir no filtro, seleciona o primeiro visível
        // currentSelectedSlot = spawnedSlots[0];
        // currentSelectedSlot.HighlightSelectedItem(true);
    }
    public void ClearSpawnedSlots()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
            {
                Destroy(spawnedSlots[i].gameObject);
            }
        }

        spawnedSlots.Clear();
        currentSelectedSlot = null;
        HideItemDescription();
        HideInventoryUIAction();
    }

    public void SetSort(InventorySortType sortType)
    {
        currentSort = sortType;
        RefreshInventoryUI();
    }

    public void SortItems(InventorySortType sortType)
    {
        currentSort = sortType;

        switch (sortType)
        {
            case InventorySortType.NameAsc:
                items = items
                    .OrderBy(i => i.itemData != null ? i.itemData.itemName : string.Empty)
                    .ToList();
                break;

            case InventorySortType.NameDesc:
                items = items
                    .OrderByDescending(i => i.itemData != null ? i.itemData.itemName : string.Empty)
                    .ToList();
                break;

            case InventorySortType.WeightAsc:
                items = items
                    .OrderBy(i => i.itemData != null ? i.itemData.weight * i.quantity : float.MaxValue)
                    .ThenBy(i => i.itemData != null ? i.itemData.itemName : string.Empty)
                    .ToList();
                break;

            case InventorySortType.WeightDesc:
                items = items
                    .OrderByDescending(i => i.itemData != null ? i.itemData.weight * i.quantity : float.MinValue)
                    .ThenBy(i => i.itemData != null ? i.itemData.itemName : string.Empty)
                    .ToList();
                break;

            case InventorySortType.SpoilTimeAsc:
                items = items
                    .OrderBy(i => i.itemData != null ? i.remainingSpoilTime : float.MaxValue)
                    .ThenBy(i => i.itemData != null ? i.itemData.itemName : string.Empty)
                    .ToList();
                break;

            case InventorySortType.SpoilTimeDesc:
                items = items
                    .OrderByDescending(i => i.itemData != null ? i.remainingSpoilTime : float.MinValue)
                    .ThenBy(i => i.itemData != null ? i.itemData.itemName : string.Empty)
                    .ToList();
                break;
        }
    }
    public List<InventoryItemInstance> FilterByName(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<InventoryItemInstance>(items);

        searchText = searchText.Trim().ToLower();

        return items
            .Where(i =>
                i.itemData != null &&
                !string.IsNullOrEmpty(i.itemData.itemName) &&
                i.itemData.itemName.ToLower().Contains(searchText))
            .ToList();
    }
    
    public void ApplySearchFilter(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            RefreshInventoryUI();
            return;
        }

        List<InventoryItemInstance> filteredItems = items
            .Where(i =>
                i.itemData != null &&
                !string.IsNullOrEmpty(i.itemData.itemName) &&
                i.itemData.itemName.ToLower().Contains(searchText.Trim().ToLower()))
            .ToList();

        ApplyFilter(filteredItems);
    }
    
    public void ApplyFilter(List<InventoryItemInstance> filteredItems)
    {
        // opcional: manter também os filtros ordenados pelo sort atual
        List<InventoryItemInstance> sortedFilteredItems = SortItemList(filteredItems, currentSort);
        DrawItems(sortedFilteredItems);
    }

    private List<InventoryItemInstance> SortItemList(List<InventoryItemInstance> itemList, InventorySortType sortType)
    {
        switch (sortType)
        {
            case InventorySortType.NameAsc:
                return itemList.OrderBy(i => i.itemData != null ? i.itemData.itemName : string.Empty).ToList();

            case InventorySortType.NameDesc:
                return itemList.OrderByDescending(i => i.itemData != null ? i.itemData.itemName : string.Empty).ToList();

            case InventorySortType.WeightAsc:
                return itemList.OrderBy(i => i.itemData != null ? i.itemData.weight : float.MaxValue).ToList();

            case InventorySortType.WeightDesc:
                return itemList.OrderByDescending(i => i.itemData != null ? i.itemData.weight : float.MinValue).ToList();

            case InventorySortType.SpoilTimeAsc:
                return itemList.OrderBy(i => i.itemData != null ? i.itemData.spoilDuration : float.MaxValue).ToList();

            case InventorySortType.SpoilTimeDesc:
                return itemList.OrderByDescending(i => i.itemData != null ? i.itemData.spoilDuration : float.MinValue).ToList();

            default:
                return itemList;
        }
    }

    public void ShowAllItems()
    {
        RefreshInventoryUI();
    }

    public void ShowItemDescription(InventoryItemInstance item, InventorySlotUI slot)
    {
        if (item == null || item.itemData == null)
            return;

        if (currentDescriptionUI == null)
        {
            CreateDescriptionInstance();
        }

        if (currentDescriptionUI == null)
            return;

        if (currentHoveredSlot != slot)
        {
            currentHoveredSlot = slot;
            currentDescriptionUI.Setup(item);
        }

        if (!currentDescriptionUI.gameObject.activeSelf)
        {
            currentDescriptionUI.gameObject.SetActive(true);
        }
    }

    public void HideItemDescription(InventorySlotUI slot = null)
    {
        if (slot != null && currentHoveredSlot != slot)
            return;

        if (currentDescriptionUI != null)
        {
            currentDescriptionUI.gameObject.SetActive(false);
        }

        currentHoveredSlot = null;
    }
    public void ToggleSortByBtn()
    {
        sortByDropdown.gameObject.SetActive(!sortByDropdown.isActiveAndEnabled);

        if (sortByDropdown.isActiveAndEnabled)
        {
            
        }
    }
    
}