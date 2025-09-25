using UnityEngine;

public class PlacementCursor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void SetActive(bool active) => gameObject.SetActive(active);
    public bool IsActive() => gameObject.activeSelf;

    public void SetSize(Vector2Int gridSize)
    {
        spriteRenderer.size = new Vector3(gridSize.x, 1f, gridSize.y);
    }

    public void UpdatePosition(Vector3 position)
    {
        transform.position = position;
    }

    public void UpdateColor(bool canPlace)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = canPlace ? Color.white : Color.red;
    }
}
