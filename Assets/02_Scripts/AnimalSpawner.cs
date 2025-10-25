using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

public class AnimalSpawner : MonoBehaviour
{
    private List<AnimalDataSO> animals = new();
    private static readonly string DatabasePath = "ItemDatabase";
    private ItemDatabase database;

    [SerializeField] private Transform animalsParent;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxAnimals = 5;
    private ScenePoints _scenePoints;

    private ScenePoints scenePoints
    {
        get
        {
            if (!_scenePoints)
            {
                _scenePoints = FindFirstObjectByType<ScenePoints>();
                if (_scenePoints == null)
                    Debug.LogError("No ScenePoints found in the scene!");
            }
            return _scenePoints;
        }
    }


    private float timer;
    private HashSet<string> spawnedAnimalIDs = new();

    private void Awake()
    {
        database = Resources.Load<ItemDatabase>(DatabasePath);
        if (database != null)
        {
            animals = database.GetItemsByCategory<AnimalDataSO>(ItemCategory.Animal).ToList();
        }

        if (!animalsParent)
        {
            animalsParent = new GameObject("Animals").transform;
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

        AnimalDataSO animalData = animals[Random.Range(0, animals.Count)];

        if (!CanSpawnAnimal(animalData)) return;

        Animal animal = SpawnAnimal(animalData, GetRandomLeavePoint());
        if (animal == null) return;
        animal.Initialize();
    }

    private bool CanSpawn()
    {
        return animals.Count > 0 && scenePoints.SpawnPoints.Count > 0 && spawnedAnimalIDs.Count < maxAnimals;
    }

    private bool CanSpawnAnimal(AnimalDataSO animalData)
    {
        if (spawnedAnimalIDs.Contains(animalData.displayName)) return false;
        if (GameManager.Instance.Garden.GetCount(animalData.displayName) >= 2) return false;
        if (animalData.conditions != null && !animalData.conditions.CanAppear()) return false;
        if (animalData.prefab == null) return false;
        float currentHour = GameManager.Instance.DayNight.GetHour();
        if (animalData.ShouldSleep(currentHour)) return false;
        if (animalData.ShouldSleep(currentHour + 2f)) return false;

        return true;
    }

    public Animal SpawnAnimal(AnimalDataSO animalData, Vector3 spawnPoint)
    {
        Debug.Log($"Spawning animal: {animalData.displayName} at {spawnPoint}");
        GameObject go = Instantiate(animalData.prefab, spawnPoint, Quaternion.identity, animalsParent);
        spawnedAnimalIDs.Add(animalData.displayName);

        if (!go.TryGetComponent<Animal>(out var animal)) return null;

        animal.BecameResident += () => UnregisterAnimal(animalData.displayName);
        animal.LeftGarden += () => UnregisterAnimal(animalData.displayName);

        return animal;
    }

    private void UnregisterAnimal(string id)
    {
        spawnedAnimalIDs.Remove(id);
    }

    public Vector3 GetRandomLeavePoint()
    {        
        Vector3 point = scenePoints.SpawnPoints[Random.Range(0, scenePoints.SpawnPoints.Count)].position;
        return point;
    }

    public WaypointPath GetWaypointPath()
    {
        return scenePoints.WaypointPath;
    }
}
