using System;
using ObjectPooling;
using UnityEngine;
using UnityEngine.InputSystem;

public class DamageNumber : MonoBehaviour, IPoolable<DamageNumber> {
    [SerializeField] private GameObject poolPrefab;
    
    public Action<DamageNumber> ReturnToPoolAction { get; set; }

    public string SetKey() => "DamageNumber";
    
    public void OnPoolSpawn() {
        //noop
    }
    public void OnPoolDespawn() {
        //noop
    }
}