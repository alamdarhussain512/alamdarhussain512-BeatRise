using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lightweight coroutine batcher to run large lists of actions across multiple frames
// Attach this component to a GameObject and call StartBatch(actions, batchSize)
public class CoroutineBatcher : MonoBehaviour {
    public static CoroutineBatcher Instance;

    void Awake() {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartBatch(IEnumerable<Action> actions, int batchSize = 50, Action onComplete = null) {
        StartCoroutine(RunBatch(actions, batchSize, onComplete));
    }

    IEnumerator RunBatch(IEnumerable<Action> actions, int batchSize, Action onComplete) {
        if(actions == null) yield break;
        var enumerator = actions.GetEnumerator();
        int processed = 0;
        while(true) {
            for(int i=0;i<batchSize;i++) {
                bool has = enumerator.MoveNext();
                if(!has) {
                    onComplete?.Invoke();
                    yield break;
                }
                try { enumerator.Current?.Invoke(); } catch(Exception ex) { Debug.LogException(ex); }
                processed++;
            }
            // optionally back off if stability monitor recommends
            if(StabilityMonitor.Instance != null && StabilityMonitor.Instance.ShouldThrottle()) {
                // wait a couple frames before resuming
                yield return null; yield return null;
            } else {
                // yield to next frame to avoid blocking
                yield return null;
            }
        }
    }
}
