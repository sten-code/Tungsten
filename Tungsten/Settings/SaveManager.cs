using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Tungsten.Settings
{
    public class SaveManager
    {
        public string FileName { get; set; }
        public Dictionary<string, object> SaveFile { get; set; }
        public static SaveManager Instance { get; set; }

        public SaveManager(string fileName)
        {
            FileName = fileName;
            Instance = this;
            SaveFile = new Dictionary<string, object>();
            if (File.Exists(FileName))
            {
                Dictionary<string, JsonElement> data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(FileName));
                if (data != null)
                {
                    foreach (KeyValuePair<string, JsonElement> pair in data)
                    {
                        SaveFile[pair.Key] = pair.Value;
                    }
                }
            }
        }

        public void Save(string identifier, object value)
        {
            SaveFile[identifier] = value;
            string json = JsonSerializer.Serialize(SaveFile);
            File.WriteAllText(FileName, json);
        }

        public T Load<T>(string identifier, T defaultValue)
        {
            if (!File.Exists(FileName))
                return defaultValue;

            string json = File.ReadAllText(FileName);
            Dictionary<string, JsonElement> data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (data == null)
            {
                return defaultValue;
            }
            else
            {
                if (data.ContainsKey(identifier))
                {
                    return data[identifier].Deserialize<T>();
                }
                else
                {
                    return defaultValue;
                }
            }
        }

    }
}
