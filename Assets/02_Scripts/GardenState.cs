using System.Collections.Generic;
using UnityEngine;

public class GardenState : MonoBehaviour
{
    private Dictionary<string, int> objectCounts = new ();

    private void OnEnable()
    {
        ItemEvents.OnItemAdded += OnItemAdded;
        ItemEvents.OnItemReplaced += ReplaceObject;
    }

    private void OnDisable()
    {
        ItemEvents.OnItemAdded -= OnItemAdded;
        ItemEvents.OnItemReplaced -= ReplaceObject;
    }

    private void OnItemAdded(ItemData item)
    {
        AddObject(item.id);
    }

    public void AddObject(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        if (objectCounts.ContainsKey(id))
            objectCounts[id]++;
        else
            objectCounts[id] = 1;
    }

    public void RemoveObject(string id)
    {
        if (string.IsNullOrEmpty(id) || !objectCounts.ContainsKey(id))
        {
            Debug.LogWarning($"Attempted to remove object with id '{id}' which does not exist in the garden state.");
            return;
        }

        objectCounts[id]--;
        if (objectCounts[id] <= 0)
            objectCounts.Remove(id);
    }

    public void ReplaceObject(string oldId, string newId)
    {
        RemoveObject(oldId);
        AddObject(newId);
    }

    public int GetCount(string id)
    {
        return objectCounts.ContainsKey(id) ? objectCounts[id] : 0;
    }

    public Dictionary<string, int> GetAllCounts()
    {
        return new Dictionary<string, int>(objectCounts);
    }
}
