using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventorySortDropdownUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown sortByDropdown;

    private void Awake()
    {
        if (sortByDropdown != null)
        {
            sortByDropdown.onValueChanged.RemoveListener(OnSortSelected);
            sortByDropdown.onValueChanged.AddListener(OnSortSelected);
            sortByDropdown.gameObject.SetActive(false);
        }
    }

    public void ToggleSortByBtn()
    {
        bool willOpen = !sortByDropdown.gameObject.activeSelf;
        sortByDropdown.gameObject.SetActive(willOpen);

        // if (willOpen)
        // {
        //     PopulateDropdown();
        // }
    }

    // private void PopulateDropdown()
    // {
    //     if (sortByDropdown == null || InventoryManager.Instance == null)
    //         return;
    //
    //     sortByDropdown.onValueChanged.RemoveListener(OnSortSelected);
    //
    //     sortByDropdown.ClearOptions();
    //
    //     List<string> options = Enum.GetNames(typeof(InventorySortType))
    //         .Select(FormatSortName)
    //         .ToList();
    //
    //     sortByDropdown.AddOptions(options);
    //
    //     InventorySortType currentSort = InventoryManager.Instance.currentSort;
    //     int currentIndex = (int)currentSort;
    //
    //     sortByDropdown.SetValueWithoutNotify(currentIndex);
    //     sortByDropdown.RefreshShownValue();
    //
    //     sortByDropdown.onValueChanged.AddListener(OnSortSelected);
    // }

    private void OnSortSelected(int index)
    {
        if (InventoryManager.Instance == null)
            return;

        InventorySortType selectedSort =
            (InventorySortType)index;

        InventoryManager.Instance.SetSort(selectedSort);
        sortByDropdown.gameObject.SetActive(false);
    }

    private string FormatSortName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return rawName;

        if (rawName.EndsWith("Asc"))
            return rawName.Replace("Asc", " Asc");

        if (rawName.EndsWith("Desc"))
            return rawName.Replace("Desc", " Desc");

        return rawName;
    }
}