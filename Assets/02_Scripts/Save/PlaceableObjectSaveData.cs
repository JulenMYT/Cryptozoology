using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
[JsonObject]
public class PlaceableObjectSaveData : Data
{
    public Vector3 position;
    public string name;
}

[Serializable]
public class PlantSaveData : PlaceableObjectSaveData
{
    public int stage;
    public float timer;
    public int portionsLeft;
    public bool isMature;
}