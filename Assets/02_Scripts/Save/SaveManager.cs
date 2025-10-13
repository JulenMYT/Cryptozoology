using System;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
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
        LoadPlaceableObjects();
    }

    private void LoadPlaceableObjects()
    {
        foreach (var objData in saveData.placeableObjectDatas.Values)
        {
            try
            {
                ObjectData itemData = ItemDatabaseRuntime.Get(objData.name);
                if (itemData == null)
                {
                    Debug.LogWarning($"ItemData with name {objData.name} not found in database.");
                    continue;
                }

                GameObject obj = GameManager.Instance.BuildingSystem.PlaceItem(itemData, objData.position);

                if (obj == null)
                {
                    Debug.LogWarning($"Failed to place item {itemData.displayName} at position {objData.position}.");
                    continue;
                }

                PlaceableObject placeableObject = obj.GetComponent<PlaceableObject>();

                if (placeableObject == null)
                {
                    Debug.LogWarning($"Placed object does not have a PlaceableObject component.");
                    continue;
                }

                placeableObject.Initialize(itemData, objData);
                placeableObject.Place();
            }
            catch (System.Exception)
            {

                throw;
            }
        }
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

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
