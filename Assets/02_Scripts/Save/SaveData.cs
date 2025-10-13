using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SaveData
{
    public Dictionary<string, PlaceableObjectSaveData> placeableObjectDatas = new();

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

    public void RemoveData(PlaceableObjectSaveData data)
    {
        if (placeableObjectDatas.ContainsKey(data.ID))
            placeableObjectDatas.Remove(data.ID);
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        placeableObjectDatas ??= new Dictionary<string, PlaceableObjectSaveData>();
    }
}
