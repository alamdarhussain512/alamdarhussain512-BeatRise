using System;

[Serializable]
public enum OfferType {
    PaidPromotion,
    FakeFollowers,
    Leak,
    Sabotage
}

[Serializable]
public class UndergroundOffer {
    public string id;
    public string name;
    public string description;
    public OfferType type;

    public int cost; // in-game currency
    public float successChance; // 0-1
    public float detectionChance; // 0-1 base, scaled by notoriety
    public int followerBoost; // immediate followers on success
    public int notorietyGain; // adds to player's notoriety when used

    // optional fixed fine applied on detection
    public int fineOnDetection = 0;
}
