using UnityEngine;

public enum ItemCategory
{
    Plant,
    Animal,
    Production,
    Building,
    Resource
}

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public ItemCategory category;
    public Sprite icon;
}
