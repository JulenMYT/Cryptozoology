using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;



public class UIItemConstruction : MonoBehaviour
{
    private enum MenuState
    {
        Closed,
        Open
    }

    private MenuState state;

    [Header("Menu Tween")]
    [SerializeField] private UIMenuSlideVertical menuTween;

    [Header("Category Buttons")]
    [SerializeField] private RectTransform layoutGroupParent;
    [SerializeField] private List<ButtonCategory> categoryButtons;
    private Dictionary<ItemCategory, Button> categoryButtonDict = new();
    [SerializeField] private float scaleFactor = 1.7f;
    private Button currentButton;

    [Serializable]
    private struct ButtonCategory
    {
        public Button button;
        public ItemCategory category;
    }

    [Header("Item Rows")]
    [SerializeField] private UIItemConstructionItemRow itemRowPrefab;
    [SerializeField] private RectTransform itemRowParent;
    [SerializeField] private float rowMovementAmount = 150f;
    private Dictionary<ItemSubCategory, UIItemConstructionItemRow> itemRowDict = new();
    private List<UIItemConstructionItemRow> itemRows = new();
    private UIItemConstructionItemRow currentItemRow;
    private int currentRowIndex = 0;

    [Header("Input Handling")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private static readonly Dictionary<ItemSubCategory, string> subcategoryNames = new Dictionary<ItemSubCategory, string>
    {
        { ItemSubCategory.PlantVegetable, "Potager" },
        { ItemSubCategory.PlantTree, "Arbres" },
        { ItemSubCategory.PlantFlower, "Fleurs" },
        { ItemSubCategory.AnimalExerior, "Extérieur" },
        { ItemSubCategory.AnimalHouse, "Abris" },
        { ItemSubCategory.None, "Divers"}
    };

    private void Awake()
    {
        foreach (var buttonCategory in categoryButtons)
        {
            categoryButtonDict.Add(buttonCategory.category, buttonCategory.button);
            buttonCategory.button.onClick.AddListener(() => OnCategoryButtonClicked(buttonCategory.category));
        }

        if (leftButton != null)
            leftButton.onClick.AddListener(HandleLeftClick);
        if (rightButton != null)
            rightButton.onClick.AddListener(HandleRightClick);
        if (upButton != null)
            upButton.onClick.AddListener(HandleUpClick);
        if (downButton != null)
            downButton.onClick.AddListener(HandleDownClick);
    }

    private void OnCategoryButtonClicked(ItemCategory category)
    {
        Button buttonClicked = categoryButtonDict[category];

        if (currentButton != null)
        {
            currentButton.transform.DOScale(Vector3.one, 0.15f)
                .SetEase(Ease.InOutBack)
                .OnUpdate(() => LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupParent));
        }

        if (buttonClicked == currentButton)
        {
            CloseMenu();
            currentButton = null;
            return;
        }

        currentButton = buttonClicked;
        buttonClicked.transform.DOScale(Vector3.one * scaleFactor, 0.2f)
            .SetEase(Ease.OutBack)
            .OnUpdate(() => LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupParent));

        DisplayCategory(category);

        OpenMenu();
    }

    private void DisplayCategory(ItemCategory category)
    {
        Clear();
        var items = ItemDatabaseRuntime.GetByCategory(category);

        foreach (var item in items)
        {
            if (!itemRowDict.TryGetValue(item.subCategory, out var itemRow))
            {
                itemRow = Instantiate(itemRowPrefab, itemRowParent);
                itemRow.Initialize(subcategoryNames[item.subCategory]);
                itemRowDict.Add(item.subCategory, itemRow);
                itemRows.Add(itemRow);
            }
            UIItemConstructionSimpleButton button = itemRow.AddButton(item);
            button.OnItemClicked += HandleItemSelected;
        }
        if (itemRows.Count > 0)
        {
            currentItemRow = itemRows[0];
            currentRowIndex = 0;
        }
        UpdateNavigationButtons();
    }

    private void HandleItemSelected(ObjectData item)
    {
        if (GameManager.Instance.Garden.GetCount(item.displayName) >= 1 && item.isUnique)
        {
            Debug.Log("Max count reached for item: " + item.name);
            return;
        }
        GameManager.Instance.BuildingSystem.SetPlacementMode(item);
    }

    private void Clear()
    {
        foreach (Transform child in itemRowParent)
        {
            Destroy(child.gameObject);
        }
        itemRowDict.Clear();
        itemRows.Clear();
        currentItemRow = null;
        currentRowIndex = 0;
    }


    private bool CanGoUp()
    {
        if (currentRowIndex <= 0)
        {
            return false;
        }
        return true;
    }

    private bool CanGoDown()
    {
        if (currentRowIndex >= itemRows.Count - 1)
        {
            return false;
        }
        return true;
    }

    private void GoUp()
    {
        if (!CanGoUp()) return;
        currentRowIndex--;
        currentItemRow = itemRows[currentRowIndex];
        Vector2 targetPos = itemRowParent.anchoredPosition - new Vector2(0, rowMovementAmount);
        itemRowParent.DOAnchorPos(targetPos, 0.25f).SetEase(Ease.OutQuad);
    }

    private void GoDown()
    {
        if (!CanGoDown()) return;
        currentRowIndex++;
        currentItemRow = itemRows[currentRowIndex];
        Vector2 targetPos = itemRowParent.anchoredPosition + new Vector2(0, rowMovementAmount);
        itemRowParent.DOAnchorPos(targetPos, 0.25f).SetEase(Ease.OutQuad);
    }

    private void HandleLeftClick()
    {
        currentItemRow.GoLeft();
        UpdateNavigationButtons();
    }

    private void HandleRightClick()
    {
        currentItemRow.GoRight();
        UpdateNavigationButtons();
    }

    private void HandleUpClick()
    {
        GoUp();
        UpdateNavigationButtons();
    }

    private void HandleDownClick()
    {
        GoDown();
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        if (leftButton != null)
            leftButton.interactable = currentItemRow != null && currentItemRow.CanGoLeft();
        if (rightButton != null)
            rightButton.interactable = currentItemRow != null && currentItemRow.CanGoRight();
        if (upButton != null)
        {
            upButton.gameObject.SetActive(CanGoUp());
        }
        if (downButton != null)
        {
            downButton.gameObject.SetActive(CanGoDown());
        }
    }

    public void OpenMenu()
    {
        if (state == MenuState.Open) return;
        state = MenuState.Open;
        menuTween.OpenMenu();
    }

    public void CloseMenu()
    {
        if (state == MenuState.Closed) return;
        state = MenuState.Closed;
        menuTween.CloseMenu();
    }
}
