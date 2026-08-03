using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlatformConfig {
    public string platformName;
    public int followerCount;
    public float followerConversion = 0.05f; // fraction of reach that converts
}

[System.Serializable]
public class Post {
    public string id;
    public string content;
    public float quality = 50f; // 0-100
    public float engagementRate = 0.05f;
}

public class SocialPlatformManager : MonoBehaviour {
    public static SocialPlatformManager Instance;
    void Awake() => Instance = this;

    public List<PlatformConfig> platforms = new List<PlatformConfig>();

    public void PublishPost(PlatformConfig platform, Post post) {
        // simple reach calculation (tunable)
        float baseReach = platform.followerCount * (0.01f + post.engagementRate);
        float qualityMult = 1f + (post.quality / 100f);
        int finalReach = Mathf.FloorToInt(baseReach * qualityMult * Random.Range(0.85f, 1.2f));
        int gained = Mathf.FloorToInt(finalReach * platform.followerConversion);
        platform.followerCount += gained;
        Debug.Log($"Published post on {platform.platformName}: reach={finalReach}, gained={gained}");
        // TODO: fire events to UI
    }
}
