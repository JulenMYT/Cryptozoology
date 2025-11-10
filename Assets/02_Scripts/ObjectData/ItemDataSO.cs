using UnityEngine;

public enum ItemCategory
{
    Animal,
    Plant,
    Shovel,
    House
}

public enum ItemSubCategory
{
    None,
    AnimalExerior,
    AnimalHouse,
    PlantVegetable,
    PlantFlower,
    PlantTree
}

[CreateAssetMenu(menuName = "Game/Item")]
public class ObjectData : ScriptableObject
{
    public string displayName;
    public ItemCategory category;
    public ItemSubCategory subCategory;
    public Sprite icon;
    public GameObject prefab;
    public Vector2Int gridSize = Vector2Int.one;
    public bool isUnique = false;
    public bool isGridItem = false;
    public int cost = 0;
}
