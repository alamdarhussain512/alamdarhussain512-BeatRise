using UnityEngine;
using UnityEngine.UI;

// Simple runtime UI to interact with a Broker NPC for negotiation
public class BrokerUIManager : MonoBehaviour {
    public BrokerNPC broker;
    UndergroundMarketManager market;
    Canvas canvas;

    void Start() {
        market = UndergroundMarketManager.Instance ?? FindObjectOfType<UndergroundMarketManager>();
        if(broker == null) broker = FindObjectOfType<BrokerNPC>();
        BuildUI();
    }

    void BuildUI() {
        var canvasGO = new GameObject("BrokerCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);

        var panel = new GameObject("BrokerPanel");
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.55f, 0.05f);
        rt.anchorMax = new Vector2(0.95f, 0.45f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = panel.AddComponent<Image>(); img.color = new Color(0f,0f,0f,0.75f);

        var title = CreateText("Title", panel.transform, "Broker: " + (broker != null ? broker.brokerName : "Unknown"));
        title.rectTransform.anchorMin = new Vector2(0.02f, 0.82f); title.rectTransform.anchorMax = new Vector2(0.98f, 0.98f);

        if(market == null) { CreateText("Error", panel.transform, "Market not found"); return; }

        for(int i=0;i<market.offers.Count;i++) {
            var offer = market.offers[i];
            var yTop = 0.7f - i*0.22f;
            var name = CreateText($"OfferName_{i}", panel.transform, $"{offer.name} - ${offer.cost}");
            name.rectTransform.anchorMin = new Vector2(0.02f, yTop); name.rectTransform.anchorMax = new Vector2(0.7f, yTop+0.08f);
            var desc = CreateText($"OfferDesc_{i}", panel.transform, offer.description);
            desc.fontSize = 12; desc.rectTransform.anchorMin = new Vector2(0.02f, yTop-0.06f); desc.rectTransform.anchorMax = new Vector2(0.98f, yTop);
            var btn = CreateButton($"Negotiate_{i}", panel.transform, "Negotiate");
            btn.GetComponent<RectTransform>().anchorMin = new Vector2(0.72f, yTop); btn.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f, yTop+0.08f);
            int idx = i;
            btn.onClick.AddListener(() => OnNegotiateClicked(market.offers[idx].id));
        }
    }

    void OnNegotiateClicked(string offerId) {
        if(broker == null) { Debug.Log("No broker available"); return; }
        var offer = market.offers.Find(o => o.id == offerId);
        if(offer == null) return;
        var gm = GameManager.Instance;
        int negotiatedCost;
        var mod = broker.Negotiate(offer, gm != null ? gm.playerProfile.reputation : 50, out negotiatedCost);
        // Show negotiation result in console and allow immediate purchase for quick testing
        Debug.Log($"Negotiated offer: {mod.name} cost={negotiatedCost} success={mod.successChance:F2} detect={mod.detectionChance:F2}");
        // For quick flow: execute modified offer by temporarily altering offer values
        var backup = new UndergroundOffer {
            id = offer.id,
            name = offer.name,
            description = offer.description,
            type = offer.type,
            cost = offer.cost,
            successChance = offer.successChance,
            detectionChance = offer.detectionChance,
            followerBoost = offer.followerBoost,
            notorietyGain = offer.notorietyGain,
            fineOnDetection = offer.fineOnDetection
        };
        // apply modified values to offer and execute, then restore
        offer.cost = mod.cost;
        offer.successChance = mod.successChance;
        offer.detectionChance = mod.detectionChance;
        offer.followerBoost = mod.followerBoost;
        offer.notorietyGain = mod.notorietyGain;
        offer.fineOnDetection = mod.fineOnDetection;

        bool result = market.ExecuteOffer(offer.id);

        // restore original offer template (so market templates remain stable)
        offer.cost = backup.cost;
        offer.successChance = backup.successChance;
        offer.detectionChance = backup.detectionChance;
        offer.followerBoost = backup.followerBoost;
        offer.notorietyGain = backup.notorietyGain;
        offer.fineOnDetection = backup.fineOnDetection;

        Debug.Log($"Broker execution result: {result}");
        Telemetry.Emit("OfferNegotiated", $"offer={offerId};broker={broker.brokerName};result={result}");
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
