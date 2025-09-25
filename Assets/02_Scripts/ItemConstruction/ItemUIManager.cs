using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    [SerializeField] private ItemTypeButton buttonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private ItemSlotButton itemSlotPrefab;
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private CategoryData[] categories;
    [SerializeField] private Button removeModeButton;

    private Dictionary<ItemCategory, Sprite> categoryIcons;
    public event Action<ItemData> OnItemSelected;
    public event Action OnRemoveModeSelected;

    private void Awake()
    {
        categoryIcons = new Dictionary<ItemCategory, Sprite>();
        foreach (var cat in categories)
            categoryIcons[cat.category] = cat.icon;

        SetupCategoryButtons();
        if (removeModeButton != null)
            removeModeButton.onClick.AddListener(() => OnRemoveModeSelected?.Invoke());
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
            slot.OnItemClicked += HandleItemSelected;
        }
    }

    private void ClearItems()
    {
        foreach (Transform child in itemListContainer)
        {
            if (child.TryGetComponent<ItemSlotButton>(out var slot))
            {
                slot.OnItemClicked -= HandleItemSelected;
            }
            Destroy(child.gameObject);
        }
    }

    private void HandleItemSelected(ItemData item)
    {
        OnItemSelected?.Invoke(item);
    }
}
