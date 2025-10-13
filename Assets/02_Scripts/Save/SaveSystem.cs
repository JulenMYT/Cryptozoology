using System.IO;
using Newtonsoft.Json;
using UnityEditor.Overlays;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    public const string FILE_NAME = "SaveFile";
    private const string SAVE_EXTENSION = ".sav";
    public static string fileName { get; private set; }
    public static string filePath { get; private set; }

    public static void Initialize()
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }

        fileName = FILE_NAME + SAVE_EXTENSION;
        filePath = SAVE_FOLDER + FILE_NAME + SAVE_EXTENSION;
    }

    public static void Save(SaveData saveObject)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string saveString = JsonConvert.SerializeObject(saveObject, settings);
        Debug.Log($"[SaveSystem] Saving to {filePath}:\n{saveString}");
        File.WriteAllText(filePath, saveString);
    }

    public static SaveData Load()
    {
        if (File.Exists(filePath))
        {
            string saveString = File.ReadAllText(filePath);
            Debug.Log($"[SaveSystem] Loading from {filePath}:\n{saveString}");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            SaveData loaded = JsonConvert.DeserializeObject<SaveData>(saveString, settings);
            if (loaded == null)
            {
                return new SaveData();
            }
            return loaded;
        }
        else
        {
            return new SaveData();
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"[SaveSystem] Deleted save file at {filePath}");
        }
    }
}
