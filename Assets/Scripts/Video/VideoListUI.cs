using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// Simple runtime video list UI to inspect uploaded videos. Minimal features for Phase 1.
public class VideoListUI : MonoBehaviour {
    Canvas canvas;
    Transform contentRoot;

    void Start() {
        BuildUI();
        Refresh();
    }

    void BuildUI() {
        var canvasGO = new GameObject("VideoListCanvas");
        canvas = canvasGO.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>(); DontDestroyOnLoad(canvasGO);

        var panel = new GameObject("VideoListPanel"); panel.transform.SetParent(canvas.transform,false); var rt = panel.AddComponent<RectTransform>(); rt.anchorMin = new Vector2(0.55f,0.05f); rt.anchorMax = new Vector2(0.95f,0.45f); rt.offsetMin = rt.offsetMax = Vector2.zero; var img = panel.AddComponent<Image>(); img.color = new Color(0f,0f,0f,0.8f);

        var title = CreateText("Title", panel.transform, "Videos"); title.rectTransform.anchorMin = new Vector2(0.02f,0.82f); title.rectTransform.anchorMax = new Vector2(0.98f,0.98f);

        var listRoot = new GameObject("ListRoot"); listRoot.transform.SetParent(panel.transform,false); contentRoot = listRoot.transform; var listRT = listRoot.AddComponent<RectTransform>(); listRT.anchorMin = new Vector2(0.02f,0.02f); listRT.anchorMax = new Vector2(0.98f,0.78f); listRT.offsetMin = listRT.offsetMax = Vector2.zero;
    }

    void Refresh() {
        foreach(Transform c in contentRoot) Destroy(c.gameObject);
        var vm = VideoManager.Instance ?? FindObjectOfType<VideoManager>();
        if(vm == null) { var t = CreateText("Empty", contentRoot, "No VideoManager found"); return; }

        foreach(var v in vm.videos.OrderByDescending(x=>x.uploadDay)) {
            var item = new GameObject("Item"); item.transform.SetParent(contentRoot,false);
            var t = CreateText("Label", item.transform, $"{v.title} — views: {v.totalViews} • subs: {v.totalSubscribersGained} • rev: ${v.revenue:F1}"); t.fontSize = 14;
            // Monetize toggle
            var btnGO = new GameObject("MonetizeBtn"); btnGO.transform.SetParent(item.transform,false); var btn = btnGO.AddComponent<Button>(); btnGO.AddComponent<Image>().color = v.isMonetized?new Color(0.1f,0.6f,0.2f,1f):new Color(0.6f,0.6f,0.6f,1f);
            var btxt = CreateText("Btxt", btnGO.transform, v.isMonetized?"Monetized":"Monetize"); btxt.alignment = TextAnchor.MiddleCenter;
            string id = v.id;
            btn.onClick.AddListener(()=>{ ToggleMonetize(id); Refresh(); });
        }
    }

    void ToggleMonetize(string id){ var vm = VideoManager.Instance; if(vm==null) return; var v = vm.GetVideo(id); if(v==null) return; v.isMonetized = !v.isMonetized; var gm = GameManager.Instance; if(gm!=null){ var snap = gm.playerProfile.videos.Find(x=>x.id==v.id); if(snap!=null) snap.isMonetized = v.isMonetized; gm.Save(); } }

    Text CreateText(string name, Transform parent, string text) { var go = new GameObject(name); go.transform.SetParent(parent,false); var t = go.AddComponent<Text>(); t.text=text; t.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize=16; t.color=Color.white; t.alignment=TextAnchor.MiddleLeft; return t; }
}
