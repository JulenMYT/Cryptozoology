using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIItemConstruction : MonoBehaviour
{
    private enum MenuState { Closed, Open, Building }
    private MenuState menuState;

    [Header("Menu Tween")]
    [SerializeField] private UIMenuSlideVertical menuTween;

    [Header("General")]
    [SerializeField] private Button toggleButton;

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
    private int currentRowIndex;

    [Header("Input Handling")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private float cursorSpeed = 500f;

    private static readonly Dictionary<ItemSubCategory, string> subcategoryNames = new()
    {
        { ItemSubCategory.PlantVegetable, "Potager" },
        { ItemSubCategory.PlantTree, "Arbres" },
        { ItemSubCategory.PlantFlower, "Fleurs" },
        { ItemSubCategory.AnimalExerior, "Extérieur" },
        { ItemSubCategory.AnimalHouse, "Abris" },
        { ItemSubCategory.None, "Divers"}
    };

    private Action onAButton;
    private Action onBButton;
    private Action onYButton;
    private Action onLeftBumper;
    private Action onRightBumper;
    private Action<Vector2> onDirectional;
    private Action<Vector2> onDirectionalHeld;

    private Action onLeftClick;
    private Action onRightClick;

    private void Awake()
    {
        foreach (var c in categoryButtons)
        {
            var localCategory = c;
            localCategory.button.onClick.AddListener(() => OnCategoryButtonClicked(localCategory));
        }

        leftButton?.onClick.AddListener(() => { currentItemRow.RowGoLeft(); UpdateNavigationButtons(); });
        rightButton?.onClick.AddListener(() => { currentItemRow.RowGoRight(); UpdateNavigationButtons(); });
        upButton?.onClick.AddListener(() => { GoVertical(-1); UpdateNavigationButtons(); });
        downButton?.onClick.AddListener(() => { GoVertical(1); UpdateNavigationButtons(); });

        toggleButton?.onClick.AddListener(ToggleMenu);
    }

    private void Start()
    {
        var input = GameManager.Instance.Input;
        input.OnAButton += () => onAButton?.Invoke();
        input.OnBButton += () => onBButton?.Invoke();
        input.OnYButton += () => onYButton?.Invoke();
        input.OnLeftBumper += () => onLeftBumper?.Invoke();
        input.OnRightBumper += () => onRightBumper?.Invoke();
        input.OnUpButton += () => onDirectional?.Invoke(Vector2.up);
        input.OnDownButton += () => onDirectional?.Invoke(Vector2.down);
        input.OnLeftButton += () => onDirectional?.Invoke(Vector2.left);
        input.OnRightButton += () => onDirectional?.Invoke(Vector2.right);
        input.OnUpButtonHeld += () => onDirectionalHeld?.Invoke(Vector2.up);
        input.OnDownButtonHeld += () => onDirectionalHeld?.Invoke(Vector2.down);
        input.OnLeftButtonHeld += () => onDirectionalHeld?.Invoke(Vector2.left);
        input.OnRightButtonHeld += () => onDirectionalHeld?.Invoke(Vector2.right);
        input.OnLeftClick += () => onLeftClick?.Invoke();
        input.OnRightClick += () => onRightClick?.Invoke();

        SwitchMode(MenuState.Closed);
    }

    private void SwitchMode(MenuState state)
    {
        menuState = state;
        onAButton = onBButton = onYButton = onLeftBumper = onRightBumper = null;
        onDirectional = onDirectionalHeld = null;
        onLeftClick = onRightClick = null;

        switch (menuState)
        {
            case MenuState.Closed: onYButton = OpenMenu; break;
            case MenuState.Open:
                onAButton = ClickCurrentButton;
                onYButton = CloseMenu;
                onLeftBumper = PreviousCategory;
                onRightBumper = NextCategory;
                onDirectional = NavigateMenu;
                break;
            case MenuState.Building:
                onAButton = onLeftClick = TryPlace;
                onBButton = onRightClick = CancelPlacement;
                onYButton = CloseMenu;
                onDirectional = MoveCursor;
                break;
        }
    }

    private void ToggleMenu()
    {
        if (menuState == MenuState.Closed) OpenMenu();
        else
        {
            if (menuState == MenuState.Building) CancelPlacement();
            CloseMenu();
            ResetCategoryVisual(currentButton);
        }
    }

    public void OpenMenu()
    {
        if (menuState == MenuState.Open) return;
        SwitchMode(MenuState.Open);
        menuTween.OpenMenu();
        OnCategoryButtonClicked(categoryButtons[0]);
        GameManager.Instance.Pause.Pause();
    }

    public void CloseMenu()
    {
        if (menuState == MenuState.Closed) return;
        if (menuState == MenuState.Building) CancelPlacement();
        SwitchMode(MenuState.Closed);
        menuTween.CloseMenu();
        ClearMenu();
        ResetCategoryVisual(currentButton);
        currentButton = null;
        GameManager.Instance.Pause.Resume();
    }

    private void OnCategoryButtonClicked(ButtonCategory category)
    {
        if (currentButton == category.button)
        {
            return;
        }

        ResetCategoryVisual(currentButton);
        currentButton = category.button;
        currentButtonIndex = category.index;
        currentButton.transform.DOScale(Vector3.one * scaleFactor, 0.25f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() => LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupParent));
        DisplayCategory(category.category);
    }

    private void ResetCategoryVisual(Button button)
    {
        if (button == null) return;
        button.transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.InOutBack)
            .SetUpdate(true)
            .OnUpdate(() => LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupParent));
    }

    private void PreviousCategory() => ChangeCategory(-1);
    private void NextCategory() => ChangeCategory(1);
    private void ChangeCategory(int offset)
    {
        int index = (currentButtonIndex + offset + categoryButtons.Count) % categoryButtons.Count;
        OnCategoryButtonClicked(categoryButtons[index]);
    }

    private void DisplayCategory(ItemCategory category)
    {
        ClearMenu();
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

    private void ClearMenu()
    {
        foreach (var row in itemRows) Destroy(row.gameObject);
        itemRowDict.Clear();
        itemRows.Clear();
        currentItemRow = null;
        currentRowIndex = 0;
    }

    private void HandleItemSelected(ObjectData item)
    {
        if (item.isUnique && GameManager.Instance.Garden.GetCount(item.displayName) >= 1) return;
        SwitchMode(MenuState.Building);
        GameManager.Instance.BuildingSystem.SetPlacementMode(item);
    }

    private void NavigateMenu(Vector2 dir)
    {
        if (currentItemRow == null) return;
        if (dir == Vector2.up) GoVertical(-1);
        else if (dir == Vector2.down) GoVertical(1);
        else if (dir == Vector2.left) currentItemRow.SelectLeft();
        else if (dir == Vector2.right) currentItemRow.SelectRight();
        UpdateNavigationButtons();
    }

    private void GoVertical(int direction)
    {
        int targetIndex = currentRowIndex + direction;
        if (targetIndex < 0 || targetIndex >= itemRows.Count) return;
        currentItemRow.DeselectButton();
        currentItemRow.ActivateRow(false);
        currentRowIndex = targetIndex;
        currentItemRow = itemRows[currentRowIndex];
        currentItemRow.ActivateRow(true);
        itemRowParent.DOAnchorPos(itemRowParent.anchoredPosition + new Vector2(0, direction * rowMovementAmount), 0.25f).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    private void UpdateNavigationButtons()
    {
        if (currentItemRow == null) return;
        leftButton.interactable = currentItemRow.CanRowGoLeft();
        rightButton.interactable = currentItemRow.CanRowGoRight();
        upButton.gameObject.SetActive(currentRowIndex > 0);
        downButton.gameObject.SetActive(currentRowIndex < itemRows.Count - 1);
    }

    private void MoveCursor(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        CursorMover.MoveCursor(dir * cursorSpeed * Time.unscaledDeltaTime);
    }

    private void TryPlace() => GameManager.Instance.BuildingSystem.TryPlaceAtCursor();
    private void CancelPlacement() => GameManager.Instance.BuildingSystem.CancelPlacement();
    private void ClickCurrentButton() => currentItemRow?.ClickCurrentButton();
}
