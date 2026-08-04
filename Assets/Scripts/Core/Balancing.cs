using UnityEngine;

public static class Balancing {
    // Time
    public static float DefaultSecondsPerGameHour = 6f; // realistic testing pace

    // Energy
    public static float MaxEnergy = 100f;
    public static float EnergyRegenPerHour = 5f; // regen per in-game hour

    // XP & Leveling
    public static int BaseXPForLevel = 100;
    public static float XPLevelCurve = 1.25f; // multiplicative curve

    // Gig / Venue
    public static float VenueCapacityFactor = 0.1f; // payout multiplier per 100 capacity

    // Social platforms
    public static float TikTokVirality = 1.6f;
    public static float YouTubeVirality = 1.0f;
    public static float InstagramVirality = 0.9f;

    // Risk systems tuning
    public static float RiskyPlatformBaseChance = 0.20f; // base chance of a negative event on risky platforms
    public static float RiskyFineMultiplier = 0.05f; // fraction of current money taken as fine
    public static int RiskyFansLossMax = 500; // max fans lost on a serious incident
    public static int RiskyBanDaysMin = 1;
    public static int RiskyBanDaysMax = 7;
}
