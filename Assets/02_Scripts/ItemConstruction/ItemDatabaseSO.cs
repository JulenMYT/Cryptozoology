using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public ObjectData[] items;

    public ObjectData GetItemById(string id)
    {
        foreach (var item in items)
        {
            if (item.id == id)
                return item;
        }
        return null;
    }

    public IEnumerable<T> GetItemsByCategory<T>(ItemCategory category) where T : ObjectData
    {
        return items.Where(item => item.category == category).OfType<T>();
    }
}
    