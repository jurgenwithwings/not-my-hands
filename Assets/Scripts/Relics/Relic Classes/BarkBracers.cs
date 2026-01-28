using System;
using UnityEngine;

[Serializable] public class BarkBracers : Relic  {
    [SerializeField] private StackableEffect stackableEffect;
    [SerializeField] private float maxReductionPerHitPercent = 0.7f;

    public override void Initialise(RelicManager relicManager, RelicData relicData) {
        base.Initialise(relicManager, relicData);
        
        BarkBracers config = data.relicClass as BarkBracers;
        stackableEffect = config.stackableEffect;
        maxReductionPerHitPercent = config.maxReductionPerHitPercent;

        stats.eventManager.OnPreApplyDamage += ReduceDamage;
    }

    private void ReduceDamage(ref DamageInfo damageInfo, Statboard victim) {
        float originalDamage = damageInfo.finalDamage;
        float damageToReduce = stackableEffect.effectValue(stacks);
        float damageCap = damageInfo.finalDamage * maxReductionPerHitPercent;
        if (damageToReduce > damageCap) {
            damageToReduce = damageCap;
        }
        damageInfo.AddFinalFlatModifier(-damageToReduce);
        PlayerHUDEvents.DebugText($"Damage Dealt: {originalDamage} | Damage Cap: {damageCap} | Max Can Reduce: {stackableEffect.effectValue(stacks)} | Reduction Result: {damageToReduce}");
    }

    public override void Remove() {
        base.Remove();
        
        stats.eventManager.OnPreApplyDamage -= ReduceDamage;
    }
}
