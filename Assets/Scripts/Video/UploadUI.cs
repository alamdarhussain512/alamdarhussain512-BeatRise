using UnityEngine;
using UnityEngine.UI;

// Runtime upload UI for videos (simple). Attach to a GameObject or let developer create it in scene.
public class UploadUI : MonoBehaviour {
    Canvas canvas;
    InputField titleField;
    InputField descField;
    Slider qualitySlider;
    Text qualityValue;
    Button uploadButton;
    Toggle monetizeToggle;

    void Start() {
        BuildUI();
    }

    void BuildUI() {
        var canvasGO = new GameObject("UploadCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        var panel = new GameObject("UploadPanel"); panel.transform.SetParent(canvas.transform, false);
        var rt = panel.AddComponent<RectTransform>(); rt.anchorMin = new Vector2(0.05f, 0.05f); rt.anchorMax = new Vector2(0.45f, 0.45f); rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = panel.AddComponent<Image>(); img.color = new Color(0f,0f,0f,0.8f);

        titleField = CreateInputField("Title", panel.transform, "Video Title"); titleField.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.75f); titleField.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f,0.9f);
        descField = CreateInputField("Desc", panel.transform, "Short description"); descField.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.50f); descField.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f,0.72f);

        qualitySlider = CreateSlider("Quality", panel.transform); qualitySlider.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.30f); qualitySlider.GetComponent<RectTransform>().anchorMax = new Vector2(0.78f,0.40f);
        qualitySlider.minValue = 0; qualitySlider.maxValue = 100; qualitySlider.value = 60;
        qualityValue = CreateText("QualityVal", panel.transform, Mathf.RoundToInt(qualitySlider.value).ToString()); qualityValue.GetComponent<RectTransform>().anchorMin = new Vector2(0.80f,0.30f); qualityValue.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f,0.40f);
        qualitySlider.onValueChanged.AddListener((v)=> { qualityValue.text = Mathf.RoundToInt(v).ToString(); });

        monetizeToggle = CreateToggle("Monetize", panel.transform, "Monetize?"); monetizeToggle.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.18f); monetizeToggle.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,0.26f);

        uploadButton = CreateButton("UploadBtn", panel.transform, "Upload"); uploadButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.03f); uploadButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.48f,0.14f);
        uploadButton.onClick.AddListener(OnUploadClicked);
    }

    void OnUploadClicked() {
        string title = titleField.text;
        string desc = descField.text;
        int length = 180; // default
        float quality = qualitySlider.value;
        bool monetize = monetizeToggle.isOn;

        var vm = VideoManager.Instance ?? FindObjectOfType<VideoManager>();
        if(vm == null) { Debug.LogWarning("VideoManager not found"); return; }

        var v = vm.UploadVideo(title, desc, length, quality, monetize);
        Debug.Log($"Uploaded video {v.title} (id={v.id})");
    }

    // small helpers to create UI elements
    GameObject CreateUIObject(string name, Transform parent) {
        var go = new GameObject(name); go.transform.SetParent(parent, false); var rt = go.AddComponent<RectTransform>(); rt.anchorMin = new Vector2(0,0); rt.anchorMax = new Vector2(1,1); rt.sizeDelta = Vector2.zero; return go; }
    Text CreateText(string name, Transform parent, string text) { var go = CreateUIObject(name,parent); var t = go.AddComponent<Text>(); t.text=text; t.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize=16; t.color=Color.white; t.alignment=TextAnchor.MiddleLeft; return t; }
    InputField CreateInputField(string name, Transform parent, string placeholder){ var go = CreateUIObject(name,parent); var img = go.AddComponent<Image>(); img.color = Color.white * 0.05f; var input = go.AddComponent<InputField>(); var text = CreateText(name+"_Text", go.transform, ""); input.textComponent = text; var ph = CreateText(name+"_PH", go.transform, placeholder); ph.color = Color.gray; return input; }
    Slider CreateSlider(string name, Transform parent){ var go = CreateUIObject(name,parent); var s = go.AddComponent<Slider>(); return s; }
    Button CreateButton(string name, Transform parent, string label){ var go = CreateUIObject(name,parent); var img = go.AddComponent<Image>(); img.color = new Color(0.2f,0.6f,0.2f,1f); var b = go.AddComponent<Button>(); var t = CreateText(name+"_Label", go.transform, label); t.alignment = TextAnchor.MiddleCenter; return b; }
    Toggle CreateToggle(string name, Transform parent, string label){ var go = CreateUIObject(name,parent); var t = CreateText(name+"_Label", go.transform, label); var toggle = go.AddComponent<Toggle>(); return toggle; }
}
