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
}
