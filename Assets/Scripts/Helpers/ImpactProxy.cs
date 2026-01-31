using System;
using ObjectPooling;
using UnityEngine;

public class ImpactProxy : MonoBehaviour, IPoolable<ImpactProxy> {
    public string ObjectPoolKey() => "ImpactProxy";
    
    public Action OnDestroyed;
    
    public void Setup(Transform parent) {
        transform.SetParent(parent, worldPositionStays: true);
    }

    private void OnDestroy() {
        OnDestroyed?.Invoke();
        OnDestroyed -= OnDestroyed;
    }

    public Action<ImpactProxy> ReturnToPool { get; set; }
    public void OnPoolPull() { }
    public void OnPoolPush() { }
}
