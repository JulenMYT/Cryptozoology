using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SaveData
{
    public DayNightSaveData dayNightSaveData = new();
    public Dictionary<string, PlaceableObjectSaveData> placeableObjectDatas = new();
    public Dictionary<string, AnimalSaveData> animalDatas = new();
    public Dictionary<string, AnimalManagerData> animalGlobalDatas = new();
    public EncyclopediaSaveData encyclopediaData = new();
    public MoneySaveData moneyData = new();

    public static string GenerateID()
    {
        return Guid.NewGuid().ToString();
    }

    public void AddData(PlaceableObjectSaveData data)
    {
        if (placeableObjectDatas.ContainsKey(data.ID))
            placeableObjectDatas[data.ID] = data;
        else
            placeableObjectDatas.Add(data.ID, data);
    }

    public void AddData(AnimalSaveData data)
    {
        if (animalDatas.ContainsKey(data.ID))
            animalDatas[data.ID] = data;
        else
            animalDatas.Add(data.ID, data);
    }

    public void AddData(AnimalManagerData data)
    {
        if (animalGlobalDatas.ContainsKey(data.SpeciesName))
            animalGlobalDatas[data.SpeciesName] = data;
        else
            animalGlobalDatas.Add(data.SpeciesName, data);
    }

    public void ClearData()
    {
        placeableObjectDatas.Clear();
        animalDatas.Clear();
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        placeableObjectDatas ??= new Dictionary<string, PlaceableObjectSaveData>();
    }
}
