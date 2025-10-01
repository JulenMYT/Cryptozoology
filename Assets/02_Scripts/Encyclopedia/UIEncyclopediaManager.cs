using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEncyclopediaManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup gridCanvas;
    [SerializeField] private CanvasGroup encyclopediaCanvas;

    [SerializeField] private Transform gridParent;
    [SerializeField] private UIEncyclopediaAnimalButton animalIconPrefab;
    [SerializeField] private Sprite unknownSprite;
    [SerializeField] private Button toggleGridButton;

    [SerializeField] private Transform leftPage;
    [SerializeField] private Transform rightPage;
    [SerializeField] private Button backButton;

    private List<AnimalDataSO> animals = new();
    private Dictionary<string, UIEncyclopediaAnimalButton> spawnedIcons = new();
    private ItemDatabase itemDatabase;

    private const string DatabasePath = "ItemDatabase";

    private void Awake()
    {
        toggleGridButton.onClick.RemoveAllListeners();
        toggleGridButton.onClick.AddListener(OpenGrid);

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(BackToGrid);

        SetCanvasVisible(gridCanvas, false);
        SetCanvasVisible(encyclopediaCanvas, false);
    }

    private void Start()
    {
        LoadAnimals();
        PopulateGrid();
    }

    private void OpenGrid()
    {
        SetCanvasVisible(gridCanvas, true);
        SetCanvasVisible(encyclopediaCanvas, false);
        RefreshGrid();

        toggleGridButton.onClick.RemoveAllListeners();
        toggleGridButton.onClick.AddListener(CloseAll);
    }

    private void CloseAll()
    {
        SetCanvasVisible(gridCanvas, false);
        SetCanvasVisible(encyclopediaCanvas, false);

        toggleGridButton.onClick.RemoveAllListeners();
        toggleGridButton.onClick.AddListener(OpenGrid);
    }

    private void BackToGrid()
    {
        SetCanvasVisible(encyclopediaCanvas, false);
        SetCanvasVisible(gridCanvas, true);

        toggleGridButton.onClick.RemoveAllListeners();
        toggleGridButton.onClick.AddListener(CloseAll);
    }

    private void SetCanvasVisible(CanvasGroup canvas, bool visible)
    {
        canvas.alpha = visible ? 1f : 0f;
        canvas.interactable = visible;
        canvas.blocksRaycasts = visible;
    }

    private void LoadAnimals()
    {
        animals.Clear();
        itemDatabase = Resources.Load<ItemDatabase>(DatabasePath);
        if (itemDatabase == null) return;
        animals.AddRange(itemDatabase.GetItemsByCategory<AnimalDataSO>(ItemCategory.Animal));
    }

    private void PopulateGrid()
    {
        foreach (var animal in animals)
        {
            var icon = Instantiate(animalIconPrefab, gridParent);
            spawnedIcons[animal.id] = icon;
            bool unlocked = GameManager.Instance.Encyclopedia.IsSectionUnlocked(animal.id, 1);
            icon.Setup(animal.id, unlocked ? animal.icon : unknownSprite);
            icon.OnButtonClicked += OnAnimalIconClicked;
        }
    }

    private void OnAnimalIconClicked(string animalId)
    {
        OpenEncyclopediaPage(animalId);
    }

    public void RefreshGrid()
    {
        foreach (var animal in animals)
        {
            if (!spawnedIcons.TryGetValue(animal.id, out var icon)) continue;
            bool unlocked = GameManager.Instance.Encyclopedia.IsSectionUnlocked(animal.id, 1);
            icon.UpdateIconImage(unlocked ? animal.icon : unknownSprite);
        }
    }

    private void OpenEncyclopediaPage(string animalId)
    {
        int animalIndex = animals.FindIndex(a => a.id == animalId);
        if (animalIndex == -1) return;

        SetCanvasVisible(encyclopediaCanvas, true);
        SetCanvasVisible(gridCanvas, false);

        ClearPage(leftPage);
        ClearPage(rightPage);

        bool isRightPage = animalIndex % 2 == 0;
        GameObject mainPrefab = animals[animalIndex].encyclopediaPrefab;

        if (isRightPage)
        {
            Instantiate(mainPrefab, rightPage);
            if (animalIndex > 0)
                Instantiate(animals[animalIndex - 1].encyclopediaPrefab, leftPage);
        }
        else
        {
            Instantiate(mainPrefab, leftPage);
            if (animalIndex + 1 < animals.Count)
                Instantiate(animals[animalIndex + 1].encyclopediaPrefab, rightPage);
        }
    }

    private void ClearPage(Transform page)
    {
        for (int i = page.childCount - 1; i >= 0; i--)
            Destroy(page.GetChild(i).gameObject);
    }
}
