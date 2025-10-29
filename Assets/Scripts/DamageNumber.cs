using System;
using ObjectPooling;
using UnityEngine;
using UnityEngine.InputSystem;

public class DamageNumber : MonoBehaviour, IPoolable<DamageNumber> {
    [SerializeField] private GameObject poolPrefab;
    
    public Action<DamageNumber> ReturnToPoolAction { get; set; }

    public string ObjectPoolKey() => "DamageNumber";
    
    public void OnPoolPull() {
        //noop
    }
    public void OnPoolPush() {
        //noop
    }
}