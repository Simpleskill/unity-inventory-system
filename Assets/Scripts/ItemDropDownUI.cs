using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemDropdownUI : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        PopulateDropdown();
    }

    public void PopulateDropdown()
    {
        dropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (var item in itemDatabase.allItems)
        {
            if (item != null)
                options.Add(item.itemName);
        }

        dropdown.AddOptions(options);
    }
}