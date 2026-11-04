using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SettingsSaveStorage
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "SettingsSaves");
    
    public static T Load<T>(string key) where T : new()
    {
        Debug.Log($"SettingsSaveStorage LoadData {key}".ToColor(Color.deepPink));

        var path = GetSaveKey(key);
    
        if (File.Exists(path))
        {
            var jsonString = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        return new T();
    }

    public static void Save(string key, object data)
    {
        Debug.Log($"SettingsSaveStorage SaveData {key}".ToColor(Color.hotPink));
    
        var path = GetSaveKey(key);
        CheckDirectoryPath(path);

        var contents = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, contents);
    }
    
    private static void CheckDirectoryPath(string path)
    {
        var directory = Path.GetDirectoryName(path);

        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string GetSaveKey(string key)
    {
        return Path.Combine(SavePath, $"{key}.json");
    }
}
