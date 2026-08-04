using UnityEngine;
using UnityEngine.UI;

// Small debug UI to advance the in-game day for testing (useful when TimeSystem is not present)
public class DayAdvanceUI : MonoBehaviour {
    Canvas canvas;
    Button dayButton;
    Text infoText;
    int debugDay = 0;

    void Start() {
        BuildUI();
        // try to initialize debugDay from GameManager if available
        var gm = GameManager.Instance;
        if(gm != null) {
            try {
                // try property TimeOfDay via reflection-safe access
                var tProp = gm.GetType().GetProperty("TimeOfDay");
                if(tProp != null) debugDay = (int)tProp.GetValue(gm);
            } catch {}
        }
        UpdateInfo();
    }

    void BuildUI() {
        var canvasGO = new GameObject("DayAdvanceCanvas");
        canvas = canvasGO.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>(); DontDestroyOnLoad(canvasGO);

        var panel = new GameObject("DayPanel"); panel.transform.SetParent(canvas.transform,false); var rt = panel.AddComponent<RectTransform>(); rt.anchorMin = new Vector2(0.80f,0.92f); rt.anchorMax = new Vector2(0.98f,0.99f); rt.offsetMin = rt.offsetMax = Vector2.zero; var img = panel.AddComponent<Image>(); img.color = new Color(0f,0f,0f,0.6f);

        dayButton = CreateButton("DayBtn", panel.transform, "Advance Day"); dayButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.12f); dayButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f,0.88f);
        dayButton.onClick.AddListener(()=>{ AdvanceDay(); });

        infoText = CreateText("Info", panel.transform, "Day: 0"); infoText.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.02f); infoText.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f,0.12f);
    }

    void AdvanceDay() {
        var gm = GameManager.Instance;
        // Prefer calling GameManager's time advance if available
        bool handled = false;
        if(gm != null) {
            var method = gm.GetType().GetMethod("AdvanceDay");
            if(method != null) {
                method.Invoke(gm, null);
                handled = true;
            } else {
                // try property TimeOfDay
                var prop = gm.GetType().GetProperty("TimeOfDay");
                if(prop != null && prop.CanRead && prop.CanWrite) {
                    int cur = (int)prop.GetValue(gm);
                    prop.SetValue(gm, cur + 1);
                    handled = true;
                }
            }
        }

        if(!handled) {
            // fallback: increment local debugDay and call VideoManager.SimulateDay
            debugDay++;
            if(VideoManager.Instance != null) VideoManager.Instance.SimulateDay(debugDay);
            Telemetry.Emit("DebugDayAdvanced", $"day={debugDay}");
        } else {
            // if handled by GameManager, also ask VideoManager to simulate using new gm time
            int day = 0;
            if(gm != null) {
                var prop = gm.GetType().GetProperty("TimeOfDay");
                if(prop != null) day = (int)prop.GetValue(gm);
            }
            if(VideoManager.Instance != null) VideoManager.Instance.SimulateDay(day);
        }

        UpdateInfo();
    }

    void UpdateInfo() {
        int day = debugDay;
        var gm = GameManager.Instance;
        if(gm != null) {
            var prop = gm.GetType().GetProperty("TimeOfDay");
            if(prop != null) day = (int)prop.GetValue(gm);
        }
        infoText.text = $"Day: {day}";
    }

    Button CreateButton(string name, Transform parent, string label) { var go = new GameObject(name); go.transform.SetParent(parent,false); var img = go.AddComponent<Image>(); img.color = new Color(0.16f,0.5f,0.16f,1f); var b = go.AddComponent<Button>(); var t = CreateText(name+"_L", go.transform, label); t.alignment = TextAnchor.MiddleCenter; return b; }
    Text CreateText(string name, Transform parent, string text) { var go = new GameObject(name); go.transform.SetParent(parent,false); var t = go.AddComponent<Text>(); t.text = text; t.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize = 14; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter; return t; }
}
