using System;
using UnityEngine;
using UnityEngine.UI;

public class UIEncyclopediaAnimalButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;

    private string animalId;
    public event Action<string> OnButtonClicked;

    public void Setup(string id, Sprite sprite)
    {
        animalId = id;
        iconImage.sprite = sprite;
    }

    public void UpdateIconImage(Sprite sprite)
    {
        iconImage.sprite = sprite;
    }

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleButtonClick);
        }
    }

    private void HandleButtonClick()
    {
        OnButtonClicked?.Invoke(animalId);
    }
}
