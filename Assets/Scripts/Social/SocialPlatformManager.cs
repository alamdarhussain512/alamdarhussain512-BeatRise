using UnityEngine;
using System;
using System.Collections.Generic;

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
            new PlatformConfig { platformName = "Tok", followerCount = 12000, followerConversion = 0.06f, reachFactor = 1.2f, algorithmBoost = Balancing.TikTokVirality },
            new PlatformConfig { platformName = "Tube", followerCount = 8000, followerConversion = 0.03f, reachFactor = 1.0f, algorithmBoost = Balancing.YouTubeVirality },
            new PlatformConfig { platformName = "Gram", followerCount = 10000, followerConversion = 0.04f, reachFactor = 0.95f, algorithmBoost = Balancing.InstagramVirality },
            new PlatformConfig { platformName = "XBird", followerCount = 6000, followerConversion = 0.02f, reachFactor = 0.8f, algorithmBoost = 0.9f },
            new PlatformConfig { platformName = "Face", followerCount = 5000, followerConversion = 0.015f, reachFactor = 0.6f, algorithmBoost = 0.7f },
            new PlatformConfig { platformName = "SoundSpot", followerCount = 4000, followerConversion = 0.025f, reachFactor = 0.9f, algorithmBoost = 1.0f },
            // Fictional "dark web" style platform with risk mechanics
            new PlatformConfig { platformName = "ShadowNet", followerCount = 1500, followerConversion = 0.10f, reachFactor = 0.5f, algorithmBoost = 2.0f, isRisky = true, reputationRisk = 10 }
        };
    }

    public PlatformConfig GetPlatformByName(string name) {
        return platforms.Find(p => p.platformName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public (int reach, int gained) PublishPost(PlatformConfig platform, Post post) {
        if(platform == null || post == null) return (0,0);

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

        // Risk mechanics: risky platforms can cause reputation loss or other side-effects
        if(platform.isRisky && GameManager.Instance != null) {
            float chance = UnityEngine.Random.Range(0f,1f);
            // 20% chance of negative event
            if(chance < 0.2f) {
                var profile = GameManager.Instance.playerProfile;
                if(profile != null) {
                    profile.reputation = Mathf.Max(0, profile.reputation - platform.reputationRisk);
                    Debug.Log($"Risky publish: {platform.platformName} caused reputation loss of {platform.reputationRisk}. New reputation={profile.reputation}");
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
}
