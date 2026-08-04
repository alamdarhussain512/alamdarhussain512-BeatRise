using System.Collections.Generic;
using UnityEngine;

// Simple analytics helpers for videos
public static class VideoAnalytics {
    // Returns views per day series for the given video
    public static List<int> GetViewsSeries(Video v) {
        var series = new List<int>();
        if (v == null) return series;
        foreach (var s in v.dayStats) series.Add(s.views);
        return series;
    }

    public static int TotalViews(Video v) {
        return v != null ? v.totalViews : 0;
    }

    public static float AverageDailyViews(Video v) {
        if (v == null || v.dayStats == null || v.dayStats.Count == 0) return 0f;
        float sum = 0f;
        foreach (var s in v.dayStats) sum += s.views;
        return sum / v.dayStats.Count;
    }
}
