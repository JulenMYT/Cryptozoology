using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemTypeButton : MonoBehaviour
{
    private ItemCategory itemCategory;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    public Action<ItemCategory> OnButtonClick;

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    public void Initialize(ItemCategory category, Sprite sprite)
    {
        itemCategory = category;

        if (iconImage != null)
            iconImage.sprite = sprite;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        OnButtonClick?.Invoke(itemCategory);
    }
}
