using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

[Serializable]
public class SimulationResult {
    public string mode;
    public int iterations;
    public int totalPublishes;
    public int totalOffers;
    public int totalReach;
    public int totalFollowersGained;
    public int totalDetections;
    public int totalFines;
    public int totalReputationLost;
    public float averageNotoriety;
    public string timestamp;
}

// A simple simulation runner that uses existing managers in the scene to run many actions and record outcomes.
public class Simulator : MonoBehaviour {
    public void RunSimulation(int iterations, string mode) {
        var spm = SocialPlatformManager.Instance ?? FindObjectOfType<SocialPlatformManager>();
        var um = UndergroundMarketManager.Instance ?? FindObjectOfType<UndergroundMarketManager>();
        var gm = GameManager.Instance ?? FindObjectOfType<GameManager>();

        if(spm == null && um == null) {
            Debug.LogError("No SocialPlatformManager or UndergroundMarketManager found to run simulation.");
            return;
        }

        // Snapshot starting state so we can restore after simulation
        var platformSnapshots = spm != null ? spm.platforms.Select(p => new { name = p.platformName, followers = p.followerCount }).ToList() : null;
        var offerSnapshots = um != null ? um.offers.Select(o => new { id = o.id }).ToList() : null;

        int totalPublishes = 0;
        int totalOffers = 0;
        int totalReach = 0;
        int totalFollowersGained = 0;
        int totalDetections = 0;
        int totalFines = 0;
        int totalReputationLost = 0;

        for(int i=0;i<iterations;i++) {
            // Choose action based on mode
            string action = mode == "PublishOnly" ? "publish" : (mode == "OffersOnly" ? "offer" : (UnityEngine.Random.value < 0.7f ? "publish" : "offer"));

            if(action == "publish" && spm != null) {
                // pick random platform
                var platform = spm.platforms[UnityEngine.Random.Range(0, spm.platforms.Count)];
                var post = new Post { id = Guid.NewGuid().ToString(), content = "simulated", quality = UnityEngine.Random.Range(30f, 90f), engagementRate = UnityEngine.Random.Range(0.02f, 0.15f) };
                var res = spm.PublishPost(platform, post);
                totalPublishes++;
                totalReach += res.reach;
                totalFollowersGained += res.gained;

                // approximate detection/fine/reputation events by checking differences in profile (if gm present)
                if(gm != null) {
                    // can't precisely detect events without instrumenting managers; approximate by checking reputation drop or fines since last iteration
                    // For accurate telemetry, managers already call Debug.Log; we count logs less reliably here.
                }
            } else if(action == "offer" && um != null) {
                var offer = um.offers[UnityEngine.Random.Range(0, um.offers.Count)];
                totalOffers++;
                // ensure enough funds for offer in simulation by temporarily topping up
                if(gm != null) gm.playerProfile.money += offer.cost + 1000;
                bool success = um.ExecuteOffer(offer.id);
                // Post-execution, check logs for detection/fines via player profile changes
                if(gm != null) {
                    // simplistic: if notoriety increased, record average; detect fines by comparing money (can't easily track per-offer without more hooks)
                }
            }
        }

        // Gather summary after simulation
        var result = new SimulationResult();
        result.mode = mode;
        result.iterations = iterations;
        result.totalPublishes = totalPublishes;
        result.totalOffers = totalOffers;
        result.totalReach = totalReach;
        result.totalFollowersGained = totalFollowersGained;
        result.totalDetections = totalDetections; // detailed detection counting requires instrumented events; left as 0 for now
        result.totalFines = totalFines;
        result.totalReputationLost = totalReputationLost;
        result.averageNotoriety = gm != null ? gm.playerProfile.notoriety : 0f;
        result.timestamp = DateTime.UtcNow.ToString("o");

        // Write to file
        try {
            var json = JsonUtility.ToJson(result, true);
            var path = Path.Combine(Application.persistentDataPath, $"simulation_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.json");
            File.WriteAllText(path, json);
            Debug.Log($"Simulation complete. Results written to {path}");
            Debug.Log(json);
        } catch(Exception ex) {
            Debug.LogError($"Failed to write simulation results: {ex.Message}");
        }

        // Optionally: restore platform follower counts to original snapshot to avoid polluting the running game
        if(spm != null && platformSnapshots != null) {
            foreach(var snap in platformSnapshots) {
                var p = spm.GetPlatformByName(snap.name);
                if(p != null) p.followerCount = snap.followers;
            }
        }

        // Save state after simulation
        if(gm != null) gm.Save();
    }
}
