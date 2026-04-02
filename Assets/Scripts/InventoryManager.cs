using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
    private bool ignoreClickThisFrame = false;

    [SerializeField] private ScrollRect inventoryScrollRect;
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

        if (inventoryScrollRect != null)
        {
            inventoryScrollRect.onValueChanged.AddListener(OnInventoryScrolled);
        }
    }

    private void Update()
    {
        if (currentActionUI == null || !currentActionUI.gameObject.activeSelf)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HideInventoryUIAction();
            return;
        }

        if (ignoreClickThisFrame)
        {
            ignoreClickThisFrame = false;
            return;
        }

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (!IsPointerOverCurrentActionUI())
            {
                HideInventoryUIAction();
            }
        }
    }
    
    private bool IsPointerOverCurrentActionUI()
    {
        if (currentActionUI == null)
            return false;

        Vector2 screenPoint = Mouse.current.position.ReadValue();

        Canvas canvas = currentActionUI.GetComponentInParent<Canvas>();
        Camera uiCamera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(currentActionUI, screenPoint, uiCamera);
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
    
    private void OnInventoryScrolled(Vector2 scrollPosition)
    {
        if (currentActionUI != null && currentActionUI.gameObject.activeSelf)
        {
            
            Debug.Log("Hidden after a scroll");
            HideInventoryUIAction();
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

        EnsureSlotIsVisible(currentSelectedSlot);
        
        if (!currentActionUI.gameObject.activeSelf)
        {
            StartCoroutine(ShowActionUIAfterAutoScroll());
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
        Debug.Log("Hide Inventory UI Action");
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
        if (slotRect == null)
            return;

        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);

        // 0 = bottom-left
        // 1 = top-left
        // 2 = top-right
        // 3 = bottom-right

        Vector3 bottomCenter = (corners[0] + corners[3]) * 0.5f ;

        currentActionUI.position = new Vector3(
            bottomCenter.x,
            bottomCenter.y ,
            currentActionUI.position.z
        );
    }
    
    public void EnsureSlotIsVisible(InventorySlotUI slot)
    {
        if (slot == null || inventoryScrollRect == null)
            return;

        RectTransform viewport = inventoryScrollRect.viewport;
        RectTransform content = inventoryScrollRect.content;
        RectTransform slotRect = slot.GetComponent<RectTransform>();

        if (viewport == null || content == null || slotRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Vector3 slotWorldPos = slotRect.position;
        Vector3 slotLocalInViewport = viewport.InverseTransformPoint(slotWorldPos);

        float slotTop = slotLocalInViewport.y;
        float slotBottom = slotTop - slotRect.rect.height;

        float viewportTop = viewport.rect.yMax;
        float viewportBottom = viewport.rect.yMin;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight)
            return;

        float hiddenHeight = contentHeight - viewportHeight;

        float topVisibilityTolerance = 6f;
        float bottomVisibilityTolerance = 2f;

        if (slotTop > viewportTop - topVisibilityTolerance)
        {
            float extraTopPadding = slotRect.rect.height * 0.5f;
            float overshootTop = (slotTop - viewportTop) + extraTopPadding + topVisibilityTolerance;
            float normalizedDelta = overshootTop / hiddenHeight;

            inventoryScrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                inventoryScrollRect.verticalNormalizedPosition + normalizedDelta
            );
        }
        else if (slotBottom < viewportBottom + bottomVisibilityTolerance)
        {
            float extraBottomPadding = slotRect.rect.height * -0.5f;
            float overshootBottom = (viewportBottom - slotBottom) + extraBottomPadding + bottomVisibilityTolerance;
            float normalizedDelta = overshootBottom / hiddenHeight;

            inventoryScrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                inventoryScrollRect.verticalNormalizedPosition - normalizedDelta
            );
        }
        Debug.Log("Ensured is visible");
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

        EnsureSlotIsVisible(currentSelectedSlot);
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

        EnsureSlotIsVisible(currentSelectedSlot);
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

        EnsureSlotIsVisible(currentSelectedSlot);
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
                currentSelectedSlot.isSelected = true;
                currentSelectedSlot.HighlightSelectedItem(true);
                EnsureSlotIsVisible(currentSelectedSlot);
                return;
            }
        }
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

    private IEnumerator ShowActionUIAfterAutoScroll()
    {
        yield return new WaitForSeconds(0f);
        ignoreClickThisFrame = true;
        PositionActionBelowSlot(currentSelectedSlot);

        if (currentActionUI != null)
            currentActionUI.gameObject.SetActive(true);
    }
    
    private bool IsSlotFullyVisible(InventorySlotUI slot)
    {
        if (slot == null || inventoryScrollRect == null)
            return false;

        RectTransform viewport = inventoryScrollRect.viewport;
        RectTransform slotRect = slot.GetComponent<RectTransform>();

        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] slotCorners = new Vector3[4];

        viewport.GetWorldCorners(viewportCorners);
        slotRect.GetWorldCorners(slotCorners);

        float viewportTop = viewportCorners[1].y;
        float viewportBottom = viewportCorners[0].y;

        float slotTop = slotCorners[1].y;
        float slotBottom = slotCorners[0].y;

        return slotTop <= viewportTop && slotBottom >= viewportBottom;
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
    private void OnDestroy()
    {
        if (inventoryScrollRect != null)
        {
            inventoryScrollRect.onValueChanged.RemoveListener(OnInventoryScrolled);
        }
    }
}