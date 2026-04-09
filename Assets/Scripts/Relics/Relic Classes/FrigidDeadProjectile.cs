using System;
using ObjectPooling;
using Stats;
using UnityEngine;
using UnityEngine.VFX;

public class FrigidDeadProjectile : MonoBehaviour, IPoolable<FrigidDeadProjectile> {
    public Action<FrigidDeadProjectile> ReturnToPool { get; set; }
    public string ObjectPoolKey() => "FrigidDead_Projectile";

    public void OnPoolPull() {
        lifetime = 0;
        attackStateTimer = 0;
        dot.ResetTimer();
    }

    public void OnPoolPush() {
        EnterState(State.Idle);
    }

    [SerializeField] private Transform nullPoint;
    [Space]
    [SerializeField] private float projectileMoveSpeed = 10f;
    [SerializeField] private float projectileRotationSpeed = 360f;
    [SerializeField] private float accelerationTime = 4f;
    [SerializeField] private float projectileLifeTime = 45f;
    [SerializeField] private float scanInterval = 1f;
    [SerializeField] private float scanRadius = 17f;
    [Space]
    [SerializeField] private float detonationRange = 1.5f;
    [SerializeField] private float detonationRadius = 3f;
    [SerializeField] private VisualEffect detonationEffect;

    private DoT dot;
    private FrigidDead owningRelic;
    private Statboard owningEntity;
    private float lifetime;
    
    private Transform target;
    
    private enum State { Idle, Attacking }
    private State state;
    private float attackStateTimer;

    public void Init(Damage damage, FrigidDead owningRelic, Statboard owningEntity) {
        dot.damage = damage;
        this.owningRelic = owningRelic;
        this.owningEntity = owningEntity;
    }
    
    private void Start() {
        target = nullPoint;
        dot.tickInterval = scanInterval;
        detonationEffect.Stop();
        detonationEffect.gameObject.transform.SetParent(null);
        EnterState(State.Idle);
    }

    private void EnterState(State state) {
        this.state = state;
        switch (state) {
            case State.Idle:
                target = nullPoint;
                break;
            case State.Attacking:
                if (target == nullPoint) {
                    EnterState(State.Idle);
                }
                break;
        }
    }

    private void Update() {
        lifetime += Time.deltaTime;
        if (lifetime > projectileLifeTime) {
            ReturnToPool?.Invoke(this);
        }
        
        dot.Update();

        attackStateTimer += state == State.Attacking ? Time.deltaTime : -Time.deltaTime;
        float moveScale = Mathf.Lerp(0, accelerationTime, attackStateTimer) / accelerationTime;
        MoveToTarget(moveScale);
        
        switch (state) {
            case State.Idle:
                attackStateTimer -= Time.deltaTime;
                if (dot.CanTick()) {
                    Collider[] hits = Physics.OverlapSphere(transform.position, scanRadius,GameConfig.Instance.pawnLayer);
                    if (hits.Length == 0) return;
                    
                    LayerMask mask = GameConfig.Instance.pawnLayer;
                    mask.AddLayerToMask(GameConfig.Instance.levelCollisionLayer);

                    foreach (Collider hit in hits) {
                        Vector3 direction = hit.transform.position - transform.position;
                        Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, scanRadius * 1.1f, GameConfig.Instance.pawnLayer);
                        
                        // Has hit a Pawn / Entity
                        if (!hitInfo.collider.gameObject.IsInLayerMask(GameConfig.Instance.pawnLayer)) continue;
                        if (hitInfo.collider.TryGetComponent(out Statboard hitEntity) && hitEntity != owningEntity) {
                            target = hitEntity.transform;
                            EnterState(State.Attacking);
                            break;
                        }
                    }
                    dot.ResetTimer();
                }
                break;
            case State.Attacking:
                if (target == nullPoint) {
                    EnterState(State.Idle);
                }

                if (Vector3.Distance(transform.position, target.transform.position) < detonationRange) {
                    detonationEffect.gameObject.transform.position = transform.position;
                    detonationEffect.Play();
                    
                    Collider[] hits = Physics.OverlapSphere(transform.position, detonationRadius,GameConfig.Instance.pawnLayer);
                    if (hits.Length == 0) {
                        ReturnToPool?.Invoke(this);
                        return;
                    }
                    
                    LayerMask mask = GameConfig.Instance.pawnLayer;
                    mask.AddLayerToMask(GameConfig.Instance.levelCollisionLayer);

                    foreach (Collider hit in hits) {
                        Vector3 direction = hit.transform.position - transform.position;
                        Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, detonationRadius * 1.1f, GameConfig.Instance.pawnLayer);
                        
                        // Has hit a Pawn / Entity
                        if (!hitInfo.collider.gameObject.IsInLayerMask(GameConfig.Instance.pawnLayer)) {
                            ReturnToPool?.Invoke(this);
                            return;
                        }
                        if (hitInfo.collider.TryGetComponent(out Statboard hitEntity) && hitEntity != owningEntity) {
                            DamageInfo info = new(dot.damage, owningEntity, hitInfo.point);
                            info.AddModifier(owningEntity.damageMultiplier.Value - 1, ModifierType.FinalAdditive);
                            Debug.Log($"{hitEntity.gameObject.name} took {info.baseDamage}");
                            hitEntity.health?.TakeDamage(info);
                        }
                    }
                    ReturnToPool?.Invoke(this);
                }
                break;
        }
    }
    
    private void MoveToTarget(float scale) {
        if (target == null) {
            EnterState(State.Idle);
            return;
        }
        Vector3 direction = target.transform.position - transform.position;
                
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float calculatedRotSpeed = projectileRotationSpeed * owningEntity.projectileSpeedMultiplier;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, calculatedRotSpeed * scale * Time.deltaTime);
        
        float calculatedMoveSpeed = projectileMoveSpeed * owningEntity.projectileSpeedMultiplier;
        transform.position += transform.forward * calculatedMoveSpeed * scale * Time.deltaTime;
    }
}