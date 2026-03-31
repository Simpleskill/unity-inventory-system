using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ItemDescriptionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    //[SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text weightLabel;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text durabilityLabel;
    [SerializeField] private TMP_Text durabilityText;
    [SerializeField] private TMP_Text spoilLabel;
    [SerializeField] private TMP_Text spoilText;

    [Header("Mouse Follow")]
    [SerializeField] private Vector2 defaultOffset = new Vector2(2f, -2f);
    [SerializeField] private float screenPadding = 8f;

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        rectTransform.pivot = new Vector2(0f, 1f);

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void Update()
    {
        FollowMouseSmart();
    }

    public void Setup(InventoryItemInstance item)
    {
        if (item == null || item.itemData == null)
            return;


        nameText.text = item.itemData.itemName + "/"+ item.itemData.itemType;
        

        iconImage.sprite = item.itemData.icon;
        iconImage.enabled = item.itemData.icon != null;
        
        if (descriptionText != null)
        {
            descriptionText.text = item.itemData.description;
        }
        
        //quantityText.text = $"Quantity: {item.quantity}";
        weightText.text = $"{item.itemData.weight*item.quantity:0.##} KG";

        if (item.currentDurability < 0)
        {
            durabilityLabel.gameObject.SetActive(false);
            durabilityText.gameObject.SetActive(false);
        }
        else
        {
            durabilityText.text = $"{item.currentDurability:0} / 100";
            durabilityLabel.gameObject.SetActive(true);
            durabilityText.gameObject.SetActive(true);
        }
        
        if (item.remainingSpoilTime < 0)
        {
            spoilLabel.gameObject.SetActive(false);
            spoilText.gameObject.SetActive(false);
        }
        else
        {
            spoilText.text = $"{item.remainingSpoilTime:0}s";
            spoilLabel.gameObject.SetActive(true);
            spoilText.gameObject.SetActive(true);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void FollowMouseSmart()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // Tooltip size in screen-space approximation
        Vector2 tooltipSize = rectTransform.rect.size;

        Vector3 lossyScale = rectTransform.lossyScale;
        tooltipSize = new Vector2(
            tooltipSize.x * lossyScale.x,
            tooltipSize.y * lossyScale.y
        );

        bool fitsRight = mouseScreenPos.x + defaultOffset.x + tooltipSize.x + screenPadding <= Screen.width;
        bool fitsBelow = mouseScreenPos.y + defaultOffset.y - tooltipSize.y - screenPadding >= 0f;

        float pivotX = fitsRight ? 0f : 1f;
        float pivotY = fitsBelow ? 1f : 0f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);

        Vector2 adjustedOffset = new Vector2(
            fitsRight ? defaultOffset.x : -defaultOffset.x,
            fitsBelow ? defaultOffset.y: -defaultOffset.y-10
        );

        SetPosition(mouseScreenPos + adjustedOffset);
    }

    private void SetPosition(Vector2 screenPosition)
    {
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            RectTransform canvasRect = parentCanvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                parentCanvas.worldCamera,
                out Vector2 localPoint
            );

            rectTransform.localPosition = localPoint;
        }
        else
        {
            rectTransform.position = screenPosition;
        }
    }
}