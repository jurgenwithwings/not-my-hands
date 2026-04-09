using System;
using ObjectPooling;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable] public class FrigidDead : Relic {
    [SerializeField] private DoT damage;

    public override void Initialise(RelicManager relicManager, RelicData relicData) {
        base.Initialise(relicManager, relicData);
        
        FrigidDead config = data.relicClass as FrigidDead;
        damage = config.damage;
        
        stats.eventManager.OnKilledTarget += OnKilledTarget;
        
        ObjectPool.InitialisePool<FrigidDeadProjectile>();
    }

    private void OnKilledTarget(Statboard statboard) {
        if (statboard.statusEffectManager.GetEffectFromList(GameConfig.Instance.freeze, out StatusEffect effect)) {
            if (ObjectPool.TryPull(out FrigidDeadProjectile projectile, statboard.transform.position + (Vector3.up * 3), Quaternion.Euler(0, Random.value * 360, 0))) {
                Damage dmg = damage.GetTickDamage(stacks);
                dmg.criticalChance = damage.damage.criticalChance;
                dmg.statusChance = damage.damage.statusChance;
                projectile.Init(dmg, this, stats);
            }
        }
    }
    
    public override void Remove() {
        base.Remove();

        stats.eventManager.OnKilledTarget -= OnKilledTarget;
    }
}