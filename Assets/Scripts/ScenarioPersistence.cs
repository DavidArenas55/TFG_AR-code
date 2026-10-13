using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CustomScenarioSaveData
{
    public string name;
    public List<ScenarioObjectData> objects = new List<ScenarioObjectData>();

    public bool hasStopwatch;
    public bool hasCountdown;
    public float countdownTime;
}

public static class ScenarioPersistence
{
    private static string ScenariosFolder
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "Scenarios");
        }
    }

    public static void SaveScenario(
        string scenarioName,
        List<ScenarioObjectData> objects,
        bool hasStopwatch,
        bool hasCountdown,
        float countdownTime)
    {
        if (string.IsNullOrWhiteSpace(scenarioName))
        {
            Debug.LogError("No se puede guardar un escenario sin nombre.");
            return;
        }

        Directory.CreateDirectory(ScenariosFolder);

        CustomScenarioSaveData data = new CustomScenarioSaveData
        {
            name = scenarioName,
            objects = objects,
            hasStopwatch = hasStopwatch,
            hasCountdown = hasCountdown,
            countdownTime = countdownTime
        };

        string json = JsonUtility.ToJson(data, true);

        string filePath = Path.Combine(
            ScenariosFolder,
            MakeSafeFileName(scenarioName) + ".json"
        );

        File.WriteAllText(filePath, json);

        Debug.Log("Escenario guardado en: " + filePath);
    }

    public static CustomScenarioSaveData LoadScenario(string scenarioName)
    {
        string filePath = Path.Combine(
            ScenariosFolder,
            MakeSafeFileName(scenarioName) + ".json"
        );

        if (!File.Exists(filePath))
        {
            Debug.LogError("No existe el escenario: " + filePath);
            return null;
        }

        string json = File.ReadAllText(filePath);

        CustomScenarioSaveData data =
            JsonUtility.FromJson<CustomScenarioSaveData>(json);

        return data;
    }

    public static List<string> GetSavedScenarioNames()
    {
        List<string> names = new List<string>();

        if (!Directory.Exists(ScenariosFolder))
            return names;

        string[] files = Directory.GetFiles(ScenariosFolder, "*.json");

        foreach (string file in files)
        {
            names.Add(Path.GetFileNameWithoutExtension(file));
        }

        return names;
    }

    public static bool DeleteScenario(string scenarioName)
    {
        string filePath = Path.Combine(
            ScenariosFolder,
            MakeSafeFileName(scenarioName) + ".json"
        );

        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);
        return true;
    }

    private static string MakeSafeFileName(string fileName)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        return fileName.Trim();
    }
}
