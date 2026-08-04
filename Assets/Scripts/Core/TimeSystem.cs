using UnityEngine;
using System;

public class TimeSystem : MonoBehaviour {
    public static TimeSystem Instance;

    [Header("Clock Settings")]
    public int currentDay = 1;
    public int currentHour = 9; // 0-23
    public float realSecondsPerGameHour = 6f; // configurable - use Balancing.DefaultSecondsPerGameHour

    [Header("State")]
    public bool isPaused = false;

    public event Action OnDayAdvanced;
    public event Action OnHourChanged;

    void Awake() { Instance = this; }
    void Start() {
        if(realSecondsPerGameHour <= 0f) realSecondsPerGameHour = Balancing.DefaultSecondsPerGameHour;
        StartCoroutine(RunClock());
    }

    System.Collections.IEnumerator RunClock() {
        while(true) {
            if(!isPaused) {
                yield return new WaitForSeconds(realSecondsPerGameHour);
                AdvanceHour(1);
            } else {
                yield return null;
            }
        }
    }

    public void AdvanceHour(int hours) {
        for(int i=0;i<hours;i++) {
            currentHour++;
            if(currentHour >= 24) { currentHour = 0; currentDay++; OnDayAdvanced?.Invoke(); }
            OnHourChanged?.Invoke();
        }
    }

    public void FastForwardHours(int hours) {
        AdvanceHour(hours);
    }

    public bool IsNight() {
        return currentHour < 6 || currentHour >= 20;
    }

    public void TogglePause() {
        isPaused = !isPaused;
    }
}
