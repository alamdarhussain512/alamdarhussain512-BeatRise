using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VideoDayStat {
    public int dayIndex;
    public int views;
}

[Serializable]
public enum CommentSentiment { Positive, Neutral, Negative }

[Serializable]
public class Comment {
    public string id;
    public string author;
    public string text;
    public int likes;
    public CommentSentiment sentiment;

    public Comment() { id = Guid.NewGuid().ToString(); }
}

[Serializable]
public class Video {
    public string id;
    public string title;
    public string description;
    public int lengthSeconds;
    public float quality; // 0-100
    public int uploadDay; // game day index

    // accumulated stats
    public int totalViews = 0;
    public int totalLikes = 0;
    public int totalSubscribersGained = 0;
    public float revenue = 0f;
    public bool isMonetized = false;

    public List<VideoDayStat> dayStats = new List<VideoDayStat>();
    public List<Comment> comments = new List<Comment>();

    public Video() { id = Guid.NewGuid().ToString(); }

    public int DaysSinceUpload(int currentDay) {
        return Mathf.Max(0, currentDay - uploadDay);
    }
}
