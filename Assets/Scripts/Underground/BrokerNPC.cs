using UnityEngine;
using System;

// Represents an underground broker NPC that can modify offer terms through negotiation.
public class BrokerNPC : MonoBehaviour {
    public string brokerName = "Murmur";
    public int trustLevel = 50; // 0-100, higher trust = better base terms

    // Negotiate an offer. Returns a modified clone of the offer with adjusted cost/success/detection.
    public UndergroundOffer Negotiate(UndergroundOffer baseOffer, int playerReputation, out int negotiatedCost) {
        // clone
        var mod = new UndergroundOffer {
            id = baseOffer.id,
            name = baseOffer.name,
            description = baseOffer.description,
            type = baseOffer.type,
            cost = baseOffer.cost,
            successChance = baseOffer.successChance,
            detectionChance = baseOffer.detectionChance,
            followerBoost = baseOffer.followerBoost,
            notorietyGain = baseOffer.notorietyGain,
            fineOnDetection = baseOffer.fineOnDetection
        };

        // Broker influence: trustLevel reduces cost and detection slightly
        float trustFactor = 1f - (trustLevel / 200f); // e.g., trust 50 -> 0.75
        mod.cost = Mathf.Max(0, Mathf.FloorToInt(baseOffer.cost * trustFactor));

        // If player has high reputation, broker can offer better success but detection risk increases slightly due to attention
        if(playerReputation > 60) {
            mod.successChance = Mathf.Clamp01(baseOffer.successChance + 0.05f);
            mod.detectionChance = Mathf.Clamp01(baseOffer.detectionChance + 0.05f);
        } else if(playerReputation < 30) {
            // low rep -> broker charges more
            mod.cost = Mathf.FloorToInt(mod.cost * 1.15f);
            mod.successChance = Mathf.Clamp01(baseOffer.successChance - 0.03f);
        }

        // Random negotiation variance
        float variance = UnityEngine.Random.Range(-0.05f, 0.05f);
        mod.successChance = Mathf.Clamp01(mod.successChance + variance);
        mod.detectionChance = Mathf.Clamp01(mod.detectionChance - (trustLevel/100f)*0.02f + variance*0.5f);

        negotiatedCost = mod.cost;
        return mod;
    }
}
