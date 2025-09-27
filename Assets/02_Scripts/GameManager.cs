using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Garden")]
    [SerializeField] private GardenState gardenState;
    public GardenState Garden => gardenState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (gardenState == null)
            Debug.LogError("GameManager: GardenState reference is missing!");
    }
}
