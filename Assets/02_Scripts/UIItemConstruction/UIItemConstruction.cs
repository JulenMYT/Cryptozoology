using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIItemConstruction : MonoBehaviour
{
    private enum MenuState { Closed, Open, Building }
    private MenuState state;

    [Header("Menu Tween")]
    [SerializeField] private UIMenuSlideVertical menuTween;

    [Header("Category Buttons")]
    [SerializeField] private RectTransform layoutGroupParent;
    [SerializeField] private List<ButtonCategory> categoryButtons;
    [SerializeField] private float scaleFactor = 1.7f;
    private Button currentButton;
    private int currentButtonIndex;

    [Serializable]
    private struct ButtonCategory
    {
        public Button button;
        public ItemCategory category;
        public int index;
    }

    [Header("Item Rows")]
    [SerializeField] private UIItemConstructionItemRow itemRowPrefab;
    [SerializeField] private RectTransform itemRowParent;
    [SerializeField] private float rowMovementAmount = 150f;

    private readonly Dictionary<ItemSubCategory, UIItemConstructionItemRow> itemRowDict = new();
    private readonly List<UIItemConstructionItemRow> itemRows = new();
    private UIItemConstructionItemRow currentItemRow;
    private int currentRowIndex = 0;

    [Header("Input Handling")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private static readonly Dictionary<ItemSubCategory, string> subcategoryNames = new()
    {
        { ItemSubCategory.PlantVegetable, "Potager" },
        { ItemSubCategory.PlantTree, "Arbres" },
        { ItemSubCategory.PlantFlower, "Fleurs" },
        { ItemSubCategory.AnimalExerior, "Extérieur" },
        { ItemSubCategory.AnimalHouse, "Abris" },
        { ItemSubCategory.None, "Divers"}
    };

    private float cursorSpeed = 500f;

    private void Awake()
    {
        foreach (var c in categoryButtons)
        {
            var localCategory = c;
            localCategory.button.onClick.AddListener(() => OnCategoryButtonClicked(localCategory));
        }

        leftButton?.onClick.AddListener(() => { currentItemRow.RowGoLeft(); UpdateNavigationButtons(); });
        rightButton?.onClick.AddListener(() => { currentItemRow.RowGoRight(); UpdateNavigationButtons(); });
        upButton?.onClick.AddListener(() => { GoUp(); UpdateNavigationButtons(); });
        downButton?.onClick.AddListener(() => { GoDown(); UpdateNavigationButtons(); });
    }

    private void Start()
    {
        SubscribeInputs();
    }

    private void SubscribeInputs()
    {
        var gm = GameManager.Instance;
        gm.BuildingSystem.OnCanceled += HandleCancel;

        gm.Input.OnLeftBumper += HandleLeftBumper;
        gm.Input.OnRightBumper += HandleRightBumper;
        gm.Input.OnAButton += HandleAButton;
        gm.Input.OnBButton += HandleBButton;

        gm.Input.OnUpButton += () => HandleDirectional(Vector2.up);
        gm.Input.OnDownButton += () => HandleDirectional(Vector2.down);
        gm.Input.OnLeftButton += () => HandleDirectional(Vector2.left);
        gm.Input.OnRightButton += () => HandleDirectional(Vector2.right);

        gm.Input.OnUpButtonHeld += () => HandleDirectional(Vector2.up, true);
        gm.Input.OnDownButtonHeld += () => HandleDirectional(Vector2.down, true);
        gm.Input.OnLeftButtonHeld += () => HandleDirectional(Vector2.left, true);
        gm.Input.OnRightButtonHeld += () => HandleDirectional(Vector2.right, true);

        gm.Input.OnLeftClick += HandleLeftClick;
        gm.Input.OnRightClick += HandleRightClick;
    }

    private void HandleDirectional(Vector2 dir, bool held = false)
    {
        var gm = GameManager.Instance;
        if (gm.BuildingSystem.CurrentMode == BuildingMode.Placement)
        {
            float speed = cursorSpeed * (held ? 1.5f : 1f);
            CursorMover.MoveCursor(dir * speed * Time.deltaTime);
        }
        else if (state == MenuState.Open)
        {
            if (dir == Vector2.up) GoUp();
            else if (dir == Vector2.down) GoDown();
            else if (dir == Vector2.left) currentItemRow.SelectLeft();
            else if (dir == Vector2.right) currentItemRow.SelectRight();

            UpdateNavigationButtons();
        }
    }

    private void HandleRightBumper()
    {
        if (state == MenuState.Building) return;
        if (state == MenuState.Closed) { OnCategoryButtonClicked(categoryButtons[0]); return; }
        int next = (currentButtonIndex + 1) % categoryButtons.Count;
        OnCategoryButtonClicked(categoryButtons[next]);
    }

    private void HandleLeftBumper()
    {
        if (state == MenuState.Building) return;
        if (state == MenuState.Closed) { OnCategoryButtonClicked(categoryButtons[0]); return; }
        int prev = (currentButtonIndex - 1 + categoryButtons.Count) % categoryButtons.Count;
        OnCategoryButtonClicked(categoryButtons[prev]);
    }

    private void HandleAButton()
    {
        if (state == MenuState.Open)
            currentItemRow?.ClickCurrentButton();
        else if (state == MenuState.Building)
            GameManager.Instance.BuildingSystem.TryPlaceAtCursor();
    }

    private void HandleBButton()
    {
        if (state == MenuState.Open) { CloseMenu(); ResetCategoryVisual(); }
        else if (state == MenuState.Building) GameManager.Instance.BuildingSystem.CancelPlacement();
    }

    private void HandleLeftClick()
    {
        if (state == MenuState.Building) GameManager.Instance.BuildingSystem.TryPlaceAtCursor();
    }

    private void HandleRightClick()
    {
        if (state == MenuState.Building) GameManager.Instance.BuildingSystem.CancelPlacement();
    }

    private void HandleCancel()
    {
        if (state == MenuState.Building) state = MenuState.Open;
    }

    // --- Menu Methods ---
    private void OnCategoryButtonClicked(ButtonCategory category)
    {
        if (currentButton == category.button) { CloseMenu(); ResetCategoryVisual(); return; }

        ResetCategoryVisual();
        currentButton = category.button;
        currentButtonIndex = category.index;

        currentButton.transform.DOScale(Vector3.one * scaleFactor, 0.25f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupParent));

        DisplayCategory(category.category);
        OpenMenu();
    }

    private void ResetCategoryVisual()
    {
        if (currentButton == null) return;
        currentButton.transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.InOutBack)
            .OnComplete(() => LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupParent));
        currentButton = null;
    }

    private void DisplayCategory(ItemCategory category)
    {
        Clear();
        var items = ItemDatabaseRuntime.GetByCategory(category);

        foreach (var item in items)
        {
            if (!itemRowDict.TryGetValue(item.subCategory, out var row))
            {
                row = Instantiate(itemRowPrefab, itemRowParent);
                row.Initialize(subcategoryNames[item.subCategory]);
                itemRowDict[item.subCategory] = row;
                itemRows.Add(row);
            }
            var button = row.AddButton(item);
            button.OnItemClicked += HandleItemSelected;
        }

        if (itemRows.Count > 0)
        {
            currentRowIndex = 0;
            currentItemRow = itemRows[0];
            currentItemRow.ActivateRow(true);
        }

        UpdateNavigationButtons();
    }

    private void HandleItemSelected(ObjectData item)
    {
        if (item.isUnique && GameManager.Instance.Garden.GetCount(item.displayName) >= 1) return;

        state = MenuState.Building;
        GameManager.Instance.BuildingSystem.SetPlacementMode(item);
    }

    private void Clear()
    {
        foreach (var row in itemRows) Destroy(row.gameObject);
        itemRowDict.Clear();
        itemRows.Clear();
        currentItemRow = null;
        currentRowIndex = 0;
    }

    private bool CanGoUp() => currentRowIndex > 0;
    private bool CanGoDown() => currentRowIndex < itemRows.Count - 1;

    private void GoUp() { if (!CanGoUp()) return; currentItemRow.DeselectButton(); currentItemRow.ActivateRow(false); currentRowIndex--; currentItemRow = itemRows[currentRowIndex]; currentItemRow.ActivateRow(true); itemRowParent.DOAnchorPos(itemRowParent.anchoredPosition - new Vector2(0, rowMovementAmount), 0.25f).SetEase(Ease.OutQuad); }
    private void GoDown() { if (!CanGoDown()) return; currentItemRow.DeselectButton(); currentItemRow.ActivateRow(false); currentRowIndex++; currentItemRow = itemRows[currentRowIndex]; currentItemRow.ActivateRow(true); itemRowParent.DOAnchorPos(itemRowParent.anchoredPosition + new Vector2(0, rowMovementAmount), 0.25f).SetEase(Ease.OutQuad); }
    private void UpdateNavigationButtons()
    {
        if (currentItemRow == null) return;
        leftButton.interactable = currentItemRow.CanRowGoLeft();
        rightButton.interactable = currentItemRow.CanRowGoRight();
        upButton.gameObject.SetActive(CanGoUp());
        downButton.gameObject.SetActive(CanGoDown());
    }

    public void OpenMenu() { if (state == MenuState.Open) return; state = MenuState.Open; menuTween.OpenMenu(); }
    public void CloseMenu() { if (state == MenuState.Closed) return; state = MenuState.Closed; menuTween.CloseMenu(); Clear(); }
}
