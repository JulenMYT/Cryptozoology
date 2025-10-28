using UnityEngine;
using UnityEngine.InputSystem;

public static class CursorMover
{
    private static float cursorSpeed = 1000f;

    public static void MoveCursor(Vector2 direction)
    {
        if (Mouse.current == null) return;

        Vector2 currentPos = Mouse.current.position.ReadValue();
        Vector2 newPos = currentPos + direction * cursorSpeed * Time.deltaTime;

        newPos.x = Mathf.Clamp(newPos.x, 0, Screen.width);
        newPos.y = Mathf.Clamp(newPos.y, 0, Screen.height);

        Mouse.current.WarpCursorPosition(newPos);
        Mouse.current.MakeCurrent();
    }

    public static void SetCursorPosition(Vector2 screenPosition)
    {
        if (Mouse.current == null) return;

        Vector2 pos = screenPosition;
        pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
        pos.y = Mathf.Clamp(pos.y, 0, Screen.height);

        Mouse.current.WarpCursorPosition(pos);
        Mouse.current.MakeCurrent();
    }

    public static Vector2 GetCursorPosition()
    {
        if (Mouse.current == null) return Vector2.zero;
        return Mouse.current.position.ReadValue();
    }
}
