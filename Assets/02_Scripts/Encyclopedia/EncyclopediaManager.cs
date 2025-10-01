using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimalProgress
{
    public string animalId;
    public List<int> unlockedSections = new();
}

public class EncyclopediaManager : MonoBehaviour
{
    [SerializeField] private List<AnimalProgress> serializedProgress = new();

    private Dictionary<string, AnimalProgress> progress = new();

    private void Awake()
    {
        LoadSerialized();
    }

    private void LoadSerialized()
    {
        progress.Clear();
        foreach (var ap in serializedProgress)
        {
            if (!string.IsNullOrEmpty(ap.animalId))
                progress[ap.animalId] = ap;
        }
    }

    public void UnlockSection(string animalId, int sectionIndex)
    {
        sectionIndex = Mathf.Clamp(sectionIndex, 1, 4);

        if (!progress.ContainsKey(animalId))
        {
            var newProgress = new AnimalProgress { animalId = animalId };
            progress[animalId] = newProgress;
            serializedProgress.Add(newProgress);
        }

        var sections = progress[animalId].unlockedSections;
        if (!sections.Contains(sectionIndex))
            sections.Add(sectionIndex);
    }

    public bool IsSectionUnlocked(string animalId, int sectionIndex)
    {
        return progress.ContainsKey(animalId) && progress[animalId].unlockedSections.Contains(sectionIndex);
    }

    public List<int> GetUnlockedSections(string animalId)
    {
        if (!progress.ContainsKey(animalId)) return new List<int>();
        return progress[animalId].unlockedSections;
    }
}
