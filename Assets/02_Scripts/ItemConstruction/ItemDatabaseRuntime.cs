using UnityEngine;
using System.Collections.Generic;

public static class ItemDatabaseRuntime
{
    private static Dictionary<string, ItemData> lookupById;
    private static Dictionary<ItemCategory, List<ItemData>> lookupByCategory;

    private static string databasePath = "ItemDatabase";

    public static void Initialize(ItemDatabase database)
    {
        lookupById = new Dictionary<string, ItemData>();
        lookupByCategory = new Dictionary<ItemCategory, List<ItemData>>();

        foreach (var item in database.items)
        {
            if (!lookupById.ContainsKey(item.id))
                lookupById.Add(item.id, item);

            if (!lookupByCategory.ContainsKey(item.category))
                lookupByCategory[item.category] = new List<ItemData>();

            lookupByCategory[item.category].Add(item);
        }
    }

    private static void EnsureInitialized()
    {
        if (lookupById == null || lookupByCategory == null)
            Initialize(Resources.Load<ItemDatabase>(databasePath));
    }

    public static ItemData Get(string id)
    {
        EnsureInitialized();

        lookupById.TryGetValue(id, out var item);
        return item;
    }

    public static List<ItemData> GetByCategory(ItemCategory category)
    {
        EnsureInitialized();

        if (lookupByCategory.TryGetValue(category, out var list))
            return list;
        return new List<ItemData>();
    }
}
