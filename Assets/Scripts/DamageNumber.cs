using System;
using ObjectPooling;
using UnityEngine;

public class DamageNumber : MonoBehaviour, IPoolable<DamageNumber> {
    [SerializeField] private GameObject poolPrefab;
    
    public Action<DamageNumber> ReturnToPoolAction { get; set; }
    public void OnPoolSpawn() {
    }
    public void OnPoolDespawn() {
    }

    void OnValidate() {
        IPoolable<DamageNumber>.PoolPrefab = poolPrefab;
    }
}