using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class House : PlaceableObject, IClickable
{
    [SerializeField] private UIHouse uiHouse;
    [SerializeField] private float capacity = 5f;
    private float foodAmount = 0f;

    private List<AnimalSleep> animals = new();
    public float FoodAmount => foodAmount;

    [ReadOnly]
    public HouseSaveData HouseSaveData = new();

    private void Start()
    {
        if (uiHouse == null)
        {
            Debug.LogError("UIHouse reference is missing in House script.");
            return;
        }

        uiHouse.Hide();
    }

    private void OnDisable()
    {
        if (uiHouse != null)
        {
            uiHouse.OnFeedButtonClicked -= FillFood;
        }
    }

    public override void Initialize(ObjectData data)
    {
        ObjectData = data;
        HouseSaveData.ID = SaveData.GenerateID();
        HouseSaveData.name = data.displayName;
        foodAmount = capacity;
        GameManager.Instance.Garden.AddObject(data.displayName, gameObject);
    }

    public override void Initialize(ObjectData objectData, PlaceableObjectSaveData saveData)
    {
        ObjectData = objectData;
        HouseSaveData = saveData as HouseSaveData;
        foodAmount = HouseSaveData.foodAmount;
        GameManager.Instance.Garden.AddObject(objectData.displayName, gameObject);
    }

    public override void Place()
    {
        base.Place();
        
        uiHouse.OnFeedButtonClicked += FillFood;
    }

    public void AddAnimal(AnimalSleep animal)
    {
        if (!animals.Contains(animal))
        {
            animals.Add(animal);
        }
    }

    public void RemoveAnimal(AnimalSleep animal)
    {
        if (animals.Contains(animal))
        {
            animals.Remove(animal);
        }
    }

    protected override void OnDestroy()
    {
        foreach (var animal in animals)
        {
            if (animal != null && animal.IsActive())
            {
                animal.HouseDone();
            }
        }

        if (Placed)
            GameManager.Instance.Garden.RemoveObject(ObjectData.displayName, gameObject);
    }

    private void FillFood()
    {
        uiHouse.Hide();     
        foodAmount = capacity;
        Debug.Log("Maison remplie de nourriture");
    }

    public bool HasFood()
    {
        Debug.Log($"Vérification de la nourriture dans la maison: {foodAmount} unités restantes.");
        return foodAmount > 0;
    }

    public void ConsumeFood(float amount)
    {
        foodAmount = Mathf.Max(0, foodAmount - amount);
    }

    public void OnClick()
    {
        uiHouse.Show();
    }

    public void OnCancel()
    {
        uiHouse.Hide();
    }

    protected override void Save()
    {
        HouseSaveData.position = transform.position;
        HouseSaveData.foodAmount = foodAmount;
        SaveManager.Instance.saveData.AddData(HouseSaveData);
    }
}
