using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private Slider durabilitySlider;
    [SerializeField] private Slider spoilSlider;

    private InventoryItemInstance currentItem;
    
    public bool isSelected = false;

    public InventoryItemInstance CurrentItem => currentItem;

    public void Setup(InventoryItemInstance itemInstance)
    {
        currentItem = itemInstance;

        if (currentItem == null || currentItem.itemData == null)
        {
            Clear();
            return;
        }

        itemIcon.sprite = currentItem.itemData.icon;
        itemIcon.enabled = currentItem.itemData.icon != null;

        quantityText.text = $"x{currentItem.quantity}";
        weightText.text = $"{(currentItem.itemData.weight * currentItem.quantity):0.##} KG";

        if (currentItem.currentDurability < 0)
        {
            durabilitySlider.gameObject.SetActive(false);
        }
        else
        {
            durabilitySlider.gameObject.SetActive(true);
            durabilitySlider.value = Mathf.Clamp01(currentItem.currentDurability / 100f);
        }

        if (currentItem.remainingSpoilTime < 0 || currentItem.itemData.spoilDuration <= 0)
        {
            spoilSlider.gameObject.SetActive(false);
        }
        else
        {
            spoilSlider.gameObject.SetActive(true);
            float percent = currentItem.remainingSpoilTime / currentItem.itemData.spoilDuration;
            spoilSlider.value = Mathf.Clamp01(percent);
        }
    }
    
    public void HighlightSelectedItem(bool isSelected)
    {
        if (itemIcon == null)
            return;

        Color baseColor = Color.white;
        Image backgroundImage = GetComponent<Image>();
        
        if (isSelected)
        {
            float darkenFactor = 0.7f; // quanto menor, mais escuro
            Color darkerColor = new Color(
                baseColor.r * darkenFactor,
                baseColor.g * darkenFactor,
                baseColor.b * darkenFactor,
                baseColor.a
            );

            backgroundImage.color = darkerColor;
        }
        else
        {
            // volta à cor original (branco mantém a sprite normal)
            backgroundImage.color = Color.white;
        }
    }
    public void ToggleSelection()
    {
        isSelected = !isSelected;
        HighlightSelectedItem(isSelected);
        InventoryManager.Instance.SwapSelected(this);
    }
    public void ToggleActionsUI()
    {
        InventoryManager.Instance.ToggleOnOffUIAction();
    }

    public void Clear()
    {
        currentItem = null;

        itemIcon.sprite = null;
        itemIcon.enabled = false;

        quantityText.text = "";
        weightText.text = "";

        durabilitySlider.value = 0;
        spoilSlider.value = 0;

        durabilitySlider.gameObject.SetActive(false);
        spoilSlider.gameObject.SetActive(false);
    }

    public void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem == null || InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.ShowItemDescription(currentItem, this);
    }
    public void SelectThisSlot()
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.SelectSlot(this);
    }

    public void DeselectThisSlot()
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.DeselectCurrentSlot();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.HideItemDescription(this);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (InventoryManager.Instance == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InventoryManager.Instance.currentSelectedSlot == this)
                DeselectThisSlot();
            else
                SelectThisSlot();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (InventoryManager.Instance.currentSelectedSlot != this)
                SelectThisSlot();

            ToggleActionsUI();
        }
    }
}