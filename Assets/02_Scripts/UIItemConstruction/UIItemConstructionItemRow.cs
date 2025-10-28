using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIItemConstructionItemRow : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Transform buttonsParent;
    [SerializeField] private UIItemConstructionSimpleButton itemButtonPrefab;
    [SerializeField] private float movementAmount = 105f;
    [SerializeField] private int buttonsPerPage = 6;

    private readonly List<UIItemConstructionSimpleButton> buttonOrder = new();
    private readonly Dictionary<string, UIItemConstructionSimpleButton> itemButtons = new();

    private int currentVisibleIndex = 0;
    private int currentSelectedIndex = -1;

    private UIItemConstructionSimpleButton currentSelected;
    public bool IsActiveRow { get; private set; }

    public void Initialize(string itemName)
    {
        itemNameText.text = itemName;
    }

    public UIItemConstructionSimpleButton AddButton(ObjectData data)
    {
        if (itemButtons.ContainsKey(data.displayName))
            return itemButtons[data.displayName];

        var newButton = Instantiate(itemButtonPrefab, buttonsParent);
        newButton.Initialize(data);
        newButton.OnItemClicked += OnButtonClicked;

        itemButtons[data.displayName] = newButton;
        buttonOrder.Add(newButton);
        return newButton;
    }

    private void OnButtonClicked(ObjectData data)
    {
        if (!IsActiveRow) return;
        var button = itemButtons[data.displayName];
        SelectButton(button);
    }

    public void Clear()
    {
        foreach (var button in buttonOrder)
        {
            button.OnItemClicked -= OnButtonClicked;
            Destroy(button.gameObject);
        }
        itemButtons.Clear();
        buttonOrder.Clear();
        currentVisibleIndex = 0;
        currentSelectedIndex = -1;
        currentSelected = null;
    }


    public void ActivateRow(bool active)
    {
        IsActiveRow = active;
        if (!active) DeselectButton();
        else if (buttonOrder.Count > 0)
            SelectButton(buttonOrder[0]);
    }

    public void SelectButton(UIItemConstructionSimpleButton button)
    {
        if (currentSelected == button) return;

        if (currentSelected != null)
            currentSelected.SetSelected(false);

        currentSelected = button;
        currentSelectedIndex = buttonOrder.IndexOf(button);

        if (currentSelected != null)
            currentSelected.SetSelected(true);
    }

    public void DeselectButton()
    {
        if (currentSelected == null) return;
        currentSelected.SetSelected(false);
        currentSelected = null;
        currentSelectedIndex = -1;
    }

    public bool CanRowGoLeft() => currentVisibleIndex > 0;
    public bool CanRowGoRight() => currentVisibleIndex + buttonsPerPage < buttonOrder.Count;

    public void RowGoLeft()
    {
        if (!CanRowGoLeft()) return;
        currentVisibleIndex--;
        buttonsParent.DOLocalMoveX(buttonsParent.localPosition.x + movementAmount, 0.25f).SetEase(Ease.OutCubic);
    }

    public void RowGoRight()
    {
        if (!CanRowGoRight()) return;
        currentVisibleIndex++;
        buttonsParent.DOLocalMoveX(buttonsParent.localPosition.x - movementAmount, 0.25f).SetEase(Ease.OutCubic);
    }

    public void SelectLeft()
    {
        if (currentSelectedIndex <= 0) return;
        int nextIndex = currentSelectedIndex - 1;
        if (nextIndex < currentVisibleIndex)
            RowGoLeft();
        SelectButton(buttonOrder[nextIndex]);
    }


    public void SelectRight()
    {
        if (currentSelectedIndex < 0 || currentSelectedIndex >= buttonOrder.Count - 1) return;
        int nextIndex = currentSelectedIndex + 1;
        if (nextIndex >= (currentVisibleIndex + buttonsPerPage))
            RowGoRight();
        SelectButton(buttonOrder[nextIndex]);
    }


    public void ResetRowPlacement()
    {
        currentVisibleIndex = 0;
        buttonsParent.localPosition = Vector3.zero;
    }

    public void ClickCurrentButton()
    {
        if (currentSelected != null)
            currentSelected.ClickButton();
    }
}
