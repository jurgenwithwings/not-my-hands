using System;
using Stats;
using UnityEngine;

[Serializable] public class HollowVictory : Relic {
    [SerializeField] private StatusEffectData damageBuff;
    [SerializeField] private float damageBuffBaseValue;
    [SerializeField] private float damageBuffStackValue;
    [SerializeField] private int damageBuffBaseMaxStacks;
    [SerializeField] private int damageBuffStackMaxStacks;
    private HollowVictoryBuff buff;

    public override void Initialise(RelicManager relicManager, RelicData relicData) {
        base.Initialise(relicManager, relicData);
        
        HollowVictory config = data.relicClass as HollowVictory;
        damageBuff = config.damageBuff;
        damageBuffBaseValue = config.damageBuffBaseValue;
        damageBuffStackValue = config.damageBuffStackValue;
        damageBuffBaseMaxStacks = config.damageBuffBaseMaxStacks;
        damageBuffStackMaxStacks = config.damageBuffStackMaxStacks;
        
        stats.eventManager.OnKilledTarget += OnKilledTarget;
    }

    private void OnKilledTarget(Statboard obj) {
        DamageInfo damage = new(Array.Empty<DamageInstance>(), stats);
        damage.statusEffects.Add(damageBuff, 1);
        HollowVictoryBuff buff = stats.statusEffectManager.AddStacks(damage) as HollowVictoryBuff;
        buff?.UpdateValues(damageBuffBaseValue, damageBuffStackValue, (int)GetStackValue(damageBuffBaseMaxStacks, damageBuffStackMaxStacks, stacks));
    }

    public override void AddStack(int amount) {
        base.AddStack(amount);

        if (buff != null) {
            buff.UpdateValues(damageBuffBaseValue, damageBuffStackValue, (int)GetStackValue(damageBuffBaseMaxStacks, damageBuffStackMaxStacks, stacks));
        }
    }

    public override void RemoveStack() {
        base.RemoveStack();
        
        if (buff != null && stacks > 0) {
            buff.UpdateValues(damageBuffBaseValue, damageBuffStackValue, (int)GetStackValue(damageBuffBaseMaxStacks, damageBuffStackMaxStacks, stacks));
        }
    }

    public override void Remove() {
        base.Remove();
        
        stats.eventManager.OnKilledTarget -= OnKilledTarget;
    }
}

[Serializable] public class HollowVictoryBuff : BuffEffect {
    [SerializeField] private StackableEffect damageEffect;
    private int maxStacks;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        HollowVictoryBuff config = data.statusEffectClass as HollowVictoryBuff;
        damageEffect = config.damageEffect;
        maxStacks = data.maxStacks;
    }

    public void UpdateValues(float newBase, float newStack, int newMaxStacks) {
        damageEffect.baseValue = newBase;
        damageEffect.stackValue = newStack;
        maxStacks = newMaxStacks;
        
        ReplaceModifier();
    }

    private void ReplaceModifier() {
        Stat stat = stats.GetStatByEnum(damageEffect.statType);

        stat.RemoveModifier(damageEffect.currentModifier);
        damageEffect.currentModifier = new Modifier(damageEffect.effectValue(stacks), damageEffect.modifierType, "HollowVictory");
        stat.AddModifier(damageEffect.currentModifier);
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        this.stacks += stacks;
        if (this.stacks > maxStacks) {
            this.stacks = maxStacks;
        }
        
        if (damageInfo.finalDamage > highestDamageReceived.finalDamage) {
            highestDamageReceived = damageInfo;
        }

        if (Data.refillDurationWhenGainingStack) {
            currentDuration = Data.maxDuration;
        }
        
        ReplaceModifier();
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);

        if (stacks > 0) {
            ReplaceModifier();
        }
    }

    public override void RemoveEffect() {
        base.RemoveEffect();
        
        stats.GetStatByEnum(damageEffect.statType).RemoveModifier(damageEffect.currentModifier);
    }
}
