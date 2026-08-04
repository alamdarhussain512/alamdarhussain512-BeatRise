using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Runs automated balance sweeps by varying a small set of balancing parameters and recording outcomes.
// This is a test helper — attach to a GameObject in a test scene and call StartSweep() from Inspector or via code.
public class BalanceSweeper : MonoBehaviour {
    [Serializable]
    public class SweepResult {
        public float cpm;
        public float subConv;
        public float commentMult;
        public int totalViews;
        public int totalSubs;
        public float totalRevenue;
    }

    public float[] CPMValues = new float[] { 0.5f, 1.5f, 3f };
    public float[] SubConvValues = new float[] { 0.001f, 0.0025f, 0.005f };
    public float[] CommentMultipliers = new float[] { 0.5f, 1f, 2f };

    public int simulateDays = 30;
    public string outputFileName = "balance_sweep.json";

    public void StartSweep() {
        StartCoroutine(RunSweep());
    }

    IEnumerator RunSweep() {
        var vm = VideoManager.Instance; var gm = GameManager.Instance;
        if(vm==null || gm==null) { Debug.LogWarning("BalanceSweeper requires VideoManager and GameManager in scene."); yield break; }

        // backup state
        var videosBackup = JsonUtility.ToJson(new Wrapper<Video> { items = vm.videos.ToArray() });
        var profileBackup = JsonUtility.ToJson(gm.playerProfile);

        var results = new List<SweepResult>();

        int totalRuns = CPMValues.Length * SubConvValues.Length * CommentMultipliers.Length;
        int runIndex = 0;

        foreach(var cpm in CPMValues) {
            foreach(var sc in SubConvValues) {
                foreach(var cm in CommentMultipliers) {
                    runIndex++;
                    Debug.Log($"BalanceSweeper: run {runIndex}/{totalRuns} cpm={cpm} subConv={sc} cm={cm}");

                    // apply balancing
                    Balancing.VideoCPM = cpm;
                    Balancing.VideoSubscriberConversion = sc;
                    Balancing.VideoCommentRateMultiplier = cm;

                    // restore videos/profile to backup
                    var wrapped = JsonUtility.FromJson<Wrapper<Video>>(videosBackup);
                    vm.videos = new List<Video>(wrapped.items);
                    gm.playerProfile = JsonUtility.FromJson<PlayerProfile>(profileBackup);

                    // simulate days (allow frames between days so StabilityMonitor can react)
                    int startDay = gm.TimeOfDay;
                    for(int d=0; d<simulateDays; d++) {
                        int day = startDay + d;
                        vm.SimulateDay(day);
                        // check stability and yield extra frames if needed
                        if(StabilityMonitor.Instance != null && StabilityMonitor.Instance.ShouldThrottle()) {
                            // back off if unstable
                            yield return null; yield return null; yield return null;
                        }
                        yield return null;
                    }

                    // collect metrics
                    int totalViews = 0; int totalSubs = 0; float totalRev = 0f;
                    foreach(var v in vm.videos) { totalViews += v.totalViews; totalSubs += v.totalSubscribersGained; totalRev += v.revenue; }
                    results.Add(new SweepResult { cpm = cpm, subConv = sc, commentMult = cm, totalViews = totalViews, totalSubs = totalSubs, totalRevenue = totalRev });

                    // small yield between runs
                    yield return null;
                }
            }
        }

        // write results
        string path = Path.Combine(Application.persistentDataPath, outputFileName);
        var container = new SweepResultContainer { results = results.ToArray() };
        File.WriteAllText(path, JsonUtility.ToJson(container, true));
        Debug.Log($"Balance sweep complete. Results written to: {path}");

        // restore original balancing to reasonable defaults
        Balancing.VideoCPM = 1.5f;
        Balancing.VideoSubscriberConversion = 0.0025f;
        Balancing.VideoCommentRateMultiplier = 1f;

        // restore state
        vm.videos = new List<Video>(JsonUtility.FromJson<Wrapper<Video>>(videosBackup).items);
        gm.playerProfile = JsonUtility.FromJson<PlayerProfile>(profileBackup);
    }

    [Serializable]
    class SweepResultContainer { public SweepResult[] results; }

    [Serializable]
    class Wrapper<T> { public T[] items; }
}
