using System;
using UnityEngine;
using UnityEngine.UI;

public class UIItemConstructionSimpleButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private ObjectData itemData;

    public event Action<ObjectData> OnItemClicked;
    public void Initialize(ObjectData item)
    {
        itemData = item;
        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        OnItemClicked?.Invoke(itemData);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }
}
