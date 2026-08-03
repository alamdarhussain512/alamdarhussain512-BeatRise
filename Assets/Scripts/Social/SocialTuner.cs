using UnityEngine;

// Simple runtime tuner for social balancing and risky platform parameters.
// Attach to a debug GameObject or leave in the scene; uses OnGUI for quick sliders.
public class SocialTuner : MonoBehaviour {
    public float tikTokVirality = Balancing.TikTokVirality;
    public float youTubeVirality = Balancing.YouTubeVirality;
    public float instagramVirality = Balancing.InstagramVirality;

    public float riskyBaseChance = Balancing.RiskyPlatformBaseChance;
    public float riskyFineMultiplier = Balancing.RiskyFineMultiplier;

    void OnGUI() {
        GUILayout.BeginArea(new Rect(10,10,320,220), "Social Tuner", GUI.skin.window);
        GUILayout.Label("Platform virality");
        tikTokVirality = GUILayout.HorizontalSlider(tikTokVirality, 0.5f, 4.0f);
        GUILayout.Label($"Tok: {tikTokVirality:F2}");
        youTubeVirality = GUILayout.HorizontalSlider(youTubeVirality, 0.5f, 2.0f);
        GUILayout.Label($"Tube: {youTubeVirality:F2}");
        instagramVirality = GUILayout.HorizontalSlider(instagramVirality, 0.5f, 2.0f);
        GUILayout.Label($"Gram: {instagramVirality:F2}");

        GUILayout.Space(8);
        GUILayout.Label("Risk tuning");
        riskyBaseChance = GUILayout.HorizontalSlider(riskyBaseChance, 0f, 0.9f);
        GUILayout.Label($"Base risk chance: {riskyBaseChance:F2}");
        riskyFineMultiplier = GUILayout.HorizontalSlider(riskyFineMultiplier, 0f, 0.5f);
        GUILayout.Label($"Fine multiplier: {riskyFineMultiplier:F2}");

        if(GUILayout.Button("Apply")) {
            Apply();
        }

        GUILayout.EndArea();
    }

    void Apply() {
        Balancing.TikTokVirality = tikTokVirality;
        Balancing.YouTubeVirality = youTubeVirality;
        Balancing.InstagramVirality = instagramVirality;
        Balancing.RiskyPlatformBaseChance = riskyBaseChance;
        Balancing.RiskyFineMultiplier = riskyFineMultiplier;
        Debug.Log("SocialTuner: applied balancing values");
    }
}
