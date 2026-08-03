using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlatformConfig {
    public string platformName;
    public int followerCount;
    public float followerConversion = 0.05f; // fraction of reach that converts
    public float reachFactor = 1.0f; // base reach multiplier
    public float algorithmBoost = 1.0f; // platform-specific boost (virality)

    // Risk / reputation mechanics for "dark web" style platforms
    public bool isRisky = false; // if true, publishing can have negative side-effects
    public int reputationRisk = 0; // reputation loss if negative event occurs

    // Tunables per platform
    public float riskChance = 0.2f; // override of global RiskyPlatformBaseChance
    public float fineAmountFixed = 0f; // fixed fine amount (optional) in game currency
    public int banDaysOnIncident = 0; // default ban days if incident triggers

    // Runtime state (not persisted directly here)
    [NonSerialized]
    public int bannedUntilDay = 0; // If currentDay < bannedUntilDay, platform is suspended
}

[System.Serializable]
public class Post {
    public string id;
    public string content;
    public float quality = 50f; // 0-100
    public float engagementRate = 0.05f; // 0-1
}

public class SocialPlatformManager : MonoBehaviour {
    public static SocialPlatformManager Instance;
    void Awake() {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // initialize default platforms if none provided
        if(platforms == null || platforms.Count == 0) InitializeDefaultPlatforms();
    }

    public List<PlatformConfig> platforms = new List<PlatformConfig>();

    public event Action<PlatformConfig, Post, int, int> OnPostPublished; // platform, post, reach, gainedFollowers

    void InitializeDefaultPlatforms() {
        platforms = new List<PlatformConfig> {
            // NOTE: Names intentionally altered to avoid trademark use (per request)
            new PlatformConfig { platformName = "Tok", followerCount = 12000, followerConversion = 0.06f, reachFactor = 1.2f, algorithmBoost = Balancing.TikTokVirality, riskChance = 0.05f },
            new PlatformConfig { platformName = "Tube", followerCount = 8000, followerConversion = 0.03f, reachFactor = 1.0f, algorithmBoost = Balancing.YouTubeVirality, riskChance = 0.03f },
            new PlatformConfig { platformName = "Gram", followerCount = 10000, followerConversion = 0.04f, reachFactor = 0.95f, algorithmBoost = Balancing.InstagramVirality, riskChance = 0.04f },
            new PlatformConfig { platformName = "XBird", followerCount = 6000, followerConversion = 0.02f, reachFactor = 0.8f, algorithmBoost = 0.9f, riskChance = 0.02f },
            new PlatformConfig { platformName = "Face", followerCount = 5000, followerConversion = 0.015f, reachFactor = 0.6f, algorithmBoost = 0.7f, riskChance = 0.02f },
            new PlatformConfig { platformName = "SoundSpot", followerCount = 4000, followerConversion = 0.025f, reachFactor = 0.9f, algorithmBoost = 1.0f, riskChance = 0.03f },
            // Fictional high-risk "dark/corrupt" platforms
            new PlatformConfig { platformName = "ShadowNet", followerCount = 1500, followerConversion = 0.10f, reachFactor = 0.5f, algorithmBoost = 2.0f, isRisky = true, reputationRisk = 10, riskChance = 0.25f, fineAmountFixed = 0f, banDaysOnIncident = 3 },
            new PlatformConfig { platformName = "BlackBazaar", followerCount = 900, followerConversion = 0.12f, reachFactor = 0.4f, algorithmBoost = 2.5f, isRisky = true, reputationRisk = 20, riskChance = 0.35f, fineAmountFixed = 200, banDaysOnIncident = 5 },
            new PlatformConfig { platformName = "DeepWave", followerCount = 600, followerConversion = 0.15f, reachFactor = 0.3f, algorithmBoost = 3.0f, isRisky = true, reputationRisk = 30, riskChance = 0.45f, fineAmountFixed = 500, banDaysOnIncident = 7 }
        };
    }

    public PlatformConfig GetPlatformByName(string name) {
        return platforms.Find(p => p.platformName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public (int reach, int gained) PublishPost(PlatformConfig platform, Post post) {
        if(platform == null || post == null) return (0,0);

        // Check bans
        var ts = FindObjectOfType<TimeSystem>();
        int currentDay = ts != null ? ts.currentDay : 0;
        if(platform.bannedUntilDay > currentDay) {
            Debug.Log($"Cannot publish: {platform.platformName} is suspended until day {platform.bannedUntilDay}");
            return (0,0);
        }

        // base reach: a fraction of followers influenced by engagement
        float baseReach = platform.followerCount * (0.01f + post.engagementRate);

        // quality multiplier: higher quality increases reach
        float qualityMult = 1f + (post.quality / 100f);

        // algorithm / platform factors
        float platformMult = platform.reachFactor * platform.algorithmBoost;

        // randomness to simulate virality
        float noise = UnityEngine.Random.Range(0.8f, 1.25f);

        int finalReach = Mathf.FloorToInt(baseReach * qualityMult * platformMult * noise);
        int gained = Mathf.FloorToInt(finalReach * platform.followerConversion);

        platform.followerCount += gained;

        Debug.Log($"Published post on {platform.platformName}: reach={finalReach}, gained={gained}, totalFollowers={platform.followerCount}");

        // Risk mechanics: risky platforms can cause a variety of negative side-effects
        if(platform.isRisky && GameManager.Instance != null) {
            float chance = UnityEngine.Random.Range(0f,1f);
            float threshold = Mathf.Clamp(platform.riskChance, 0f, 1f);
            if(chance < threshold) {
                var profile = GameManager.Instance.playerProfile;
                if(profile != null) {
                    // pick an outcome based on severity
                    float roll = UnityEngine.Random.Range(0f,1f);
                    if(roll < 0.4f) {
                        // Reputation loss
                        profile.reputation = Mathf.Max(0, profile.reputation - platform.reputationRisk);
                        Debug.Log($"Risk event: reputation loss {platform.reputationRisk} on {platform.platformName}");
                    } else if(roll < 0.7f) {
                        // Fine: either fixed or proportional
                        int fine = platform.fineAmountFixed > 0 ? Mathf.FloorToInt(platform.fineAmountFixed) : Mathf.FloorToInt(profile.money * Balancing.RiskyFineMultiplier);
                        profile.money = Mathf.Max(0, profile.money - fine);
                        Debug.Log($"Risk event: fined {fine} for activity on {platform.platformName}");
                    } else {
                        // Suspension: ban platform for some days
                        int banDays = platform.banDaysOnIncident > 0 ? platform.banDaysOnIncident : UnityEngine.Random.Range(Balancing.RiskyBanDaysMin, Balancing.RiskyBanDaysMax+1);
                        platform.bannedUntilDay = currentDay + banDays;
                        Debug.Log($"Risk event: {platform.platformName} suspended for {banDays} days (until day {platform.bannedUntilDay})");
                        // fans loss as part of suspension
                        int fansLoss = UnityEngine.Random.Range(10, Balancing.RiskyFansLossMax);
                        profile.fans = Mathf.Max(0, profile.fans - fansLoss);
                        Debug.Log($"Fans lost due to suspension: {fansLoss}");
                    }
                }
            }
        }

        OnPostPublished?.Invoke(platform, post, finalReach, gained);
        return (finalReach, gained);
    }

    // convenience overload: publish by platform name
    public (int reach, int gained) PublishPost(string platformName, Post post) {
        var p = GetPlatformByName(platformName);
        return PublishPost(p, post);
    }

    // Persist platform follower counts into the player's profile
    public void ExportToProfile(PlayerProfile profile) {
        if(profile == null) return;
        profile.socialPlatforms = platforms.Select(p => new PlatformSnapshot { platformName = p.platformName, followerCount = p.followerCount, bannedUntilDay = p.bannedUntilDay }).ToList();
    }

    // Load platform follower counts from player's profile
    public void ImportFromProfile(PlayerProfile profile) {
        if(profile == null || profile.socialPlatforms == null || profile.socialPlatforms.Count == 0) return;
        foreach(var snap in profile.socialPlatforms) {
            var existing = GetPlatformByName(snap.platformName);
            if(existing != null) {
                existing.followerCount = snap.followerCount;
                existing.bannedUntilDay = snap.bannedUntilDay;
            }
            else {
                // if platform not found, add a placeholder platform with the saved follower count
                var p = new PlatformConfig { platformName = snap.platformName, followerCount = snap.followerCount };
                p.bannedUntilDay = snap.bannedUntilDay;
                platforms.Add(p);
            }
        }
    }
}
