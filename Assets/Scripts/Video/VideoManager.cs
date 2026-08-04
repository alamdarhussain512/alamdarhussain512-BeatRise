using System.Collections.Generic;
using UnityEngine;

// Manager for video uploads, daily simulation, monetization and persistence hooks
public class VideoManager : MonoBehaviour {
    public static VideoManager Instance;

    public List<Video> videos = new List<Video>();

    void Awake() {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        // Load from profile if available
        var gm = GameManager.Instance;
        if(gm != null && gm.playerProfile != null && gm.playerProfile.videos != null) {
            // Rehydrate lightweight snapshots into Video entries (basic)
            foreach(var snap in gm.playerProfile.videos) {
                var v = new Video();
                v.id = snap.id;
                v.title = snap.title;
                v.uploadDay = snap.uploadDay;
                v.totalViews = snap.totalViews;
                v.isMonetized = snap.isMonetized;
                videos.Add(v);
            }
        }
    }

    // Call to create/upload a video. Returns the created Video object.
    public Video UploadVideo(string title, string description, int lengthSeconds, float quality, bool monetize) {
        var gm = GameManager.Instance;
        var day = gm != null ? gm.TimeOfDay : 0; // fallback
        var v = new Video {
            title = title,
            description = description,
            lengthSeconds = lengthSeconds,
            quality = Mathf.Clamp(quality, 0f, 100f),
            uploadDay = day,
            isMonetized = monetize
        };
        videos.Add(v);

        // Persist lightweight snapshot to profile
        if(gm != null && gm.playerProfile != null) {
            var snap = new VideoSnapshot { id = v.id, title = v.title, uploadDay = v.uploadDay, totalViews = v.totalViews, isMonetized = v.isMonetized };
            gm.playerProfile.videos.Add(snap);
            gm.Save();
        }

        Telemetry.Emit("VideoUploaded", $"id={v.id};title={v.title};quality={v.quality}");
        return v;
    }

    // Simulate per-day updates for all videos. currentDay is the game's day index.
    // This method is intentionally not extremely heavy; it does simple per-video math.
    public void SimulateDay(int currentDay) {
        if(videos == null || videos.Count == 0) return;
        var gm = GameManager.Instance;

        foreach(var v in videos) {
            int age = v.DaysSinceUpload(currentDay);

            // view curve: initial surge then decay
            float baseReach = 200f;
            float qualityFactor = Mathf.Lerp(0.5f, 2.0f, v.quality / 100f);
            float ageDecay = 1f / (1f + age * 0.12f); // long tail

            // platform influence (approx): better reputation increases organic reach
            float repBoost = 1f + (gm != null ? (gm.playerProfile.reputation - 50f) / 200f : 0f);

            // compute expected views
            float expected = baseReach * qualityFactor * ageDecay * repBoost * Balancing.VideoBaseMultiplier;
            // small daily randomness
            expected *= Random.Range(0.85f, 1.25f);

            int dayViews = Mathf.RoundToInt(expected);
            v.totalViews += dayViews;
            v.dayStats.Add(new VideoDayStat { dayIndex = currentDay, views = dayViews });

            // subscribers conversion
            int subs = Mathf.FloorToInt(dayViews * Balancing.VideoSubscriberConversion);
            v.totalSubscribersGained += subs;
            if(gm != null) gm.playerProfile.fans += subs;

            // monetization
            if(v.isMonetized) {
                float rev = (dayViews / 1000f) * Balancing.VideoCPM;
                v.revenue += rev;
                if(gm != null) gm.playerProfile.money += Mathf.FloorToInt(rev);
            }

            Telemetry.Emit("VideoDaySimulated", $"id={v.id};day={currentDay};views={dayViews};subs={subs};rev={v.revenue:F2}");
        }

        // persist summaries
        if(gm != null && gm.playerProfile != null) {
            gm.playerProfile.videos.Clear();
            foreach(var v in videos) {
                gm.playerProfile.videos.Add(new VideoSnapshot { id = v.id, title = v.title, uploadDay = v.uploadDay, totalViews = v.totalViews, isMonetized = v.isMonetized });
            }
            gm.Save();
        }
    }

    // Simple helper to find a video by id
    public Video GetVideo(string id) {
        return videos.Find(x => x.id == id);
    }
}
