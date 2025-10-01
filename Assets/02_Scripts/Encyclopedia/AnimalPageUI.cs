using UnityEngine;
using System.Collections.Generic;

public class AnimalPageUI : MonoBehaviour
{
    [SerializeField] private GameObject[] sections;

    private string animalId;

    public void Setup(string id)
    {
        animalId = id;
        List<int> unlocked = GameManager.Instance.Encyclopedia.GetUnlockedSections(animalId);

        for (int i = 0; i < sections.Length; i++)
            sections[i].SetActive(unlocked.Contains(i+1));
    }
}
