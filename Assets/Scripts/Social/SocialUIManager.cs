using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// This script builds a simple compose UI at runtime for the SocialHub scene
// Attach it to an empty GameObject in the SocialHub scene (or leave it on the SocialHub root)
// It depends on UnityEngine.UI being available in the project.
public class SocialUIManager : MonoBehaviour {
    SocialPlatformManager manager;

    Canvas canvas;
    Dropdown platformDropdown;
    InputField contentField;
    Slider qualitySlider;
    Text qualityValueText;
    Button publishButton;
    Text statusText;

    // Joke UI
    Button jokeButton;
    Text jokeText;

    void Start() {
        manager = SocialPlatformManager.Instance ?? FindObjectOfType<SocialPlatformManager>();

        // Ensure RandomJokeManager exists
        if(RandomJokeManager.Instance == null) {
            var go = new GameObject("RandomJokeManager");
            go.AddComponent<RandomJokeManager>();
        }

        BuildUI();
        RefreshPlatformList();
    }

    void BuildUI() {
        // Canvas
        var canvasGO = new GameObject("SocialCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        // Panel
        var panel = CreateUIObject("Panel", canvasGO.transform);
        var img = panel.AddComponent<Image>();
        img.color = new Color(0f,0f,0f,0.6f);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.55f);
        rt.anchorMax = new Vector2(0.95f, 0.95f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Platform dropdown label
        var pLabel = CreateText("PlatformLabel", panel.transform, "Platform:");
        pLabel.rectTransform.anchorMin = new Vector2(0.02f, 0.75f);
        pLabel.rectTransform.anchorMax = new Vector2(0.18f, 0.9f);

        // Platform dropdown
        platformDropdown = CreateDropdown("PlatformDropdown", panel.transform);
        platformDropdown.GetComponent<RectTransform>().anchorMin = new Vector2(0.20f, 0.75f);
        platformDropdown.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f, 0.9f);

        // Content label
        var cLabel = CreateText("ContentLabel", panel.transform, "Post Content:");
        cLabel.rectTransform.anchorMin = new Vector2(0.02f, 0.50f);
        cLabel.rectTransform.anchorMax = new Vector2(0.18f, 0.68f);

        // Content input field
        contentField = CreateInputField("ContentField", panel.transform);
        var cfRT = contentField.GetComponent<RectTransform>();
        cfRT.anchorMin = new Vector2(0.02f, 0.28f);
        cfRT.anchorMax = new Vector2(0.98f, 0.62f);

        // Quality label
        var qLabel = CreateText("QualityLabel", panel.transform, "Quality:");
        qLabel.rectTransform.anchorMin = new Vector2(0.02f, 0.14f);
        qLabel.rectTransform.anchorMax = new Vector2(0.18f, 0.24f);

        // Quality slider
        qualitySlider = CreateSlider("QualitySlider", panel.transform);
        var qsRT = qualitySlider.GetComponent<RectTransform>();
        qsRT.anchorMin = new Vector2(0.20f, 0.13f);
        qsRT.anchorMax = new Vector2(0.78f, 0.24f);
        qualitySlider.minValue = 0f;
        qualitySlider.maxValue = 100f;
        qualitySlider.value = 60f;
        qualitySlider.onValueChanged.AddListener((v) => { if(qualityValueText!=null) qualityValueText.text = Mathf.RoundToInt(v).ToString(); });

        // Quality value text
        qualityValueText = CreateText("QualityValue", panel.transform, Mathf.RoundToInt(qualitySlider.value).ToString());
        qualityValueText.rectTransform.anchorMin = new Vector2(0.80f, 0.13f);
        qualityValueText.rectTransform.anchorMax = new Vector2(0.98f, 0.24f);

        // Publish button
        publishButton = CreateButton("PublishButton", panel.transform, "Publish");
        var pbRT = publishButton.GetComponent<RectTransform>();
        pbRT.anchorMin = new Vector2(0.02f, 0.02f);
        pbRT.anchorMax = new Vector2(0.30f, 0.11f);
        publishButton.onClick.AddListener(OnPublishClicked);

        // Joke button
        jokeButton = CreateButton("JokeButton", panel.transform, "Get Joke");
        var jbRT = jokeButton.GetComponent<RectTransform>();
        jbRT.anchorMin = new Vector2(0.32f, 0.02f);
        jbRT.anchorMax = new Vector2(0.50f, 0.11f);
        jokeButton.onClick.AddListener(OnJokeClicked);

        // Status text
        statusText = CreateText("StatusText", panel.transform, "Ready to publish.");
        statusText.alignment = TextAnchor.MiddleLeft;
        statusText.rectTransform.anchorMin = new Vector2(0.52f, 0.02f);
        statusText.rectTransform.anchorMax = new Vector2(0.98f, 0.11f);

        // Joke display area (below the panel)
        var jokePanel = CreateUIObject("JokePanel", canvasGO.transform);
        var jpImg = jokePanel.AddComponent<Image>(); jpImg.color = new Color(0f,0f,0f,0.5f);
        var jprt = jokePanel.GetComponent<RectTransform>();
        jprt.anchorMin = new Vector2(0.05f, 0.40f);
        jprt.anchorMax = new Vector2(0.95f, 0.53f);
        jprt.offsetMin = jprt.offsetMax = Vector2.zero;

        jokeText = CreateText("JokeText", jokePanel.transform, "Press 'Get Joke' to fetch a random joke.");
        jokeText.fontSize = 14;
        jokeText.alignment = TextAnchor.UpperLeft;
    }

    void RefreshPlatformList() {
        platformDropdown.ClearOptions();
        if(manager == null) { platformDropdown.options.Add(new Dropdown.OptionData("<no platforms>")); return; }
        var names = manager.platforms.Select(p => p.platformName + " (" + p.followerCount + ")").ToList();
        platformDropdown.AddOptions(names);
    }

    void OnPublishClicked() {
        if(manager == null) { statusText.text = "No SocialPlatformManager found"; return; }
        if(platformDropdown.options.Count == 0) { statusText.text = "No platforms configured"; return; }

        var idx = platformDropdown.value;
        var platform = manager.platforms[idx];
        var content = contentField.text;
        var quality = qualitySlider.value;

        var post = new Post { id = System.Guid.NewGuid().ToString(), content = content, quality = quality, engagementRate = Mathf.Clamp(0.02f + (content.Length/200f), 0.01f, 0.25f) };

        var res = manager.PublishPost(platform, post);

        // apply effects to player profile: fans gained + XP + small money for engagement
        if(GameManager.Instance != null) {
            var gm = GameManager.Instance;
            gm.playerProfile.fans += res.gained;
            gm.playerProfile.AddXP(Mathf.Max(5, Mathf.FloorToInt(res.reach/100f)));
            gm.AddMoney(Mathf.FloorToInt(res.reach * 0.01f));
            gm.Save();
        }

        statusText.text = $"Published to {platform.platformName}: reach={res.reach}, newFollowers={res.gained}. Reputation={GameManager.Instance.playerProfile.reputation}";

        // refresh dropdown display (show updated follower counts)
        RefreshPlatformList();
    }

    void OnJokeClicked() {
        if(RandomJokeManager.Instance == null) { statusText.text = "Joke manager not found"; return; }
        RandomJokeManager.Instance.RequestJoke(jokeText);
    }

    // Helper factory methods
    GameObject CreateUIObject(string name, Transform parent) {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,0);
        rt.anchorMax = new Vector2(1,1);
        rt.sizeDelta = Vector2.zero;
        return go;
    }

    Text CreateText(string name, Transform parent, string text) {
        var go = CreateUIObject(name, parent);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 18;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleLeft;
        return t;
    }

    Dropdown CreateDropdown(string name, Transform parent) {
        var go = CreateUIObject(name, parent);
        var dd = go.AddComponent<Dropdown>();
        // minimal visuals
        var img = go.AddComponent<Image>();
        img.color = Color.white * 0.1f;
        dd.targetGraphic = img;
        dd.captionText = CreateText(name+"_Caption", go.transform, "");
        dd.template = CreateDropdownTemplate(go.transform);
        return dd;
    }

    RectTransform CreateDropdownTemplate(Transform root) {
        // Build a usable dropdown template hierarchy
        var templateGO = CreateUIObject("Template", root);
        var rt = templateGO.AddComponent<RectTransform>();
        // size and anchors
        rt.anchorMin = new Vector2(0,0);
        rt.anchorMax = new Vector2(1,0.6f);
        rt.pivot = new Vector2(0.5f,1f);

        // Background
        var bg = templateGO.AddComponent<Image>();
        bg.color = new Color(0f,0f,0f,0.9f);

        // Viewport (ScrollRect viewport)
        var viewport = CreateUIObject("Viewport", templateGO.transform);
        var vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(1,1,1,0.01f);
        var mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content (will hold items)
        var content = CreateUIObject("Content", viewport.transform);
        var contentRT = content.GetComponent<RectTransform>();
        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ScrollRect on template
        var scroll = templateGO.AddComponent<ScrollRect>();
        scroll.content = contentRT;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.horizontal = false;

        // Item template
        var item = CreateUIObject("Item", content.transform);
        var itemImg = item.AddComponent<Image>();
        itemImg.color = new Color(1,1,1,0.05f);
        var toggle = item.AddComponent<Toggle>();
        toggle.transition = Selectable.Transition.ColorTint;
        var label = CreateText("ItemLabel", item.transform, "Option");
        label.alignment = TextAnchor.MiddleLeft;
        label.rectTransform.anchorMin = new Vector2(0,0);
        label.rectTransform.anchorMax = new Vector2(1,1);

        // Hide item by default (Dropdown expects template to be inactive)
        templateGO.SetActive(false);

        return rt;
    }

    InputField CreateInputField(string name, Transform parent) {
        var go = CreateUIObject(name, parent);
        var img = go.AddComponent<Image>();
        img.color = Color.white * 0.1f;
        var input = go.AddComponent<InputField>();
        var text = CreateText(name+"_Text", go.transform, "");
        input.textComponent = text;
        text.alignment = TextAnchor.UpperLeft;
        return input;
    }

    Slider CreateSlider(string name, Transform parent) {
        var go = CreateUIObject(name, parent);
        var slider = go.AddComponent<Slider>();
        var bg = CreateUIObject("Background", go.transform).AddComponent<Image>();
        bg.color = Color.gray;
        var fill = CreateUIObject("Fill", go.transform).AddComponent<Image>();
        fill.color = Color.cyan;
        slider.fillRect = fill.rectTransform;
        return slider;
    }

    Button CreateButton(string name, Transform parent, string label) {
        var go = CreateUIObject(name, parent);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f,0.6f,0.2f,1f);
        var btn = go.AddComponent<Button>();
        var txt = CreateText(name+"_Label", go.transform, label);
        txt.alignment = TextAnchor.MiddleCenter;
        return btn;
    }
}
