using UnityEngine;
using System.Collections.Generic;

public class UIAnimalPage : MonoBehaviour
{
    [SerializeField] private GameObject[] sections;

    [SerializeField] private string animalId;

    public void Start()
    {
        List<int> unlocked = GameManager.Instance.Encyclopedia.GetUnlockedSections(animalId);
        Debug.Log($"Animal {animalId} has unlocked sections: {string.Join(", ", unlocked)}");
        for (int i = 0; i < sections.Length; i++)
            sections[i].SetActive(unlocked.Contains(i+1));
    }
}
