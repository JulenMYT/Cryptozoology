using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHouse : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button feedButton;

    public event Action OnFeedButtonClicked;

    private void Awake()
    {
        feedButton.onClick.AddListener(() => OnFeedButtonClicked?.Invoke());
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
