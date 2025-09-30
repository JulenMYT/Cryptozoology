using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimalProgress
{
    public string animalId;
    public int unlockedSections;
}

public class EncyclopediaManager : MonoBehaviour
{
    private Dictionary<string, int> progress = new();

    public void UnlockSection(string animalId)
    {
        if (!progress.ContainsKey(animalId))
            progress[animalId] = 0;

        progress[animalId] = Mathf.Min(progress[animalId] + 1, 4);
    }

    public int GetUnlockedSections(string animalId)
    {
        if (!progress.ContainsKey(animalId)) return 0;
        return progress[animalId];
    }
}
