using System;
using System.Collections.Generic;
using UnityEngine;

// Monitors frame timings and reports spikes / long stalls.
// Attach to a persistent GameObject (e.g., DebugTools) or let it auto-create itself.
public class StabilityMonitor : MonoBehaviour {
    public static StabilityMonitor Instance;

    [Tooltip("Frame time (ms) above which a frame is considered slow")]
    public float slowFrameThresholdMs = 40f;
    [Tooltip("Number of consecutive slow frames to consider the app unstable")]
    public int consecutiveSlowThreshold = 8;
    [Tooltip("If true, StabilityMonitor will log spikes to telemetry/console")]
    public bool emitTelemetry = true;

    public float currentFrameMs { get; private set; }
    public float maxFrameMs { get; private set; }
    public float avgFrameMs { get; private set; }
    public int slowFrameCount { get; private set; }
    public int consecutiveSlowFrames { get; private set; }

    Queue<float> recentFrames = new Queue<float>();
    int sampleSize = 120; // average over last N frames

    void Awake() {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update() {
        currentFrameMs = Time.unscaledDeltaTime * 1000f;
        recentFrames.Enqueue(currentFrameMs);
        if(recentFrames.Count > sampleSize) recentFrames.Dequeue();

        // compute stats
        float sum = 0f; maxFrameMs = 0f; slowFrameCount = 0;
        foreach(var v in recentFrames) { sum += v; if(v > maxFrameMs) maxFrameMs = v; if(v > slowFrameThresholdMs) slowFrameCount++; }
        avgFrameMs = recentFrames.Count > 0 ? sum / recentFrames.Count : 0f;

        if(currentFrameMs > slowFrameThresholdMs) consecutiveSlowFrames++; else consecutiveSlowFrames = 0;

        if(consecutiveSlowFrames >= consecutiveSlowThreshold) {
            if(emitTelemetry) Telemetry.Emit("StabilitySpike", $"consecutive={consecutiveSlowFrames};currentMs={currentFrameMs:F1};avgMs={avgFrameMs:F1};maxMs={maxFrameMs:F1}");
            Debug.LogWarning($"[StabilityMonitor] Spike detected: consecutive slow frames = {consecutiveSlowFrames}, currentFrameMs={currentFrameMs:F1}ms, avg={avgFrameMs:F1}ms");
            // reset counter after emitting to avoid noisy repeats
            consecutiveSlowFrames = 0;
        }
    }

    // Quick helper used by stress testers: should we throttle work this frame?
    public bool ShouldThrottle() {
        // Throttle if the rolling average or max exceed safe limits
        if(maxFrameMs > slowFrameThresholdMs * 2f) return true;
        if(avgFrameMs > slowFrameThresholdMs * 1.5f) return true;
        return false;
    }
}
