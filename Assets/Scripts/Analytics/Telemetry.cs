using UnityEngine;

// Simple telemetry emitter for in-game tuning. Emits to Console for now.
public static class Telemetry {
    public static void Emit(string eventName, string payload = "") {
        Debug.Log($"[Telemetry] {eventName}: {payload}");
        // future: write to file or remote endpoint for analysis
    }
}
