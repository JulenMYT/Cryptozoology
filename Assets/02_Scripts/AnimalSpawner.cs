using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AnimalSpawner : MonoBehaviour
{
    private List<AnimalDataSO> animals = new();
    private static readonly string DatabasePath = "ItemDatabase";
    private ItemDatabase database;

    [SerializeField] private List<Transform> spawnPoints = new();
    [SerializeField] private WaypointPath waypointPath;
    [SerializeField] private Transform animalsParent;
    [SerializeField] private float spawnInterval = 10f;

    private float timer;

    private void Awake()
    {
        database = Resources.Load<ItemDatabase>(DatabasePath);
        if (database != null)
        {
            animals = database.GetItemsByCategory(ItemCategory.Animal)
                .Select(item => item as AnimalDataSO)
                .Where(animal => animal != null)
                .ToList();
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnRandomAnimal();
        }
    }

    private void TrySpawnRandomAnimal()
    {
        Debug.Log("Attempting to spawn an animal...");
        if (animals.Count == 0 || spawnPoints.Count == 0) return;

        AnimalDataSO animalData = animals[Random.Range(0, animals.Count)];
        AnimalConditions conditions = animalData.conditions;

        if (conditions != null && !conditions.CanAppear()) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        if (animalData.prefab != null)
        {
            GameObject go = Instantiate(animalData.prefab, spawnPoint.position, spawnPoint.rotation, animalsParent);

            if (go.TryGetComponent<IAnimal>(out var animal))
            {
                if (waypointPath != null && waypointPath.waypoints.Count > 0)
                {
                    AnimalPatrol patrol = go.GetComponentInChildren<AnimalPatrol>();
                    if (patrol)
                    {
                        patrol.SetWaypoints(waypointPath);
                    }
                    else
                    {
                        Debug.LogWarning($"Animal prefab {animalData.prefab.name} does not have an AnimalPatrol component.");
                    }
                }

                animal.SpawnAsVisitor();
            }
        }
    }
}
