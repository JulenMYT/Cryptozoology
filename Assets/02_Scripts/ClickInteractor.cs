using UnityEngine;

public interface IClickable
{
    void OnClick();
    void OnCancel();
}

public class ClickInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    private IClickable currentClickable;

    private void Start()
    {
        GameManager.Instance.GameModeManager.OnModeChanged += HandleModeChanged;
        HandleModeChanged(GameManager.Instance.GameModeManager.CurrentMode);
    }

    private void OnDisable()
    {
        GameManager.Instance.GameModeManager.OnModeChanged -= HandleModeChanged;
        UnsubscribeInput();
    }

    private void HandleModeChanged(GameMode mode)
    {
        if (mode == GameMode.Normal)
            SubscribeInput();
        else
        {
            CancelCurrentClickable();
            UnsubscribeInput();
        }
    }

    private void SubscribeInput()
    {
        GameManager.Instance.Input.OnLeftClick -= HandleLeftClick;
        GameManager.Instance.Input.OnRightClick -= HandleRightClick;
        GameManager.Instance.Input.OnLeftClick += HandleLeftClick;
        GameManager.Instance.Input.OnRightClick += HandleRightClick;
    }

    private void UnsubscribeInput()
    {
        GameManager.Instance.Input.OnLeftClick -= HandleLeftClick;
        GameManager.Instance.Input.OnRightClick -= HandleRightClick;
    }

    private void HandleLeftClick()
    {
        if (GameManager.Instance.Input.IsPointedOverUI()) return;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 100f, interactableLayer))
        {
            if (hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                CancelCurrentClickable();
                clickable.OnClick();
                currentClickable = clickable;
            }
        }
    }

    private void HandleRightClick()
    {
        if (GameManager.Instance.Input.IsPointedOverUI()) return;

        CancelCurrentClickable();
    }

    private void CancelCurrentClickable()
    {
        if (currentClickable != null)
        {
            currentClickable.OnCancel();
            currentClickable = null;
        }
    }
}
