using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    [Header("Prefabs & Containers")]
    [SerializeField] private ItemTypeButton buttonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private ItemSlotButton itemSlotPrefab;
    [SerializeField] private Transform itemListContainer;

    [SerializeField] private GridPlacementManager gridPlacementManager;

    [Header("Category Data")]
    [SerializeField] private CategoryData[] categories;

    private Dictionary<ItemCategory, Sprite> categoryIcons;

    private void Awake()
    {
        categoryIcons = new Dictionary<ItemCategory, Sprite>();
        foreach (var cat in categories)
        {
            categoryIcons[cat.category] = cat.icon;
        }

        SetupCategoryButtons();
    }

    private void SetupCategoryButtons()
    {
        foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
        {
            if (!categoryIcons.ContainsKey(category))
            {
                Debug.LogWarning($"No icon set for category {category}");
                continue;
            }

            var buttonObj = Instantiate(buttonPrefab, buttonContainer);
            buttonObj.Initialize(category, categoryIcons[category]);
            buttonObj.OnButtonClick += DisplayItems;
        }
    }

    private void DisplayItems(ItemCategory category)
    {
        ClearItems();

        List<ItemData> items = ItemDatabaseRuntime.GetByCategory(category);

        foreach (var item in items)
        {
            ItemSlotButton slotButton = Instantiate(itemSlotPrefab, itemListContainer);
            if (slotButton != null)
            {
                slotButton.Initialize(item);
                slotButton.OnClickItem += PlaceItemInScene;
            }
        }
    }

    private void ClearItems()
    {
        foreach (Transform child in itemListContainer)
        {
            var slotButton = child.GetComponent<ItemSlotButton>();
            if (slotButton != null)
                slotButton.OnClickItem -= PlaceItemInScene;

            Destroy(child.gameObject);
        }
    }

    private void PlaceItemInScene(ItemData item)
    {
        gridPlacementManager.SelectItem(item);
    }
}
