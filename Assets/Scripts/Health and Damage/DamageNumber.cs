using System;
using System.Collections.Generic;
using ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DamageNumber : MonoBehaviour, IPoolable<DamageNumber> {
    public Action<DamageNumber> ReturnToPool { get; set; }
    public string ObjectPoolKey() => "DamageNumber";

    [SerializeField] private TMP_Text text;
    [SerializeField] private AnimationCurve opacityCurve;
    [SerializeField] private AnimationCurve sizeCurve;
    [SerializeField] private AnimationCurve damageScaleCurve;
    [SerializeField] private float duration = 1.7f;
    [SerializeField] private float screeSize = 0.12f;
    [SerializeField] private float floatAmount = 2.2f;
    [SerializeField] private float positionVarianceAmount = 0.5f;

    private float scaledScreenSize => screeSize * PlayerSettings.DamageNumberScale;
    
    private const float DamageScaleEnd = 1000000;
    
    private Transform lookTarget;
    private Canvas canvas;
    private float startTime;
    private Vector3 startPosition;
    private float damageScale;
    private Vector3 floatDirection;

    private List<Image> effects;

    private void Awake() {
        lookTarget = Camera.main.transform;
        canvas = GetComponent<Canvas>();
    }

    private void Start() {
        text.transform.localScale = Vector3.zero;
    }

    public void OnPoolPull() {
        startTime = Time.time;
        startPosition = transform.position + Random.insideUnitSphere * positionVarianceAmount * GetScreenScaleFactor();
        text.rectTransform.localPosition = Vector3.zero;
        floatDirection = RandomFloatDirection(65f);
        canvas.sortingOrder = -1;
        text.transform.localScale = Vector3.zero;
    }
    
    public void SetDamage(DamageInfo info) {
        float finalDamage = info.finalDamage;
        
        string result = PlayerSettings.AbbreviateDamageNumbers ? 
            DamageExtensions.AbbreviateNumber(finalDamage) : 
            finalDamage.ToString(DamageExtensions.damageNumberFormat);

        for (int i = 0; i < info.resultingCritLevel; i++) {
            result += "!";
        }
        
        // Status Effects
        if (info.resultingAppliedEffects.Count > 0) result += " ";
        foreach (StatusEffect effect in info.resultingAppliedEffects) {
            if (effect.Data.icon != null) {
                result += $"<sprite name=\"{effect.Data.displayName}\">";
            }
        }
        
        text.text = result;
        
        float t = Mathf.InverseLerp(1, DamageScaleEnd, finalDamage);
        damageScale = damageScaleCurve.Evaluate(t);
    }
    
    private void Update() {
        float elapsed = Time.time - startTime;
        float t = elapsed / duration;

        canvas.sortingOrder--;
        
        if (t >= 1f) {
            ReturnToPool?.Invoke(this);
            return;
        }

        float screenScaleFactor = GetScreenScaleFactor();
        
        transform.position = startPosition + (floatDirection * floatAmount * t * screenScaleFactor);
        
        text.alpha = opacityCurve.Evaluate(t);
        
        text.transform.localScale = Vector3.one * sizeCurve.Evaluate(t) * damageScale * screenScaleFactor;
    }
    
    private void LateUpdate() {
        transform.LookAt(lookTarget);
    }
    
    public void OnPoolPush() {
        text.alpha = 0f;
    }

    private float GetScreenScaleFactor() {
        return Vector3.Distance(lookTarget.transform.position, transform.position) * scaledScreenSize;
    }
    
    public Vector3 RandomFloatDirection(float coneAngle)
    {
        float angleRad = coneAngle * Mathf.Deg2Rad;

        float theta = Random.Range(0f, Mathf.PI * 2f);

        float z = Random.Range(Mathf.Cos(angleRad), 1f);
        float r = Mathf.Sqrt(1f - z * z);

        Vector3 localDir = new Vector3(r * Mathf.Cos(theta), z, r * Mathf.Sin(theta));

        return localDir.normalized;
    }
}