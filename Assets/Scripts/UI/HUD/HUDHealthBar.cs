using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDHealthBar : MonoBehaviour {
    public enum BarType { Health, Mana }

    [SerializeField] private BarType barType;
    
    [SerializeField] private Slider redHealthBar;
    [SerializeField] private Slider yellowHealthBar;
    [SerializeField] private TMP_Text valueText;

    private float targetHealth = 1;

    private float lastDamageTime;
    private float secondaryHealthMoveDelay = 1.68f;

    private void Awake() {
        redHealthBar.value = 1;
        yellowHealthBar.value = 1;

        switch (barType) {
            case BarType.Health:
                PlayerHUDEvents.OnHealthChanged += SetTargetHealth;
                break;
            case BarType.Mana:
                PlayerHUDEvents.OnManaChanged += SetTargetHealth;
                break;
        }
    }

    private void OnDestroy() {
        switch (barType) {
            case BarType.Health:
                PlayerHUDEvents.OnHealthChanged -= SetTargetHealth;
                break;
            case BarType.Mana:
                PlayerHUDEvents.OnManaChanged -= SetTargetHealth;
                break;
        }
    }

    private void SetTargetHealth(float currentHealth, float maxHealth) {
        float percent = currentHealth / maxHealth;
        if (percent < targetHealth && 
                 Mathf.Approximately(yellowHealthBar.value, redHealthBar.value)) { // If health lost, and anim not in progress. Start Timer.
            lastDamageTime = Time.time;
        }
        targetHealth = percent;
        redHealthBar.value = percent;
        
        if (redHealthBar.value > yellowHealthBar.value || (yellowHealthBar.value - percent <= 0.01f && yellowHealthBar.value - redHealthBar.value <= 0.01f)) { //If gaining health or small damage, then skip animation.
            yellowHealthBar.value = targetHealth;
        }
        
        valueText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    private void Update() {
        if (Time.time - lastDamageTime > secondaryHealthMoveDelay) {
            yellowHealthBar.value = Mathf.MoveTowards(yellowHealthBar.value, targetHealth, Time.deltaTime * 0.5f);
        }
    }
}