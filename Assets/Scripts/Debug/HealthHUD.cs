using UnityEngine;

// Simple on-screen health/fps HUD using OnGUI for easy debugging/testing.
public class HealthHUD : MonoBehaviour {
    public int fontSize = 14;
    public Rect windowRect = new Rect(8,8,220,90);
    bool visible = true;

    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    void OnGUI() {
        GUI.skin.label.fontSize = fontSize;
        windowRect = GUI.Window(123456, windowRect, DrawWindow, "Health");
    }

    void DrawWindow(int id) {
        float fps = 1f / Time.unscaledDeltaTime;
        float currentMs = Time.unscaledDeltaTime * 1000f;
        string s = $"FPS: {fps:F1}\nFrame: {currentMs:F1} ms\nAvg: {StabilityMonitor.Instance?.avgFrameMs:F1} ms\nMax: {StabilityMonitor.Instance?.maxFrameMs:F1} ms";
        if(StabilityMonitor.Instance != null) s += $"\nSlowFrames: {StabilityMonitor.Instance.slowFrameCount}";
        GUILayout.Label(s);
        if(GUILayout.Button(visible?"Hide":"Show", GUILayout.Width(80))) visible = !visible;
        GUI.DragWindow();
    }
}
