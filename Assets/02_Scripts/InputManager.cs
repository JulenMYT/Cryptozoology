using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public event System.Action OnLeftClick, OnRightClick;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnLeftClick?.Invoke();

        if (Input.GetMouseButtonDown(1))
            OnRightClick?.Invoke();
    }

    public bool IsPointedOverUI()
    => EventSystem.current.IsPointerOverGameObject();
}
