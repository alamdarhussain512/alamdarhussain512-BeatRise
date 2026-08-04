using UnityEngine;
using System.Collections.Generic;

// Very small analytics UI (OnGUI) to show a simple sparkline of views/day for selected video
public class VideoAnalyticsUI : MonoBehaviour {
    int selectedIndex = 0;
    Vector2 windowPos = new Vector2(10, 160);
    Rect windowRect = new Rect(10,160,320,160);

    void OnGUI() {
        windowRect = GUI.Window(99991, windowRect, DrawWindow, "Video Analytics");
    }

    void DrawWindow(int id) {
        var vm = VideoManager.Instance;
        if(vm == null) { GUILayout.Label("No VideoManager found"); GUI.DragWindow(); return; }
        if(vm.videos == null || vm.videos.Count == 0) { GUILayout.Label("No videos uploaded"); GUI.DragWindow(); return; }

        string[] titles = new string[vm.videos.Count];
        for(int i=0;i<titles.Length;i++) titles[i] = vm.videos[i].title;
        selectedIndex = GUILayout.SelectionGrid(selectedIndex, titles, 1);

        var v = vm.videos[Mathf.Clamp(selectedIndex,0,vm.videos.Count-1)];
        GUILayout.Label($"Total views: {v.totalViews}  •  Subs: {v.totalSubscribersGained}  •  Revenue: ${v.revenue:F1}");

        // draw sparkline
        var series = VideoAnalytics.GetViewsSeries(v);
        DrawSparkline(series, 280, 60);

        if(GUILayout.Button("Open Details")) {
            // try to bring VideoDetailUI into view by selecting same index if present
            var detail = FindObjectOfType<VideoDetailUI>();
            if(detail!=null) {
                var dd = detail.GetType().GetField("videoDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                // best effort: call RefreshVideos on detail if exists
                var method = detail.GetType().GetMethod("RefreshVideos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if(method!=null) method.Invoke(detail, null);
            }
        }

        GUI.DragWindow();
    }

    void DrawSparkline(List<int> values, int width, int height) {
        GUILayout.BeginHorizontal();
        Rect r = GUILayoutUtility.GetRect(width, height);
        if(values==null || values.Count==0) { GUI.Label(r, "No daily data yet"); GUILayout.EndHorizontal(); return; }

        // compute min/max
        int min = int.MaxValue; int max = 0;
        foreach(var v in values) { if(v<min) min=v; if(v>max) max=v; }
        if(min==int.MaxValue) min=0;
        if(max==min) max = min+1;

        // draw background
        GUI.Box(r, "");
        var prev = new Vector2(r.x, r.y + r.height);
        for(int i=0;i<values.Count;i++) {
            float t = (float)i / (values.Count-1);
            float x = Mathf.Lerp(r.x+4, r.xMax-4, t);
            float y = Mathf.Lerp(r.yMax-4, r.y+4, (values[i]-min) / (float)(max-min));
            var p = new Vector2(x,y);
            if(i>0) DrawLine(prev,p,Color.cyan,2f);
            prev = p;
        }
        GUILayout.EndHorizontal();
    }

    // Simple line draw using textures
    Texture2D _tex;
    void DrawLine(Vector2 a, Vector2 b, Color color, float width) {
        if(_tex==null) { _tex = new Texture2D(1,1); _tex.SetPixel(0,0,Color.white); _tex.Apply(); }
        Color old = GUI.color; GUI.color = color;
        Matrix4x4 matrix = GUI.matrix;
        float angle = Vector3.Angle(b-a, Vector2.right);
        if(a.y > b.y) angle = -angle;
        float len = (b-a).magnitude;
        GUIUtility.RotateAroundPivot(angle, a);
        GUI.DrawTexture(new Rect(a.x, a.y-width/2, len, width), _tex);
        GUI.matrix = matrix; GUI.color = old;
    }
}
