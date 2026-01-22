using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour, IStatboard {
    public Statboard statboard { get; set; }
    public void StatboardFinishedSet() {
        statboard.eventManager.OnDamageTaken += SetSliderValue;
    }

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider secondaryHealthSlider;

    private Camera targetCamera;
    [SerializeField] private float visibleDuration = 2f; //How long the bar stays visible after taking damage (seconds).
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float screeSize = 0.1f;


    private float defaultScaleFactor = 0.01f;
    private Canvas canvas;
    private bool isVisible;
    private float lastDamageTime;
    private float targetHealth = 1;
    private float secondaryHealthMoveDelay = 1.68f;

    private float maxHealth => statboard.maxHealth;
    private float currentHealth => statboard.health.CurrentHealth;

    void Awake() {
        canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.enabled = false;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Start() {
        healthSlider.value = 1f;
        secondaryHealthSlider.value = 1f;
    }

    private void OnDestroy() {
        statboard.eventManager.OnDamageTaken -= SetSliderValue;
    }

    private void SetSliderValue(DamageInfo damageInfo) {
        float percent = currentHealth / maxHealth;
        
        if (percent < targetHealth && 
            Mathf.Approximately(secondaryHealthSlider.value, healthSlider.value)) { // If health lost, and anim not in progress. Start Timer.
            lastDamageTime = Time.time;
        }
        targetHealth = percent;
        healthSlider.value = percent;
        
        if (percent > targetHealth || secondaryHealthSlider.value - percent <= 0.01f) { //If gaining health or small damage, then skip animation.
            secondaryHealthSlider.value = targetHealth;
        }
    }

    private void Update() {
        if (Time.time - lastDamageTime > secondaryHealthMoveDelay) {
            secondaryHealthSlider.value = Mathf.MoveTowards(secondaryHealthSlider.value, targetHealth, Time.deltaTime * 0.5f);
        }
    }
    
    void LateUpdate() {
        // Billboard the health bar toward the camera
        if (targetCamera != null && canvas != null) {
            canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - targetCamera.transform.position);
            float distance = Vector3.Distance(targetCamera.transform.position, transform.position);
            canvas.transform.localScale = Vector3.one * distance * screeSize * defaultScaleFactor;
        }

        // Handle visibility timer
        if (canvas != null) {
            bool shouldBeVisible = (Time.time - lastDamageTime < visibleDuration) || IsLookedAt();
            if (shouldBeVisible != isVisible)
            {
                isVisible = shouldBeVisible;
                canvas.enabled = isVisible;
            }
        }
    }
    
    bool IsLookedAt() {
        if (lookTarget == null || targetCamera == null) return false;

        Vector3 dirToTarget = (lookTarget.position - targetCamera.transform.position).normalized;
        float dot = Vector3.Dot(targetCamera.transform.forward, dirToTarget);
        return dot > 0.98f; // Adjust this threshold for sensitivity (1 = dead center)
    }
}
