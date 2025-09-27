using UnityEngine;
using System.Collections.Generic;

public static class ItemDatabaseRuntime
{
    private static Dictionary<string, ObjectData> lookupById;
    private static Dictionary<ItemCategory, List<ObjectData>> lookupByCategory;

    private static string databasePath = "ItemDatabase";

    public static void Initialize(ItemDatabase database)
    {
        lookupById = new Dictionary<string, ObjectData>();
        lookupByCategory = new Dictionary<ItemCategory, List<ObjectData>>();

        foreach (var item in database.items)
        {
            if (!lookupById.ContainsKey(item.id))
                lookupById.Add(item.id, item);

            if (!lookupByCategory.ContainsKey(item.category))
                lookupByCategory[item.category] = new List<ObjectData>();

            lookupByCategory[item.category].Add(item);
        }
    }

    private static void EnsureInitialized()
    {
        if (lookupById == null || lookupByCategory == null)
            Initialize(Resources.Load<ItemDatabase>(databasePath));
    }

    public static ObjectData Get(string id)
    {
        EnsureInitialized();

        lookupById.TryGetValue(id, out var item);
        return item;
    }

    public static List<ObjectData> GetByCategory(ItemCategory category)
    {
        EnsureInitialized();

        if (lookupByCategory.TryGetValue(category, out var list))
            return list;
        return new List<ObjectData>();
    }
}
