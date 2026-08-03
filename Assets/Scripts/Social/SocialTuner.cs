using UnityEngine;

// Simple runtime tuner for social balancing and risky platform parameters.
// Attach to a debug GameObject or leave in the scene; uses OnGUI for quick sliders.
public class SocialTuner : MonoBehaviour {
    public float tikTokVirality = Balancing.TikTokVirality;
    public float youTubeVirality = Balancing.YouTubeVirality;
    public float instagramVirality = Balancing.InstagramVirality;

    public float riskyBaseChance = Balancing.RiskyPlatformBaseChance;
    public float riskyFineMultiplier = Balancing.RiskyFineMultiplier;

    // Underground/broker tuning
    public float notorietyDetectionScale = Balancing.UndergroundNotorietyDetectionScale;
    public float brokerTrustMultiplier = Balancing.BrokerTrustCostMultiplier;

    // Simulation settings
    public int simulationIterations = 500;
    public enum SimulationMode { PublishOnly, OffersOnly, Mixed }
    public SimulationMode simMode = SimulationMode.Mixed;

    void OnGUI() {
        GUILayout.BeginArea(new Rect(10,10,360,360), "Social Tuner", GUI.skin.window);
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

        GUILayout.Space(8);
        GUILayout.Label("Underground / Broker tuning");
        notorietyDetectionScale = GUILayout.HorizontalSlider(notorietyDetectionScale, 0f, 0.02f);
        GUILayout.Label($"Notoriety detection scale: {notorietyDetectionScale:F4}");
        brokerTrustMultiplier = GUILayout.HorizontalSlider(brokerTrustMultiplier, 0.5f, 1.2f);
        GUILayout.Label($"Broker trust cost multiplier: {brokerTrustMultiplier:F2}");

        GUILayout.Space(8);
        GUILayout.Label("Simulation");
        GUILayout.Label($"Iterations: {simulationIterations}");
        simulationIterations = (int)GUILayout.HorizontalSlider(simulationIterations, 10, 5000);
        simMode = (SimulationMode)GUILayout.SelectionGrid((int)simMode, new string[] { "PublishOnly", "OffersOnly", "Mixed" }, 3);

        if(GUILayout.Button("Apply")) {
            Apply();
        }

        if(GUILayout.Button("Run Simulation")) {
            RunSimulation();
        }

        GUILayout.EndArea();
    }

    void Apply() {
        Balancing.TikTokVirality = tikTokVirality;
        Balancing.YouTubeVirality = youTubeVirality;
        Balancing.InstagramVirality = instagramVirality;
        Balancing.RiskyPlatformBaseChance = riskyBaseChance;
        Balancing.RiskyFineMultiplier = riskyFineMultiplier;
        Balancing.UndergroundNotorietyDetectionScale = notorietyDetectionScale;
        Balancing.BrokerTrustCostMultiplier = brokerTrustMultiplier;
        Debug.Log("SocialTuner: applied balancing values");
    }

    void RunSimulation() {
        var sim = FindObjectOfType<Simulator>();
        if(sim == null) {
            Debug.LogError("Simulator not found in scene. Add a Simulator GameObject with the Simulator component to run simulations.");
            return;
        }

        Debug.Log($"Starting simulation: mode={simMode} iterations={simulationIterations}");
        sim.RunSimulation(simulationIterations, simMode.ToString());
    }
}
