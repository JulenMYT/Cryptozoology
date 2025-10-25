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
    [SerializeField] private bool bLoadSerialized = false;

    private Dictionary<string, AnimalProgress> progress = new();

    private void Awake()
    {
        if (bLoadSerialized)
            LoadSerialized();
    }

    private void Start()
    {
        SaveManager.Instance.OnSave += Save;
    }

    private void OnDisable()
    {
        SaveManager.Instance.OnSave -= Save;
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

    private void Save()
    {
        var encyclopediaData = new EncyclopediaSaveData();

        foreach (var kvp in progress)
        {
            encyclopediaData.progress[kvp.Value.animalId] = kvp.Value.unlockedSections;
        }

        SaveManager.Instance.saveData.encyclopediaData = encyclopediaData;
    }

    public void Load(EncyclopediaSaveData encyclopediaData)
    {
        if (bLoadSerialized)
            return;

        progress.Clear();
        serializedProgress.Clear();
        foreach (var entry in encyclopediaData.progress)
        {
            var animalProgress = new AnimalProgress
            {
                animalId = entry.Key,
                unlockedSections = new List<int>(entry.Value)
            };
            progress[entry.Key] = animalProgress;
            serializedProgress.Add(animalProgress);
        }
    }
}
