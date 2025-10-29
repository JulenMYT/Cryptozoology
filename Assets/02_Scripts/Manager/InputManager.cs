using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public event Action OnLeftClick, OnRightClick;
    public event Action OnRightBumper, OnLeftBumper;
    public event Action OnAButton, OnBButton, OnXButton, OnYButton;
    public event Action OnUpButton, OnDownButton, OnLeftButton, OnRightButton;
    public event Action OnUpButtonHeld, OnDownButtonHeld, OnLeftButtonHeld, OnRightButtonHeld;

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
            OnLeftClick?.Invoke();
        if (Input.GetMouseButtonUp(1))
            OnRightClick?.Invoke();

        if (Input.GetKeyUp(KeyCode.E))
            OnRightBumper?.Invoke();
        if (Input.GetKeyUp(KeyCode.Q))
            OnLeftBumper?.Invoke();

        if (Input.GetKeyUp(KeyCode.K))
            OnAButton?.Invoke();
        if (Input.GetKeyUp(KeyCode.J))
            OnBButton?.Invoke();
        if (Input.GetKeyUp(KeyCode.I))
            OnXButton?.Invoke();
        if (Input.GetKeyUp(KeyCode.U))
            OnYButton?.Invoke();

        if (Input.GetKeyUp(KeyCode.UpArrow))
            OnUpButton?.Invoke();
        if (Input.GetKeyUp(KeyCode.DownArrow))
            OnDownButton?.Invoke();
        if (Input.GetKeyUp(KeyCode.LeftArrow))
            OnLeftButton?.Invoke();
        if (Input.GetKeyUp(KeyCode.RightArrow))
            OnRightButton?.Invoke();

        if (Input.GetKey(KeyCode.UpArrow))
            OnUpButtonHeld?.Invoke();
        if (Input.GetKey(KeyCode.DownArrow))
            OnDownButtonHeld?.Invoke();
        if (Input.GetKey(KeyCode.LeftArrow))
            OnLeftButtonHeld?.Invoke();
        if (Input.GetKey(KeyCode.RightArrow))
            OnRightButtonHeld?.Invoke();
    }

    public bool IsPointedOverUI()
        => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}
