using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static string PrefabPath = "SaveManagerPrefab";

    private static SaveManager instance;
    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = Instantiate(Resources.Load<GameObject>(PrefabPath));
                instance = go.GetComponent<SaveManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeScene()
    {
        _ = Instance;
    }
    public SaveData saveData;

    public event Action OnSave;

    private void Awake()
    {
        SaveSystem.Initialize();
    }

    private void Start()
    {
        saveData = SaveSystem.Load();
        LoadGame();
    }

    private void SaveGame()
    {
        saveData.ClearData();
        OnSave?.Invoke();
        SaveSystem.Save(saveData);
    }

    private void LoadGame()
    {
        GameManager.Instance.DayNight.Load(saveData.dayNightSaveData);
        GameManager.Instance.Encyclopedia.Load(saveData.encyclopediaData);
        GameManager.Instance.Money.Load(saveData.moneyData);
        LoadPlaceableObjects();
        LoadAnimals();
    }

    private void LoadPlaceableObjects()
    {
        foreach (var objData in saveData.placeableObjectDatas.Values)
        {
            try
            {
                ObjectData itemData = ItemDatabaseRuntime.Get(objData.name);
                GameObject obj = GameManager.Instance.BuildingSystem.PlaceItem(itemData, objData.position);
                PlaceableObject placeableObject = obj.GetComponent<PlaceableObject>();

                placeableObject.Initialize(itemData, objData);
                placeableObject.Place();
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }

    private void LoadAnimals()
    {
        foreach (var animalData in saveData.animalDatas.Values)
        {
            AnimalDataSO itemData = (AnimalDataSO)ItemDatabaseRuntime.Get(animalData.name);
            Animal animal;
            if (animalData.Type == AnimalType.Resident)
            {
                GameObject obj = GameManager.Instance.BuildingSystem.PlaceItem(itemData, animalData.Position);
                animal = obj.GetComponent<Animal>();
            }
            else
            {
                animal = GameManager.Instance.AnimalSpawner.SpawnAnimal(itemData, animalData.Position);
            }

            animal.Initialize(animalData);
        }

        GameManager.Instance.Animals.LoadFromSave(
            new List<AnimalManagerData>(saveData.animalGlobalDatas.Values)
        );
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveSystem.DeleteSave();
        }
    }
}
