using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[Serializable] public class DaggerRelic : Relic {
    [SerializeField] private GameObject daggerPrefab;
    [SerializeField] private float seekRange = 20;
    [SerializeField] private DoT seekTimer;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float baseHealPercent = 0.01f;
    [SerializeField] private float stackHealPercent = 0.005f;
    [SerializeField] private int baseDaggers = 6;
    [SerializeField] private int stackDaggers = 1;
    [SerializeField] private float daggerOrbitRange = 3;
    [SerializeField] private float daggerOrbitSpeed = 35;
    private DaggerRelicProjectile[] ownedDaggers;
    private Stack<DaggerRelicProjectile> daggerStack = new Stack<DaggerRelicProjectile>();
    private List<GameObject> orbitPoints = new List<GameObject>();
    private PositionConstraint orbitCenter;
    
    public Action<DaggerRelicProjectile> OnReturnedToIdle;
    
    private int orbitingDaggers => (int)GetStackValue(baseDaggers, stackDaggers, stacks);

    public override void Initialise(RelicManager relicManager, RelicData relicData) {
        base.Initialise(relicManager, relicData);
        
        DaggerRelic config = data.relicClass as DaggerRelic;
        daggerPrefab = config.daggerPrefab;
        seekRange = config.seekRange;
        seekTimer = config.seekTimer;
        targetLayer = config.targetLayer;
        groundLayer = config.groundLayer;
        baseHealPercent = config.baseHealPercent;
        stackHealPercent = config.stackHealPercent;
        baseDaggers = config.baseDaggers;
        stackDaggers = config.stackDaggers;
        daggerOrbitRange = config.daggerOrbitRange;
        
        orbitCenter = new GameObject("OrbitCenter").AddComponent<PositionConstraint>();
        ConstraintSource cs = new ConstraintSource() {
            sourceTransform = stats.transform,
            weight = 1
        };
        orbitCenter.AddSource(cs);
        orbitCenter.constraintActive = true;
        orbitCenter.locked = true;

        BuildOrbitPoints(baseDaggers);
    }

    private void BuildOrbitPoints(int amount) {
        float angleStep = 360f / amount;
        
        for (int i = 0; i < amount; i++) {
            if (orbitPoints.Count <= i) {
                orbitPoints.Add(new GameObject($"OrbitPoint{i}"));
                SphereCollider coll = orbitPoints[i].AddComponent<SphereCollider>();
                coll.isTrigger = true;
                coll.radius = 0.7f;
                coll.gameObject.layer = GameConfig.Instance.ignoreRaycastLayer;
            }
            
            orbitPoints[i].transform.SetParent(orbitCenter.transform, worldPositionStays: false);
            
            float angle = angleStep * i * Mathf.Deg2Rad;
            
            orbitPoints[i].transform.localPosition = new Vector3(Mathf.Cos(angle) * daggerOrbitRange, 0f, Mathf.Sin(angle) * daggerOrbitRange);
        }

        //Remove Excess Points
        if (orbitPoints.Count > amount) {
            for (int i = orbitPoints.Count - 1; i >= amount; i--) {
                Object.Destroy(orbitPoints[i]);
                orbitPoints.RemoveAt(i);
            }
        }

        List<DaggerRelicProjectile> daggers = ownedDaggers?.ToList() ?? new List<DaggerRelicProjectile>();
        daggerStack.Clear();
        for (int i = 0; i < orbitPoints.Count; i++) {
            if (daggers.Count <= i) {
                daggers.Add(Object.Instantiate(daggerPrefab).GetComponent<DaggerRelicProjectile>());
                daggers[i].transform.SetParent(orbitCenter.transform, worldPositionStays: false);
                daggers[i].transform.localPosition = Vector3.zero;
            }

            daggers[i].Setup(orbitPoints[i], this, stats);
            if (daggers[i].IsIdle) {
                daggerStack.Push(daggers[i]);
            }
        }

        //Remove Excess Daggers
        if (daggers.Count > orbitPoints.Count) {
            for (int i = orbitPoints.Count - 1; i >= amount; i--) {
                Object.Destroy(daggers[i]);
                daggers.RemoveAt(i);
            }
        }
        
        ownedDaggers = daggers.ToArray();
    }

    public void DaggerReturned(DaggerRelicProjectile dagger) {
        daggerStack.Push(dagger);
    }

    public void DaggerHit(Statboard victim, DaggerRelicProjectile dagger) {
        DamageInfo damage = new(seekTimer.GetTickDamage(stacks), stats, dagger.transform.position);
        
        victim?.health?.TakeDamage(damage);
        stats?.health?.Heal(stats.maxHealth * GetStackValue(baseHealPercent, stackHealPercent, stacks));
    }

    public override void Update() {
        base.Update();
        
        seekTimer.Update();
        if (seekTimer.CanTick() && daggerStack.Count > 0) {
            seekTimer.ResetTimer();
            LookForTarget();
        }
        
        orbitCenter.transform.Rotate(new Vector3(0, daggerOrbitSpeed * Time.deltaTime, 0));
    }

    private void LookForTarget() {
        Collider[] colliders = Physics.OverlapSphere(stats.transform.position, seekRange, targetLayer);
        for (int i = 0; i < daggerStack.Count; i++) {
            int colliderIndex = Random.Range(0, colliders.Length);
            
            LayerMask targetAndGroundLayer = targetLayer;
            targetAndGroundLayer |= (1 << GameConfig.Instance.levelCollisionLayer);
            Physics.Raycast(stats.transform.position, colliders[colliderIndex].transform.position - stats.transform.position, out RaycastHit hit, seekRange, targetAndGroundLayer);
            
            if (hit.collider != null && hit.collider.TryGetComponent(out Statboard target)) {
                 var pop = daggerStack.Pop();
                 pop.SeekTarget(target);
            }
        }
    }

    public override void AddStack(int amount) {
        base.AddStack(amount);
        
        BuildOrbitPoints(orbitingDaggers);
    }

    public override void RemoveStack() {
        base.RemoveStack();

        if (stacks > 0) {
            BuildOrbitPoints(orbitingDaggers);
        }
    }

    public override void Remove() {
        base.Remove();

        foreach (DaggerRelicProjectile dagger in ownedDaggers) {
            Object.Destroy(dagger.gameObject);
        }

        foreach (GameObject point in orbitPoints) {
            Object.Destroy(point.gameObject);
        }
        
        Object.Destroy(orbitCenter.gameObject);
    }
}