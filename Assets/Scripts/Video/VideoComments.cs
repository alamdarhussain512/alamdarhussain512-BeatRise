using UnityEngine;

// Simple comment generator helpers for videos
public static class VideoComments {
    static string[] positive = new string[] {
        "Loved this! 🔥", "Amazing track, wow!", "Subscribed! More please", "This is viral material", "Vocals are top-tier"
    };
    static string[] neutral = new string[] {
        "Not bad", "Interesting approach", "Could be better", "Nice production", "I like the beat"
    };
    static string[] negative = new string[] {
        "This is cringe", "Sounds fake", "Not my taste", "Bought followers?", "Disliked"
    };

    static string[] names = new string[] { "Sam", "Ria", "Noah", "Aisha", "Omar", "Maya", "Liam", "Zara" };

    // Generates comments based on day's views and video quality.
    // Outputs counts of each sentiment via out parameters and appends to video.comments
    public static void GenerateComments(Video v, int dayViews, out int positives, out int neutrals, out int negatives) {
        positives = neutrals = negatives = 0;
        if(v == null || dayViews <= 0) return;

        // comment rate is 0.1% of views by default, capped
        int count = Mathf.Clamp(Mathf.FloorToInt(dayViews * 0.001f), 0, 12);
        if(count == 0) return;

        float qualityFactor = Mathf.Clamp01(v.quality / 100f);
        for(int i=0;i<count;i++) {
            float r = Random.Range(0f,1f);
            Comment c = new Comment();
            c.author = names[Random.Range(0, names.Length)];
            if(r < 0.4f * qualityFactor) {
                c.sentiment = CommentSentiment.Positive;
                c.text = positive[Random.Range(0, positive.Length)];
                positives++;
            } else if(r < 0.7f) {
                c.sentiment = CommentSentiment.Neutral;
                c.text = neutral[Random.Range(0, neutral.Length)];
                neutrals++;
            } else {
                c.sentiment = CommentSentiment.Negative;
                c.text = negative[Random.Range(0, negative.Length)];
                negatives++;
            }
            v.comments.Add(c);
        }
    }
}
