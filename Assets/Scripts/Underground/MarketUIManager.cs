using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// Runtime Market UI for browsing underground offers
public class MarketUIManager : MonoBehaviour {
    UndergroundMarketManager market;
    Canvas canvas;

    void Start() {
        market = UndergroundMarketManager.Instance ?? FindObjectOfType<UndergroundMarketManager>();
        BuildUI();
    }

    void BuildUI() {
        var canvasGO = new GameObject("MarketCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        var panel = new GameObject("MarketPanel");
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.05f);
        rt.anchorMax = new Vector2(0.45f, 0.45f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = panel.AddComponent<Image>(); img.color = new Color(0f,0f,0f,0.75f);

        var title = CreateText("Title", panel.transform, "Underground Market");
        title.rectTransform.anchorMin = new Vector2(0.02f, 0.82f); title.rectTransform.anchorMax = new Vector2(0.98f, 0.98f);

        if(market == null) { CreateText("Error", panel.transform, "Market not found"); return; }

        for(int i=0;i<market.offers.Count;i++) {
            var offer = market.offers[i];
            var yTop = 0.7f - i*0.22f;
            var name = CreateText($"OfferName_{i}", panel.transform, $"{offer.name} - ${offer.cost}");
            name.rectTransform.anchorMin = new Vector2(0.02f, yTop); name.rectTransform.anchorMax = new Vector2(0.7f, yTop+0.08f);
            var desc = CreateText($"OfferDesc_{i}", panel.transform, offer.description);
            desc.fontSize = 12; desc.rectTransform.anchorMin = new Vector2(0.02f, yTop-0.06f); desc.rectTransform.anchorMax = new Vector2(0.98f, yTop);
            var btn = CreateButton($"Buy_{i}", panel.transform, "Buy");
            btn.GetComponent<RectTransform>().anchorMin = new Vector2(0.72f, yTop); btn.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f, yTop+0.08f);
            int idx = i;
            btn.onClick.AddListener(() => OnBuyClicked(market.offers[idx].id));
        }
    }

    void OnBuyClicked(string offerId) {
        // Confirmation simple dialog (blocking) - for now use Debug.Log and execute
        Debug.Log($"Attempting to buy offer {offerId}");
        var success = market.ExecuteOffer(offerId);
        Debug.Log($"Offer {offerId} result: {success}");
    }

    Text CreateText(string name, Transform parent, string text) {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>(); t.text = text; t.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize = 16; t.color = Color.white; t.alignment = TextAnchor.UpperLeft;
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = new Vector2(0,0); rt.anchorMax = new Vector2(1,1); rt.sizeDelta = Vector2.zero;
        return t;
    }

    Button CreateButton(string name, Transform parent, string label) {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = new Color(0.2f,0.6f,0.2f,1f);
        var btn = go.AddComponent<Button>();
        var txt = CreateText(name+"_Label", go.transform, label); txt.alignment = TextAnchor.MiddleCenter; txt.fontSize = 14;
        return btn;
    }
}
