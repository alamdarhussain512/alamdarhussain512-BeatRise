using UnityEngine;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    [Header("References")]
    public TimeSystem timeSystem;
    public PlayerProfile playerProfile;

    [Header("Settings")]
    public bool debugMode = false;
    public float moneyMultiplier = 1f; // can be tuned for difficulty

    public event Action OnGameSaved;

    void Awake() {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        if(timeSystem == null) timeSystem = FindObjectOfType<TimeSystem>();
        if(playerProfile == null) playerProfile = SaveSystem.Load();

        // Hook into time system to handle hourly effects
        if(timeSystem != null) timeSystem.OnHourChanged += HandleHourlyTick;
    }

    void HandleHourlyTick() {
        // Regenerate energy each in-game hour
        playerProfile.energy = Mathf.Min(Balancing.MaxEnergy, playerProfile.energy + Balancing.EnergyRegenPerHour);
    }

    public void ApplyCost(float moneyCost, float energyCost) {
        playerProfile.money = Mathf.Max(0, playerProfile.money - Mathf.FloorToInt(moneyCost * moneyMultiplier));
        playerProfile.energy = Mathf.Max(0f, playerProfile.energy - energyCost);
    }

    public void AddMoney(int amount) {
        playerProfile.money += Mathf.FloorToInt(amount * moneyMultiplier);
    }

    public void Save() {
        SaveSystem.Save(playerProfile);
        OnGameSaved?.Invoke();
    }
}
