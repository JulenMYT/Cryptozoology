using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    private Dictionary<string, AnimalGroup> groups = new Dictionary<string, AnimalGroup>();

    public AnimalGroup RegisterAnimal(string speciesName, int initialSize = 1)
    {
        if (!groups.TryGetValue(speciesName, out AnimalGroup group))
        {
            group = new AnimalGroup(speciesName, initialSize);
            groups.Add(speciesName, group);
            Debug.Log($"Created new group for {speciesName}");
        }
        else
        {
            group.GroupSize += initialSize;
        }

        return group;
    }

    public void RemoveAnimal(string speciesName, int amount = 1)
    {
        if (groups.TryGetValue(speciesName, out AnimalGroup group))
        {
            group.GroupSize -= amount;
            if (group.GroupSize <= 0)
            {
                groups.Remove(speciesName);
                Debug.Log($"Group {speciesName} removed because it became empty");
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
        float dayLength = 60f;
        foreach (var group in groups.Values)
        {
            group.UpdateGroup(Time.deltaTime, dayLength);
        }
    }
}

[Serializable]
public class AnimalGroup
{
    public string SpeciesName;
    public int GroupSize;

    public float Hunger;

    public AnimalGroup(string speciesName, int initialSize)
    {
        SpeciesName = speciesName;
        GroupSize = initialSize;
        Hunger = 1f;
    }

    public void UpdateGroup(float deltaTime, float dayLength, float hungerDecayPerDay = 0.1f)
    {
        Hunger = Math.Clamp(Hunger - hungerDecayPerDay * (deltaTime / dayLength), 0f, 1f);
    }
}

