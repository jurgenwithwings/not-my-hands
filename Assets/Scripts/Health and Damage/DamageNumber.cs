using System;
using ObjectPooling;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageNumber : MonoBehaviour, IPoolable<DamageNumber> {
    public Action<DamageNumber> ReturnToPool { get; set; }
    public string ObjectPoolKey() => "DamageNumber";

    [SerializeField] private TMP_Text text;
    [SerializeField] private AnimationCurve opacityCurve;
    [SerializeField] private AnimationCurve sizeCurve;
    [SerializeField] private AnimationCurve damageScaleCurve;
    [SerializeField] private float duration = 1.7f;
    [SerializeField] private float screeSize = 0.1f;
    [SerializeField] private float floatAmount = 2.2f;
    [SerializeField] private float positionVarianceAmount = 1f;
    
    private const float DamageScaleEnd = 1000000;
    
    private Transform lookTarget;
    private Canvas canvas;
    private float startTime;
    private Vector3 startPosition;
    private float damageScale;
    private Vector3 floatDirection;

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
    
    public void SetDamage(float damage) {
        Debug.Log(damage);
        text.text = PlayerSettings.AbbreviateDamageNumbers ? 
            DamageExtensions.AbbreviateNumber(damage) : 
            damage.ToString(DamageExtensions.damageNumberFormat);
        
        float t = Mathf.InverseLerp(1, DamageScaleEnd, damage);
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
        return Vector3.Distance(lookTarget.transform.position, transform.position) * screeSize;
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