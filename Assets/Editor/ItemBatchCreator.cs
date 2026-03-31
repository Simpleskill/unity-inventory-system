using UnityEditor;
using UnityEngine;
using System.IO;

public static class ItemBatchCreator
{
    private class ItemCreationData
    {
        public int itemId;
        public string itemName;
        public string description;
        public ItemType itemType;
        public int maxStack;
        public float weight;
        public float spoilDuration;
        public float durability;

        public ItemCreationData(
            int itemId,
            string itemName,
            string description,
            ItemType itemType,
            int maxStack,
            float weight,
            float spoilDuration,
            float durability)
        {
            this.itemId = itemId;
            this.itemName = itemName;
            this.description = description;
            this.itemType = itemType;
            this.maxStack = maxStack;
            this.weight = weight;
            this.spoilDuration = spoilDuration;
            this.durability = durability;
        }
    }

    [MenuItem("Tools/Inventory/Create Batch Items")]
    public static void CreateBatchItems()
    {
        string folderPath = "Assets/Items/NewItems";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            //AssetDatabase.CreateFolder("Assets", "Items");
            AssetDatabase.CreateFolder("Assets/Items", "NewItems");
        }

        ItemCreationData[] items =
        {
            new ItemCreationData(0,"Wood","Wooow so great, Wood",ItemType.Resource,100,1,-1,-1),
            new ItemCreationData(1,"Stone","Wooow so great, Stone",ItemType.Resource,100,1,-1,-1),
            new ItemCreationData(2,"Thatch","Wooow so great, Thatch",ItemType.Resource,100,0.1f,-1,-1),
            new ItemCreationData(3,"Fiber","Wooow so great, Fiber",ItemType.Resource,100,0.01f,-1,-1),
            new ItemCreationData(4,"Flint","Wooow so great, Flint",ItemType.Resource,100,1,-1,-1),
            new ItemCreationData(5,"Crystal","Wooow so great, Crystal",ItemType.Resource,100,2,-1,-1),
            new ItemCreationData(6,"Obsidian","Wooow so great, Obsidian",ItemType.Resource,100,2,-1,-1),
            new ItemCreationData(7,"Metal","Wooow so great, Metal",ItemType.Resource,100,1,-1,-1),
            new ItemCreationData(8,"Metal Ingot","Wooow so great, Metal Ingot",ItemType.Resource,100,2,-1,-1),
            new ItemCreationData(9,"Cementing Paste","Wooow so great, Cementing Paste",ItemType.Resource,100,0.01f,-1,-1),
            new ItemCreationData(10,"Chitin","Wooow so great, Chitin",ItemType.Resource,100,0.01f,-1,-1),
            new ItemCreationData(11,"Keratin","Wooow so great, Keratin",ItemType.Resource,100,0.01f,-1,-1),
            new ItemCreationData(12,"Stone Pickaxe","Wooow so great, Stone Pickaxe",ItemType.Tool,1,1,-1,100),
            new ItemCreationData(13,"Stone Hatchet","Wooow so great, Stone Hatchet",ItemType.Tool,1,1,-1,100),
            new ItemCreationData(14,"Campfire","Wooow so great, Campfire",ItemType.Structure,10,5,-1,-1),
            new ItemCreationData(15,"Small Box","Wooow so great, Small Box",ItemType.Structure,10,5,-1,-1),
            new ItemCreationData(16,"Wooden Storage","Wooow so great, Wooden Storage",ItemType.Structure,10,10,-1,-1),
            new ItemCreationData(17,"Raw Meat","Wooow so great, Raw Meat",ItemType.Consumable,100,0.01f,120,-1),
            new ItemCreationData(18,"Cooked Meat","Wooow so great, Cooked Meat",ItemType.Consumable,100,0.01f,240,-1),
            new ItemCreationData(19,"Spoiled Meat","Wooow so great, Spoiled Meat",ItemType.Consumable,100,0.01f,120,-1),
            new ItemCreationData(20,"Hide","Wooow so great, Hide",ItemType.Resource,100,0.01f,-1,-1),
            new ItemCreationData(21,"Bow","Wooow so great, Bow",ItemType.Weapon,1,5,-1,-1),
            new ItemCreationData(22,"Arrow","Wooow so great, Arrow",ItemType.Ammo,100,0.01f,-1,-1),
            new ItemCreationData(23,"Berries","Wooow so great, Berries",ItemType.Consumable,100,0.01f,120,-1),
            new ItemCreationData(24,"Hide Chestpiece","Wooow so great, Hide Chestpiece",ItemType.Equipment,1,1,-1,100),
            new ItemCreationData(25,"Hide Leggins","Wooow so great, Hide Leggins",ItemType.Equipment,1,1,-1,100),
            new ItemCreationData(26,"Hide Gloves","Wooow so great, Hide Gloves",ItemType.Equipment,1,1,-1,100),
            new ItemCreationData(27,"Hide Boots","Wooow so great, Hide Boots",ItemType.Equipment,1,1,-1,100),
            new ItemCreationData(28,"Health Potion","Wooow so great, Health Potion",ItemType.Consumable,100,0.01f,480,-1),

        };

        foreach (ItemCreationData data in items)
        {
            CreateItemAsset(folderPath, data);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Batch item creation completed.");
    }

    private static void CreateItemAsset(string folderPath, ItemCreationData data)
    {
        string assetPath = $"{folderPath}/{data.itemId}.asset";

        InventoryItemData existing = AssetDatabase.LoadAssetAtPath<InventoryItemData>(assetPath);
        if (existing != null)
        {
            Debug.LogWarning($"Item already exists, skipping: {data.itemId}");
            return;
        }

        InventoryItemData newItem = ScriptableObject.CreateInstance<InventoryItemData>();

        newItem.itemId = data.itemId;
        newItem.itemName = data.itemName;
        newItem.description = data.description;
        newItem.itemType = data.itemType;
        newItem.maxStack = data.maxStack;
        newItem.weight = data.weight;
        newItem.spoilDuration = data.spoilDuration;
        newItem.durability = data.durability;

        // These stay null for now and can be assigned later in the Inspector
        newItem.itemPrefab = null;
        newItem.icon = null;

        AssetDatabase.CreateAsset(newItem, assetPath);
        EditorUtility.SetDirty(newItem);
    }
}