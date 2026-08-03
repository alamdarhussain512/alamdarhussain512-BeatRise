using UnityEngine;
using System;

public class TimeSystem : MonoBehaviour {
    public static TimeSystem Instance;
    public int currentDay = 1;
    public int currentHour = 9; // 0-23
    public float realSecondsPerGameHour = 5f; // configurable

    public event Action OnDayAdvanced;
    public event Action OnHourChanged;

    void Awake() { Instance = this; }
    void Start() { StartCoroutine(RunClock()); }

    System.Collections.IEnumerator RunClock() {
        while(true) {
            yield return new WaitForSeconds(realSecondsPerGameHour);
            currentHour++;
            if(currentHour >= 24) { currentHour = 0; currentDay++; OnDayAdvanced?.Invoke(); }
            OnHourChanged?.Invoke();
        }
    }

    public void FastForwardHours(int hours) {
        currentHour = (currentHour + hours) % 24;
        if(hours >= 24) { currentDay += hours / 24; OnDayAdvanced?.Invoke(); }
        OnHourChanged?.Invoke();
    }
}
