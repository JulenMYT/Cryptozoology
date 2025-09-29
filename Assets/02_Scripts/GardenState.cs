using System.Collections.Generic;
using UnityEngine;

public class GardenState : MonoBehaviour
{
    private Dictionary<string, int> objectCounts = new ();
    private Dictionary<string, List<GameObject>> objectRefs = new();

    public void AddObject(string id, GameObject obj = null)
    {
        if (string.IsNullOrEmpty(id)) return;

        if (objectCounts.ContainsKey(id))
            objectCounts[id]++;
        else
            objectCounts[id] = 1;

        if (obj != null)
        {
            if (!objectRefs.ContainsKey(id))
                objectRefs[id] = new List<GameObject>();
            objectRefs[id].Add(obj);
        }
    }

    public void RemoveObject(string id, GameObject obj = null)
    {
        if (string.IsNullOrEmpty(id) || !objectCounts.ContainsKey(id)) return;

        objectCounts[id]--;
        if (objectCounts[id] <= 0)
            objectCounts.Remove(id);

        if (obj != null && objectRefs.ContainsKey(id))
        {
            objectRefs[id].Remove(obj);
            if (objectRefs[id].Count == 0)
                objectRefs.Remove(id);
        }
    }

    public void ReplaceObject(string oldId, string newId, GameObject obj = null)
    {
        RemoveObject(oldId, obj);
        AddObject(newId, obj);
    }

    public int GetCount(string id)
    {
        return objectCounts.ContainsKey(id) ? objectCounts[id] : 0;
    }

    public Dictionary<string, int> GetAllCounts()
    {
        return new Dictionary<string, int>(objectCounts);
    }

    public void DebugGardenState()
    {
        Debug.Log("Garden state:");
        foreach (var kvp in objectCounts)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
    }
}
