using UnityEngine;

[System.Serializable]
public class Venue {
    public string id;
    public string name;
    public int capacity;
    public int basePayout;
    public float exposure;
}

public struct GigResult { public int fansGained; public int payout; }

public class GigSystem : MonoBehaviour {
    public static GigSystem Instance;
    void Awake() => Instance = this;

    public Venue[] venues;

    public GigResult PlayGig(Venue v, Song song, PlayerProfile p, bool playedMinigame, int minigameScore) {
        float perf = p.skills.stagecraft + (playedMinigame ? minigameScore/10f : 0f);
        float popularity = song.finalQuality + p.fans / 1000f;
        int fansGained = Mathf.FloorToInt((v.exposure * (popularity + perf)) / 10f);
        int payout = Mathf.Max(0, v.basePayout + Mathf.FloorToInt(popularity * 10));
        Debug.Log($"Gig result at {v.name}: fansGained={fansGained}, payout={payout}");
        return new GigResult { fansGained = fansGained, payout = payout };
    }
}
