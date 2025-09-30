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
    [SerializeField] private int maxAnimals = 5;

    private float timer;
    private HashSet<string> spawnedAnimalIDs = new();

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
        if (!CanSpawn()) return;

        AnimalDataSO animalData = animals[UnityEngine.Random.Range(0, animals.Count)];

        if (!CanSpawnAnimal(animalData)) return;

        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        SpawnAnimal(animalData, spawnPoint);
    }

    private bool CanSpawn()
    {
        return animals.Count > 0 && spawnPoints.Count > 0 && spawnedAnimalIDs.Count < maxAnimals;
    }

    private bool CanSpawnAnimal(AnimalDataSO animalData)
    {
        if (spawnedAnimalIDs.Contains(animalData.id)) return false;
        if (GameManager.Instance.Garden.GetCount(animalData.id) >= 2) return false;
        if (animalData.conditions != null && !animalData.conditions.CanAppear()) return false;
        if (animalData.prefab == null) return false;

        return true;
    }

    private void SpawnAnimal(AnimalDataSO animalData, Transform spawnPoint)
    {
        GameObject go = Instantiate(animalData.prefab, spawnPoint.position, spawnPoint.rotation, animalsParent);
        spawnedAnimalIDs.Add(animalData.id);

        if (!go.TryGetComponent<Animal>(out var animal)) return;

        SetupPatrol(go);
        SetupLeave(go, spawnPoint);

        animal.SpawnAsVisitor();
        animal.BecameResident += () => UnregisterAnimal(animalData.id);
        animal.LeftGarden += () => UnregisterAnimal(animalData.id);
    }

    private void SetupPatrol(GameObject go)
    {
        if (waypointPath == null) return;

        AnimalPatrol patrol = go.GetComponentInChildren<AnimalPatrol>();
        if (patrol)
        {
            patrol.SetWaypoints(waypointPath);
        }
        else
        {
            Debug.LogWarning($"Animal prefab {go.name} does not have an AnimalPatrol component.");
        }
    }

    private void SetupLeave(GameObject go, Transform spawnPoint)
    {
        AnimalLeave leave = go.GetComponentInChildren<AnimalLeave>();
        if (leave)
        {
            leave.SetSpawnPoint(spawnPoint);
        }
        else
        {
            Debug.LogWarning($"Animal prefab {go.name} does not have an AnimalLeave component.");
        }
    }

    private void UnregisterAnimal(string id)
    {
        spawnedAnimalIDs.Remove(id);
    }
}
