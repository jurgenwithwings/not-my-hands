using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour, IStatboard {
    public Statboard statboard { get; set; }
    public virtual void StatboardFinishedSet() {
        effectBar.Init(statboard);
        
        statboard.eventManager.OnDamageTaken += OnDamageTaken;
        statboard.eventManager.OnHealthChanged += OnHealthChanged;
    }

    [SerializeField] protected EffectBar effectBar;
    [Space]
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Slider healthSlider;
    [SerializeField] protected Slider secondaryHealthSlider;

    protected Camera targetCamera;
    [Tooltip("How long the bar stays visible after taking damage (seconds).")]
    [SerializeField] protected float visibleDuration = 2f;
    [Tooltip("The point that the player need to look at in order to for the health bar to be visible.")]
    [SerializeField] protected Transform lookTarget;
    [SerializeField] protected float screeSize = 0.16f;
    [Tooltip("Adjust this threshold for sensitivity (1 = dead center)")]
    [SerializeField] protected float visibilitySensitivity = 0.98f;
    [SerializeField] protected float alwaysVisibleDistance = 5f;
    [SerializeField] protected AnimationCurve distanceResponseCurve;

    protected float defaultScaleFactor = 0.01f;
    protected bool isVisible;
    protected float lastDamageTime;
    protected float targetHealth = 1;
    protected float secondaryHealthMoveDelay = 1.68f;

    protected float maxHealth => statboard.maxHealth;
    protected float currentHealth => statboard.health.CurrentHealth;

    
    // Setup
    protected virtual void Awake() {
        if (canvas != null)
            canvas.enabled = false;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    protected virtual void Start() {
        healthSlider.value = 1f;
        secondaryHealthSlider.value = 1f;
    }

    protected virtual void OnDestroy() {
        statboard.eventManager.OnDamageTaken -= OnDamageTaken;
        statboard.eventManager.OnHealthChanged -= OnHealthChanged;
    }

    
    // Handle Event Calls
    protected virtual void OnDamageTaken(DamageInfo _) => SetSliderValue();
    protected virtual void OnHealthChanged(float _, float _2) => SetSliderValue();
    
    // Change Health Bar
    protected virtual void SetSliderValue() {
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

    
    // Animate Health Bar
    protected virtual void Update() {
        // Change Secondary Health Bar
        if (Time.time - lastDamageTime > secondaryHealthMoveDelay) {
            secondaryHealthSlider.value = Mathf.MoveTowards(secondaryHealthSlider.value, targetHealth, Time.deltaTime * 0.5f);
        }
    }
    
    protected virtual void LateUpdate() {
        if (canvas == null) return;
        
        float distance = Vector3.Distance(targetCamera.transform.position, transform.position);
        HandleVisibilityTimer(distance);
        Billboard(distance);
    }

    protected void Billboard(float distance) {
        if (canvas.enabled && targetCamera != null) {
            canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - targetCamera.transform.position);
            canvas.transform.localScale = Vector3.one * distance * screeSize * defaultScaleFactor * distanceResponseCurve.Evaluate(distance);
        }
    }

    protected void HandleVisibilityTimer(float distance) {
        bool shouldBeVisible = (Time.time - lastDamageTime < visibleDuration) || IsLookedAt(distance);
        if (shouldBeVisible != isVisible)
        {
            isVisible = shouldBeVisible;
            canvas.enabled = isVisible;
        }
    }

    protected virtual bool IsLookedAt(float distance) {
        if (lookTarget == null || targetCamera == null) return false;
        if (distance < alwaysVisibleDistance) return true;

        Vector3 dirToTarget = (lookTarget.position - targetCamera.transform.position).normalized;
        float dot = Vector3.Dot(targetCamera.transform.forward, dirToTarget);
        return dot > visibilitySensitivity;
    }
}