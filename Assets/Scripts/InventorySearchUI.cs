using TMPro;
using UnityEngine;

public class InventorySearchUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchInput;

    private void Awake()
    {
        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveListener(OnSearchValueChanged);
            searchInput.onValueChanged.AddListener(OnSearchValueChanged);
        }
    }

    private void OnDestroy()
    {
        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveListener(OnSearchValueChanged);
        }
    }

    public void OnSearchValueChanged(string searchText)
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.ApplySearchFilter(searchText);
    }
}