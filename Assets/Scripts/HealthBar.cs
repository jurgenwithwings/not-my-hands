using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
    [SerializeField] private Slider healthSlider;

    private Camera targetCamera;
    [SerializeField] private float visibleDuration = 2f; //How long the bar stays visible after taking damage (seconds).
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float screeSize = 0.1f;

    [SerializeField] private Statboard stats;

    public void SetStatboard(Statboard board) {
        stats = board;
    }

    private float defaultScaleFactor = 0.01f;
    private float lastDamageTime;
    private Canvas canvas;
    private bool isVisible;

    private float maxHealth => stats.maxHealth;
    private float currentHealth => stats.health.currentHealth;

    void Awake() {
        canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.enabled = false;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Start() {
        stats.eventManager.OnDamageTaken += SetSliderValue;
    }

    private void OnDestroy() {
        stats.eventManager.OnDamageTaken -= SetSliderValue;
    }

    private void SetSliderValue(DamageInfo damageInfo) {
        float healthPercent = currentHealth / maxHealth;
        healthSlider.value = healthPercent;
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
