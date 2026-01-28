using System;
using Stats;
using UnityEngine;

[Serializable] public class Windward : Relic  {
    [SerializeField] private StackableEffect speedEffect;
    [SerializeField] private StackableEffect durationEffect;
    [SerializeField] private StatusEffectData buffData;
    private float buffActiveTime;
    
    private WindwardBuff speedBuff;

    public override void Initialise(RelicManager relicManager, RelicData relicData) {
        base.Initialise(relicManager, relicData);

        Windward config = data.relicClass as Windward;
        speedEffect = config.speedEffect;
        durationEffect = config.durationEffect;
        buffData = config.buffData;

        stats.eventManager.OnDamageTaken += ApplyBuff;
    }

    private void ApplyBuff(DamageInfo obj) {
        DamageInfo statusDamage = new(Array.Empty<DamageInstance>(), stats);
        statusDamage.statusEffects.Add(buffData, 1);
        speedBuff = stats.statusEffectManager.AddStacks(statusDamage) as WindwardBuff;
        speedBuff.ApplyStats(speedEffect.effectValue(stacks), durationEffect.effectValue(stacks));
    }

    public override void Remove() {
        base.Remove();

        stats.eventManager.OnDamageTaken -= ApplyBuff;
    }
}

[Serializable] public class WindwardBuff : BuffEffect {
    [SerializeField] private float value;
    [SerializeField] private ModifierType buffModifierType;
    [SerializeField] private Statboard.VariableType targetStat;
    private Modifier mod;
    private float maxDuration;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        WindwardBuff config = data.statusEffectClass as WindwardBuff;
        value = config.value;
        buffModifierType = config.buffModifierType;
        targetStat = config.targetStat;
    }

    public void ApplyStats(float value, float duration) {
        this.value = value;
        maxDuration = duration;
        
        ReplaceModifier();
        currentDuration = maxDuration;
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);

        ReplaceModifier();
    }

    public override void Update() {
        base.Update();
        
        PlayerHUDEvents.DebugText($"Value {value} | Max Duration {maxDuration} | Current Duration {currentDuration}", 0.1f, "Windward");
    }

    private void ReplaceModifier() {
        Stat stat = stats.GetStatByEnum(targetStat);

        stat.RemoveModifier(mod);
        mod = new Modifier(value, buffModifierType, "Windward");
        stat.AddModifier(mod);
        
        currentDuration = maxDuration;
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);
        if (this.stacks > 0) {
            ReplaceModifier();
        }
    }

    public override void RemoveEffect() {
        base.RemoveEffect();
        
        stats.moveSpeed.RemoveModifier(mod);
    }
}
