using System.Collections.Generic;
using UnityEngine;

public class AnimalSaveData : Data
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public AnimalState State;
    public AnimalType Type;
    public string name;

    public Dictionary<string, int> eatenCounts;
}
