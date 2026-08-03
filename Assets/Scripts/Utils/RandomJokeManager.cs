using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
class Joke {
    public int id;
    public string type;
    public string setup;
    public string punchline;
    public string joke; // some APIs return a single-field joke
}

public class RandomJokeManager : MonoBehaviour {
    public static RandomJokeManager Instance;

    [Tooltip("API endpoint (default: Official Joke API)")]
    public string apiUrl = "https://official-joke-api.appspot.com/random_joke";

    void Awake() {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Request a joke and write it into the provided UI Text
    public void RequestJoke(Text targetText) {
        if(targetText == null) return;
        StartCoroutine(FetchJokeCoroutine(targetText));
    }

    IEnumerator FetchJokeCoroutine(Text targetText) {
        targetText.text = "Loading joke...";

        using(var www = UnityWebRequest.Get(apiUrl)) {
#if UNITY_2020_1_OR_NEWER
            yield return www.SendWebRequest();
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
                targetText.text = "Error fetching joke: " + www.error;
                yield break;
            }
#else
            yield return www.Send();
            if(www.isNetworkError || www.isHttpError) {
                targetText.text = "Error fetching joke: " + www.error;
                yield break;
            }
#endif
            HandleResponse(www.downloadHandler.text, targetText);
        }
    }

    void HandleResponse(string json, Text targetText) {
        // Try parse Official Joke structure first
        try {
            var j = JsonUtility.FromJson<Joke>(json);
            if(j != null) {
                if(!string.IsNullOrEmpty(j.setup) || !string.IsNullOrEmpty(j.punchline)) {
                    targetText.text = j.setup + "\n\n" + j.punchline;
                    return;
                }
                if(!string.IsNullOrEmpty(j.joke)) {
                    targetText.text = j.joke;
                    return;
                }
            }
        } catch {}

        // Fallback: show raw JSON
        targetText.text = json;
    }
}
