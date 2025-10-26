using UnityEngine;
using DG.Tweening;

public class UIMenuSlideVertical : MonoBehaviour
{
    [SerializeField] private RectTransform menuTransform;
    [SerializeField] private float slideDistance = 800f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease easeIn = Ease.InBack;
    [SerializeField] private Ease easeOut = Ease.OutBack;

    private Vector2 shownPos;
    private Vector2 hiddenPos;
    private bool isOpen;

    private void Start()
    {
        shownPos = menuTransform.anchoredPosition;
        hiddenPos = shownPos - new Vector2(0f, slideDistance);
        menuTransform.anchoredPosition = hiddenPos;
    }

    public void OpenMenu()
    {
        if (isOpen) return;
        isOpen = true;

        menuTransform.DOAnchorPos(shownPos, duration)
            .SetEase(easeOut);
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;

        menuTransform.DOAnchorPos(hiddenPos, duration)
            .SetEase(easeIn);
    }
}
