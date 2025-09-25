using System.Collections.Generic;
using UnityEngine;

public class ItemUIManager : MonoBehaviour
{
    [SerializeField] private ItemTypeButton buttonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private ItemSlotButton itemSlotPrefab;
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private CategoryData[] categories;

    private Dictionary<ItemCategory, Sprite> categoryIcons;

    private void Awake()
    {
        categoryIcons = new Dictionary<ItemCategory, Sprite>();
        foreach (var cat in categories)
            categoryIcons[cat.category] = cat.icon;

        SetupCategoryButtons();
    }

    private void SetupCategoryButtons()
    {
        foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
        {
            if (!categoryIcons.ContainsKey(category)) continue;

            var button = Instantiate(buttonPrefab, buttonContainer);
            button.Initialize(category, categoryIcons[category]);
            button.OnButtonClick += DisplayItems;
        }
    }

    private void DisplayItems(ItemCategory category)
    {
        ClearItems();

        var items = ItemDatabaseRuntime.GetByCategory(category);

        foreach (var item in items)
        {
            var slot = Instantiate(itemSlotPrefab, itemListContainer);
            slot.Initialize(item);
        }
    }

    private void ClearItems()
    {
        foreach (Transform child in itemListContainer)
            Destroy(child.gameObject);
    }
}
