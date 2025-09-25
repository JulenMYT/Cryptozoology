using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemSlotButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private ItemData itemData;

    public Action<ItemData> OnClickItem;

    public void Initialize(ItemData item)
    {
        itemData = item;
        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        OnClickItem?.Invoke(itemData);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }
}
