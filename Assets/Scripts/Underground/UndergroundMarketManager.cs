using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class OfferUsageSnapshot {
    public string offerId;
    public int timesUsed;
}

public class UndergroundMarketManager : MonoBehaviour {
    public static UndergroundMarketManager Instance;

    public List<UndergroundOffer> offers = new List<UndergroundOffer>();

    void Awake() {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(offers == null || offers.Count == 0) InitializeDefaultOffers();
    }

    void InitializeDefaultOffers() {
        offers = new List<UndergroundOffer> {
            new UndergroundOffer { id = "promo_small", name = "Whisper Promo", description = "Small paid push to playlists and channels. Moderate risk, small boost.", type = OfferType.PaidPromotion, cost = 200, successChance = 0.6f, detectionChance = 0.08f, followerBoost = 120, notorietyGain = 5, fineOnDetection = 0 },
            new UndergroundOffer { id = "fake_followers", name = "Follower Pack (Fake)", description = "Buy quick followers. High risk: may be detected and cause penalties.", type = OfferType.FakeFollowers, cost = 500, successChance = 1.0f, detectionChance = 0.45f, followerBoost = 1200, notorietyGain = 20, fineOnDetection = 300 },
            new UndergroundOffer { id = "leak_track", name = "Leak to Underground", description = "Pay broker to leak track for virality. High reward, high risk.", type = OfferType.Leak, cost = 800, successChance = 0.55f, detectionChance = 0.35f, followerBoost = 2500, notorietyGain = 30, fineOnDetection = 600 }
        };
    }

    // Simple execution flow: returns true if offer succeeded (granting followerBoost), false otherwise; detection events may still occur
    public bool ExecuteOffer(string offerId) {
        var offer = offers.FirstOrDefault(o => o.id == offerId);
        if(offer == null) return false;
        var gm = GameManager.Instance;
        if(gm == null) return false;

        // Check funds
        if(gm.playerProfile.money < offer.cost) {
            Debug.Log("Not enough money for offer");
            return false;
        }

        gm.playerProfile.money -= offer.cost;

        // Compute detection chance scaled by notoriety
        float notoriety = gm.playerProfile.notoriety;
        float scaledDetection = Mathf.Clamp01(offer.detectionChance + (notoriety * Balancing.UndergroundNotorietyDetectionScale));

        // Roll for success
        float roll = UnityEngine.Random.Range(0f,1f);
        bool success = roll < offer.successChance;

        // Apply success effects
        if(success) {
            gm.playerProfile.fans += offer.followerBoost;
            Debug.Log($"Offer {offer.name} succeeded: +{offer.followerBoost} fans");
        } else {
            Debug.Log($"Offer {offer.name} failed to produce the intended effect");
        }

        // Increase notoriety
        gm.playerProfile.notoriety = Mathf.Min(100, gm.playerProfile.notoriety + offer.notorietyGain);

        // Detection roll
        float detectRoll = UnityEngine.Random.Range(0f,1f);
        if(detectRoll < scaledDetection) {
            // Detected: apply penalties
            int fine = offer.fineOnDetection > 0 ? offer.fineOnDetection : Mathf.FloorToInt(gm.playerProfile.money * Balancing.RiskyFineMultiplier);
            gm.playerProfile.money = Mathf.Max(0, gm.playerProfile.money - fine);
            gm.playerProfile.reputation = Mathf.Max(0, gm.playerProfile.reputation - Mathf.Clamp(offer.notorietyGain / 2, 5, 30));

            // Optional: trigger platform enforcement event (simulated)
            Debug.Log($"Offer {offer.name} was detected! Fine={fine}, reputation now={gm.playerProfile.reputation}");

            // Track detection event for telemetry
        }

        // Record usage
        var existing = gm.playerProfile.offerUsages.Find(u => u.offerId == offer.id);
        if(existing == null) {
            gm.playerProfile.offerUsages.Add(new OfferUsageSnapshot { offerId = offer.id, timesUsed = 1 });
        } else existing.timesUsed++;

        // Save after action
        gm.Save();

        return success;
    }

    // Export/Import usage and notoriety to PlayerProfile
    public void ExportToProfile(PlayerProfile profile) {
        if(profile == null) return;
        profile.notoriety = GameManager.Instance.playerProfile.notoriety;
        profile.offerUsages = GameManager.Instance.playerProfile.offerUsages;
    }

    public void ImportFromProfile(PlayerProfile profile) {
        if(profile == null) return;
        if(profile.offerUsages != null && profile.offerUsages.Count > 0) {
            foreach(var u in profile.offerUsages) {
                var o = offers.FirstOrDefault(x => x.id == u.offerId);
                if(o == null) continue; // unknown offer
                // no direct action needed; usages are just for analytics
            }
        }
    }
}
