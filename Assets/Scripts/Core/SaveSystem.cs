using UnityEngine;
using System.IO;

public static class SaveSystem {
    static string savePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(PlayerProfile profile) {
        var json = JsonUtility.ToJson(profile);
        File.WriteAllText(savePath, json);
        Debug.Log($"Saved profile to {savePath}");
    }

    public static PlayerProfile Load() {
        if(!File.Exists(savePath)) return new PlayerProfile();
        var json = File.ReadAllText(savePath);
        try {
            return JsonUtility.FromJson<PlayerProfile>(json);
        } catch {
            Debug.LogError("Failed to load save, returning new profile");
            return new PlayerProfile();
        }
    }
}
