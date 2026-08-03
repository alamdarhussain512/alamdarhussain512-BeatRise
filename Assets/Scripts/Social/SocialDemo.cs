using UnityEngine;

// Simple debug helper to publish posts from the Inspector during development
public class SocialDemo : MonoBehaviour {
    public SocialPlatformManager manager;
    public int platformIndex = 0;
    public Post demoPost = new Post { id = "demo", content = "Check out my new track!", quality = 60f, engagementRate = 0.05f };

    void Start() {
        if(manager == null) manager = FindObjectOfType<SocialPlatformManager>();
    }

    [ContextMenu("Publish Demo Post")]
    public void PublishDemo() {
        if(manager == null) { Debug.LogError("SocialPlatformManager not found"); return; }
        if(manager.platforms.Count == 0) { Debug.LogError("No platforms configured"); return; }
        var platform = manager.platforms[Mathf.Clamp(platformIndex, 0, manager.platforms.Count-1)];
        var res = manager.PublishPost(platform, demoPost);
        Debug.Log($"Demo publish to {platform.platformName}: reach={res.reach}, gained={res.gained}");
    }
}
