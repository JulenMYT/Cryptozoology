using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static string PrefabPath = "GameManagerPrefab";
    private static GameManager instance;
    public static GameManager Instance 
    {
        get
        {
            if (instance == null)
            {
                GameObject go = Instantiate(Resources.Load<GameObject>(PrefabPath));
                instance = go.GetComponent<GameManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private GameManager() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeScene()
    {
        _ = Instance;
    }

    [Header("Garden")]
    [SerializeField] private GardenState gardenState;
    public GardenState Garden => gardenState;

    [Header("Input")]
    [SerializeField] private InputManager inputManager;
    public InputManager Input => inputManager;
<<<<<<< Updated upstream
=======

    [Header("Encyclopedia")]
    [SerializeField] private EncyclopediaManager encyclopediaManager;
    public EncyclopediaManager Encyclopedia => encyclopediaManager;

    [Header("Day/Night Cycle")]
    [SerializeField] private DayNightCycleManager dayNightCycle;
    public DayNightCycleManager DayNight => dayNightCycle;

>>>>>>> Stashed changes
}
