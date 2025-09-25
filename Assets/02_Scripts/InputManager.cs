using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("InputManager");
                instance = go.AddComponent<InputManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private InputManager() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeScene()
    {
        _ = Instance;
    }

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
