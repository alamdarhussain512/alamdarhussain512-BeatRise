using UnityEngine;

[System.Serializable]
public class Skills {
    public int vocals = 1;
    public int production = 1;
    public int stagecraft = 1;
    public int charisma = 1;
    public int marketing = 1;
}

[System.Serializable]
public class PlayerProfile {
    public string playerName = "NewArtist";
    public string genre = "Pop";
    public int level = 1;
    public int xp = 0;
    public int money = 500;
    public int fans = 100;
    public float energy = Balancing.MaxEnergy;
    public Skills skills = new Skills();

    // Add XP and handle leveling
    public void AddXP(int amount) {
        xp += amount;
        while(xp >= XPForLevel(level+1)) {
            xp -= XPForLevel(level+1);
            level++;
            OnLevelUp();
        }
    }

    int XPForLevel(int targetLevel) {
        // geometric curve
        int req = Mathf.FloorToInt(Balancing.BaseXPForLevel * Mathf.Pow(Balancing.XPLevelCurve, targetLevel-1));
        return Mathf.Max(10, req);
    }

    void OnLevelUp() {
        // simple reward: restore some energy and give small money bonus
        energy = Mathf.Min(Balancing.MaxEnergy, energy + 20f);
        money += 100;
        Debug.Log($"Player leveled up to {level}");
    }
}
