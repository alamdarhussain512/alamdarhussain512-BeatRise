using UnityEngine;
using System.IO;
using System;

public static class SaveSystem {
    static string saveFileName = "save.json";
    static string tmpFileName = "save.json.tmp";
    static string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
    static string TmpPath => Path.Combine(Application.persistentDataPath, tmpFileName);

    public static void Save(PlayerProfile profile) {
        try {
            var json = JsonUtility.ToJson(profile, true);
            // write atomically: write temp then replace
            File.WriteAllText(TmpPath, json);
            if(File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(TmpPath, SavePath);
            Debug.Log($"Saved profile to {SavePath}");
        } catch(Exception ex) {
            Debug.LogError($"Failed to save profile: {ex.Message}");
        }
    }

    public static PlayerProfile Load() {
        try {
            if(!File.Exists(SavePath)) return new PlayerProfile();
            var json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<PlayerProfile>(json) ?? new PlayerProfile();
        } catch(Exception ex) {
            Debug.LogError($"Failed to load save, returning new profile: {ex.Message}");
            return new PlayerProfile();
        }
    }
}
