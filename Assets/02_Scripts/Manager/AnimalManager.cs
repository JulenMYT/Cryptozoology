using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    [SerializeField] private float tickInterval = 2f;
    [SerializeField] private List<AnimalGroup> animalGroups = new();
    private Dictionary<string, AnimalGroup> groups = new();
    private float tickTimer = 0f;

    private void Start()
    {
        SaveManager.Instance.OnSave += Save;
    }

    private void OnDisable()
    {
        SaveManager.Instance.OnSave -= Save;
    }

    public AnimalGroup RegisterAnimal(AnimalDataSO animalData, int initialSize = 1)
    {
        if (!groups.TryGetValue(animalData.displayName, out AnimalGroup group))
        {
            group = new AnimalGroup(animalData, initialSize);
            groups.Add(animalData.displayName, group);
            animalGroups.Add(group);
            Debug.Log($"Created new group for {animalData}");
        }
        else
        {
            group.GroupSize += initialSize;
        }

        return group;
    }

    public void RemoveAnimal(AnimalDataSO animalData, int amount = 1)
    {
        if (groups.TryGetValue(animalData.displayName, out AnimalGroup group))
        {
            group.GroupSize -= amount;
            if (group.GroupSize <= 0)
            {
                animalGroups.Remove(group);
                groups.Remove(animalData.displayName);
                Debug.Log($"Group {animalData} removed because it became empty");
            }
        }
    }

    public AnimalGroup GetGroup(string speciesName)
    {
        groups.TryGetValue(speciesName, out var group);
        return group;
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;
        tickTimer = 0f;

        float dayLength = GameManager.Instance.DayNight.DayLengthInSeconds;

        foreach (var group in groups.Values)
        {
            group.UpdateGroup(tickInterval, dayLength);
        }
    }

    private void Save()
    {
        foreach (var data in GetGlobalSaveData())
        {
            SaveManager.Instance.saveData.AddData(data);
        }
    }

    public List<AnimalManagerData> GetGlobalSaveData()
    {
        var list = new List<AnimalManagerData>();
        foreach (var kvp in groups)
        {
            var data = new AnimalManagerData
            {
                ID = SaveData.GenerateID(),
                SpeciesName = kvp.Key,
                Hunger = kvp.Value.Hunger
            };
            list.Add(data);
        }
        return list;
    }

    public void LoadFromSave(List<AnimalManagerData> datas)
    {
        foreach (var data in datas)
        {
            if (groups.TryGetValue(data.SpeciesName, out var group))
            {
                group.Hunger = data.Hunger;
            }
        }
    }
}

[Serializable]
public class AnimalGroup
{
    private const float maxHunger = 1f;
    public AnimalDataSO Data;
    public int GroupSize;
    public float Hunger;
    private House house;

    public AnimalGroup(AnimalDataSO speciesName, int initialSize)
    {
        Data = speciesName;
        GroupSize = initialSize;
        Hunger = maxHunger;
    }

    public void UpdateGroup(float deltaTime, float dayLength)
    {
        if (GroupSize <= 0) return;

        if (!house)
            SearchForHouse();

        float foodNeeded = Data.foodConsumptionPerDay * GroupSize * (deltaTime / dayLength);

        if (house && house.HasFood())
        {
            Hunger = Mathf.Min(maxHunger, Hunger + Data.hungerDecayRate * (deltaTime / dayLength));
            house.ConsumeFood(foodNeeded);
        }
        else
        {
            Hunger -= Data.hungerDecayRate * (deltaTime / dayLength);
        }
    }

    private void SearchForHouse()
    {
        var obj = GameManager.Instance.Garden.GetObject(Data.houseID);
        if (obj != null)
            house = obj.GetComponent<House>();
    }
}

