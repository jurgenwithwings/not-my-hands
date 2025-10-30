using System;
using ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DamageNumber : MonoBehaviour, IPoolable<DamageNumber> {
    public Action<DamageNumber> ReturnToPool { get; set; }
    public string ObjectPoolKey() => "DamageNumber";

    [SerializeField] private TMP_Text text;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float duration = 1f;
    
    private Transform lookTarget;
    private float startTime;
    private Vector3 startPosition;

    private void Awake() {
        lookTarget = Camera.main.transform;
    }

    public void OnPoolPull() {
        startTime = Time.time;
        startPosition = transform.position;
        text.rectTransform.localPosition = Vector3.zero;
    }
    
    public void SetDamage(float damage) {
        switch (damage) { 
            case >= 1000 and < 1000000:
                damage /= 1000f;
                text.text = damage.ToString("0.#") + "k";
                break;
            case >= 1000000 and < 1000000000:
                damage /= 1000000f;
                text.text = damage.ToString("0.##") + "M";
                break;
            case >= 1000000000:
                damage /= 1000000000f;
                text.text = damage.ToString("0.###") + "B";
                break;
            default:
                damage = Mathf.RoundToInt(damage);
                text.text = damage.ToString();
                break;
        }
    }
    
    private void Update() {
        float elapsed = Time.time - startTime;
        float t = elapsed / duration;
        if (t >= 1f) {
            ReturnToPool?.Invoke(this);
            return;
        }
        float curveValue = curve.Evaluate(t);
        transform.position = startPosition + (Vector3.up * t * 2f);
        text.alpha = curveValue;
        text.transform.localScale = Vector3.one * curveValue;
    }
    
    private void LateUpdate() {
        transform.LookAt(lookTarget);
    }
    
    public void OnPoolPush() {
        text.alpha = 0f;
    }
}