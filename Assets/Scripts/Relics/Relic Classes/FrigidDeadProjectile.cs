using System;
using ObjectPooling;
using UnityEngine;

public class FrigidDeadProjectile : Projectile, IPoolable<FrigidDeadProjectile> {
    public Action<FrigidDeadProjectile> ReturnToPool { get; set; }
    public string ObjectPoolKey() => "FrigidDeadProjectile";
    public void OnPoolPull() { }
    public void OnPoolPush() { }
    
    [SerializeField] private float followDistance = 1.6f;
    [SerializeField] private float projectileMoveSpeed = 10f;
    [SerializeField] private float projectileRotationSpeed = 360f;

    private FrigidDead owningRelic;
    private Statboard owningEntity;
    private float lifetime;
    
    private enum State { Idle, Following, Attacking }
    private State state;

    private void Start() {
        EnterState(State.Idle);
    }

    private void EnterState(State state) {
        switch (state) {
            case State.Idle:
                break;
            case State.Following:
                break;
            case State.Attacking:
                break;
        }
    }

    private void Update() {
        switch (state) {
            case State.Idle:
                break;
            case State.Following:
                break;
            case State.Attacking:
                break;
        }
    }
}