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
    public int money = 500;
    public int fans = 100;
    public float energy = 100f;
    public Skills skills = new Skills();
}
