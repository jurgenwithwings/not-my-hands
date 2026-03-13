using System;
using Stats;
using UnityEngine;

[Serializable] public class BarkBracers : Relic  {
    [SerializeField] private float baseValue = 10;
    [SerializeField] private float stackValue = 3;
    [SerializeField] private float maxReductionPerHitPercent = 0.7f;

    public override void Initialise(RelicManager relicManager, RelicData relicData) {
        base.Initialise(relicManager, relicData);
        
        BarkBracers config = data.relicClass as BarkBracers;
        baseValue = config.baseValue;
        stackValue = config.stackValue;
        maxReductionPerHitPercent = config.maxReductionPerHitPercent;

        stats.eventManager.OnPreTakeDamage += ReduceDamage;
    }

    private void ReduceDamage(ref DamageInfo damageInfo) {
        float originalDamage = damageInfo.finalDamage;
        float damageToReduce = GetStackValue(baseValue, stackValue, stacks);
        float damageCap = damageInfo.finalDamage * maxReductionPerHitPercent;
        if (damageToReduce > damageCap) {
            damageToReduce = damageCap;
        }
        
        damageInfo.AddModifier(-damageToReduce, ModifierType.FinalFlat);
    }

    public override void Remove() {
        base.Remove();
        
        stats.eventManager.OnPreTakeDamage -= ReduceDamage;
    }
}
