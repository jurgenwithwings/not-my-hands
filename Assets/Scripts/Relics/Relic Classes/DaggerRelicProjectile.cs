using System;
using UnityEngine;
using UnityEngine.VFX;

public class DaggerRelicProjectile : MonoBehaviour {
    [SerializeField] private Vector3 idleRotation;
    [SerializeField] private Vector3 idleRotationSpeed;
    [SerializeField] private float seekRotationSpeed;
    [SerializeField] private float seekMoveSpeed;
    [SerializeField] private float stuckTime;
    [SerializeField] private VisualEffect vfx;

    private DaggerRelic owningRelic;
    private Statboard owningEntity;
    private GameObject orbitPoint;
    private enum State { Idle, Seeking, Returning, Stuck, }
    private State state = State.Idle;
    public bool IsIdle => state == State.Idle;

    private GameObject target;

    private float stuckDuration;

    private void Start() {
        EnterState(State.Idle);
    }
    
    public void Setup(GameObject orbitPoint, DaggerRelic owningRelic, Statboard owningEntity) {
        this.orbitPoint = orbitPoint;
        this.owningRelic = owningRelic;
        this.owningEntity = owningEntity;
    }
    
    public void SeekTarget(Statboard target) {
        this.target = target.gameObject;
        EnterState(State.Seeking);
    }

    private void Update() {
        switch (state) {
            case State.Idle:
                transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + idleRotationSpeed * Time.deltaTime);
                transform.localPosition = Vector3.zero;
                break;
            case State.Seeking:
                MoveToTarget(1);
                break;
            case State.Returning:
                MoveToTarget(3);
                break;
            case State.Stuck:
                stuckDuration -= Time.deltaTime;
                if (stuckDuration <= 0) {
                    target = orbitPoint;
                    EnterState(State.Returning);
                }
                break;
        }
    }

    private void MoveToTarget(float scale) {
        Vector3 direction = target.transform.position - transform.position;
                
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, seekRotationSpeed * scale * Time.deltaTime);
        
        transform.position += transform.forward * seekMoveSpeed * scale * Time.deltaTime;
    }

    private void EnterState(State state) {
        this.state = state;
        switch (state) {
            case State.Idle:
                vfx.Stop();
                transform.SetParent(orbitPoint.transform);
                transform.localRotation = Quaternion.Euler(idleRotation);
                transform.localPosition = Vector3.zero;
                owningRelic.DaggerReturned(this);
                break;
            case State.Seeking or State.Returning:
                vfx.Play();
                transform.parent = null;
                break;
            case State.Stuck:
                vfx.Stop();
                stuckDuration = stuckTime;
                transform.SetParent(target.transform, worldPositionStays: true);
                break;
        }
    }

    private void OnTriggerEnter(Collider other) {
        switch (state) {
            case State.Seeking:
                if (other.gameObject == owningEntity.gameObject) return;
                if (other.TryGetComponent(out Statboard victim)) {
                    owningRelic.DaggerHit(victim, this);
                }
                target = other.gameObject;
                transform.SetParent(other.transform, worldPositionStays: true);
                EnterState(State.Stuck);
                break;
            case State.Returning:
                if (other.gameObject == target) {
                    transform.SetParent(other.transform, worldPositionStays:false);
                    EnterState(State.Idle);
                }
                break;
        }
    }
}