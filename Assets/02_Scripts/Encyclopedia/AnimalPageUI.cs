using UnityEngine;

public class AnimalPageUI : MonoBehaviour
{
    [SerializeField] private GameObject[] sections;

    private string animalId;

    public void Setup(string id)
    {
        animalId = id;
        int unlocked = GameManager.Instance.Encyclopedia.GetUnlockedSections(animalId);

        for (int i = 0; i < sections.Length; i++)
            sections[i].SetActive(i < unlocked);
    }
}
