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

    [Header("Input")]
    [SerializeField] private InputManager inputManager;
    public InputManager Input => inputManager;

    [Header("Encyclopedia")]
    [SerializeField] private EncyclopediaManager encyclopediaManager;
    public EncyclopediaManager Encyclopedia => encyclopediaManager;

    [Header("Day/Night Cycle")]
    [SerializeField] private DayNightCycleManager dayNightCycle;
    public DayNightCycleManager DayNight => dayNightCycle;

    [Header("Game Mode")]
    [SerializeField] private GameModeManager gameModeManager;
    public GameModeManager GameModeManager => gameModeManager;

    [Header("Animals")]
    [SerializeField] private AnimalManager animalManager;
    public AnimalManager Animals => animalManager;

    [Header("Building System")]
    [SerializeField] private BuildingSystem buildingSystem;     
    public BuildingSystem BuildingSystem => buildingSystem;

    [Header("Garden")]
    [SerializeField] private GardenState gardenState;
    public GardenState Garden => gardenState;

    [Header("Spawner")]
    [SerializeField] private AnimalSpawner animalSpawner;
    public AnimalSpawner AnimalSpawner => animalSpawner;

    [Header("Money")]   
    [SerializeField] private MoneyManager moneyManager;
    public MoneyManager Money => moneyManager;
}
