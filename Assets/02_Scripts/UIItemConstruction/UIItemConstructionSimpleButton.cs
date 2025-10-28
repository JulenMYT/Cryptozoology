using System;
using UnityEngine;
using UnityEngine.UI;

public class UIItemConstructionSimpleButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;
    [SerializeField] private Image selectedImage;

    private ObjectData itemData;

    public event Action<ObjectData> OnItemClicked;

    private void Awake()
    {
        SetSelected(false);
    }

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

    public void SetSelected(bool selected)
    {
        if (selectedImage != null)
        {
            selectedImage.enabled = selected;
        }
    }

    public void ClickButton()
    {
        button.onClick.Invoke();
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }
}
