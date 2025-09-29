using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlantVisual : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public IEnumerator SetColorFor(Color color, float duration)
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = color;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = originalColor;
    }
}
