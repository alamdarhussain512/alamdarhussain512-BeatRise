using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// UI to view video details and moderate comments
public class VideoDetailUI : MonoBehaviour {
    Canvas canvas;
    Dropdown videoDropdown;
    Transform commentRoot;
    Button moderateAllBtn;
    Text infoText;

    void Start() {
        BuildUI();
        RefreshVideos();
    }

    void BuildUI() {
        var canvasGO = new GameObject("VideoDetailCanvas");
        canvas = canvasGO.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>(); DontDestroyOnLoad(canvasGO);

        var panel = new GameObject("DetailPanel"); panel.transform.SetParent(canvas.transform,false); var rt = panel.AddComponent<RectTransform>(); rt.anchorMin = new Vector2(0.05f,0.05f); rt.anchorMax = new Vector2(0.45f,0.45f); rt.offsetMin = rt.offsetMax = Vector2.zero; var img = panel.AddComponent<Image>(); img.color = new Color(0f,0f,0f,0.85f);

        var title = CreateText("Title", panel.transform, "Video Details & Comments"); title.rectTransform.anchorMin = new Vector2(0.02f,0.82f); title.rectTransform.anchorMax = new Vector2(0.98f,0.98f);

        videoDropdown = CreateDropdown("VideoSelect", panel.transform); videoDropdown.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.7f); videoDropdown.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f,0.8f);
        videoDropdown.onValueChanged.AddListener((i)=> { PopulateComments(); });

        var listRoot = new GameObject("CommentsRoot"); listRoot.transform.SetParent(panel.transform,false); commentRoot = listRoot.transform; var listRT = listRoot.AddComponent<RectTransform>(); listRT.anchorMin = new Vector2(0.02f,0.12f); listRT.anchorMax = new Vector2(0.98f,0.68f); listRT.offsetMin = listRT.offsetMax = Vector2.zero;

        moderateAllBtn = CreateButton("ModerateAll", panel.transform, "Auto-moderate negatives"); moderateAllBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f,0.02f); moderateAllBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.48f,0.10f);
        moderateAllBtn.onClick.AddListener(()=>{ AutoModerate(); PopulateComments(); });

        infoText = CreateText("Info", panel.transform, "Select a video to view comments."); infoText.rectTransform.anchorMin = new Vector2(0.52f,0.02f); infoText.rectTransform.anchorMax = new Vector2(0.98f,0.10f);
    }

    void RefreshVideos() {
        var vm = VideoManager.Instance ?? FindObjectOfType<VideoManager>();
        videoDropdown.ClearOptions();
        if(vm==null || vm.videos.Count==0) { videoDropdown.options.Add(new Dropdown.OptionData("<no videos>")); return; }
        foreach(var v in vm.videos) videoDropdown.options.Add(new Dropdown.OptionData(v.title));
        videoDropdown.value = 0; videoDropdown.RefreshShownValue();
        PopulateComments();
    }

    void PopulateComments() {
        foreach(Transform c in commentRoot) Destroy(c.gameObject);
        var vm = VideoManager.Instance; if(vm==null || vm.videos.Count==0) return;
        var v = vm.videos[Mathf.Clamp(videoDropdown.value,0,vm.videos.Count-1)];
        infoText.text = $"Views: {v.totalViews} • Subs: {v.totalSubscribersGained} • Monetized: {v.isMonetized}";
        if(v.comments==null || v.comments.Count==0) { var t = CreateText("NoC","","No comments yet"); t.transform.SetParent(commentRoot,false); return; }
        foreach(var c in v.comments.ToList()) {
            var item = new GameObject("C"); item.transform.SetParent(commentRoot,false);
            var txt = CreateText("T", item.transform, $"{c.author}: {c.text} [{c.sentiment}]"); txt.fontSize = 14;
            var delBtn = CreateButton("Del", item.transform, "Delete"); delBtn.onClick.AddListener(()=>{ DeleteComment(v,c); PopulateComments(); });
            var repBtn = CreateButton("Reply", item.transform, "Reply"); repBtn.onClick.AddListener(()=>{ ReplyToComment(v,c); PopulateComments(); });
        }
    }

    void DeleteComment(Video v, Comment c) {
        var gm = GameManager.Instance;
        if(gm != null) {
            // cost energy to moderate
            gm.playerProfile.energy = Mathf.Max(0f, gm.playerProfile.energy - 2f);
            // small reputation gain for removing toxic content
            gm.playerProfile.reputation = Mathf.Clamp(gm.playerProfile.reputation + 1, 0, 100);
            gm.Save();
        }
        v.comments.Remove(c);
        Telemetry.Emit("CommentDeleted", $"video={v.id};comment={c.id};sentiment={c.sentiment}");
    }

    void ReplyToComment(Video v, Comment c) {
        var gm = GameManager.Instance;
        // adding a reply costs energy but can convert negative into neutral
        if(gm != null) gm.playerProfile.energy = Mathf.Max(0f, gm.playerProfile.energy - 1f);
        var r = new Comment(); r.author = gm!=null?gm.playerProfile.playerName:"You"; r.text = "Thanks for the feedback!"; r.sentiment = CommentSentiment.Positive;
        v.comments.Add(r);
        Telemetry.Emit("CommentReplied", $"video={v.id};comment={c.id}");
    }

    void AutoModerate() {
        var vm = VideoManager.Instance; if(vm==null) return;
        var v = vm.videos[Mathf.Clamp(videoDropdown.value,0,vm.videos.Count-1)];
        var negatives = v.comments.Where(x=>x.sentiment==CommentSentiment.Negative).ToList();
        var gm = GameManager.Instance;
        int removed = 0;
        foreach(var n in negatives) {
            v.comments.Remove(n); removed++;
        }
        if(gm!=null) { gm.playerProfile.energy = Mathf.Max(0f, gm.playerProfile.energy - removed*2); gm.playerProfile.reputation = Mathf.Clamp(gm.playerProfile.reputation + removed, 0,100); gm.Save(); }
        Telemetry.Emit("AutoModerate", $"video={v.id};removed={removed}");
    }

    // helper factories
    Dropdown CreateDropdown(string name, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent,false); var dd = go.AddComponent<Dropdown>(); dd.targetGraphic = go.AddComponent<Image>(); dd.captionText = CreateText(name+"_cap", go.transform, ""); return dd; }
    Button CreateButton(string name, Transform parent, string label) { var go = new GameObject(name); go.transform.SetParent(parent,false); var img = go.AddComponent<Image>(); img.color = new Color(0.2f,0.6f,0.2f,1f); var b = go.AddComponent<Button>(); var t = CreateText(name+"_L", go.transform, label); t.alignment = TextAnchor.MiddleCenter; return b; }
    Text CreateText(string name, Transform parent, string text) { var go = new GameObject(name); if(parent!=null) go.transform.SetParent(parent,false); var t = go.AddComponent<Text>(); t.text = text; t.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize = 16; t.color = Color.white; t.alignment = TextAnchor.MiddleLeft; return t; }
}
