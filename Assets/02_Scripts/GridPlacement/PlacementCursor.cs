using UnityEngine;
using UnityEngine.UI;

public class UICursor : MonoBehaviour
{
    [SerializeField] private RectTransform cursor;
    [SerializeField] private float speed = 1000f;
    [SerializeField] private Canvas canvas;
    [SerializeField] private LayerMask raycastLayer;

    private Vector2 currentPos;
    private bool visible = true;

    private void Awake()
    {
        if (cursor == null)
            cursor = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        currentPos = cursor.anchoredPosition;
    }

    public void Move(Vector2 direction)
    {
        if (!visible) return;

        currentPos += direction * speed * Time.unscaledDeltaTime;

        float maxX = Screen.width;
        float maxY = Screen.height;
        currentPos.x = Mathf.Clamp(currentPos.x, 0, maxX);
        currentPos.y = Mathf.Clamp(currentPos.y, 0, maxY);

        cursor.anchoredPosition = currentPos;
    }

    public void Warp(Vector2 screenPosition)
    {
        currentPos = new Vector2(
            Mathf.Clamp(screenPosition.x, 0, Screen.width),
            Mathf.Clamp(screenPosition.y, 0, Screen.height)
        );
        cursor.anchoredPosition = currentPos;
    }

    public Vector2 GetScreenPosition()
    {
        return currentPos;
    }

    public bool RaycastFromCursor(out RaycastHit hit, float distance = 100f)
    {
        Vector2 screenPos = GetScreenPosition();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out hit, distance, raycastLayer);
    }

    public void Show()
    {
        visible = true;
        cursor.gameObject.SetActive(true);
    }

    public void Hide()
    {
        visible = false;
        cursor.gameObject.SetActive(false);
    }
}
