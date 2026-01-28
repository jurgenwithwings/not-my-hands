using System;
using Stats;
using UnityEngine;

[Serializable]
public abstract class Relic {
    protected RelicManager manager;
    public RelicData data { get; private set; }
    
    protected Statboard stats => manager.statboard;

    public int stacks { get; private set; }
    
    public virtual void Initialise(RelicManager relicManager, RelicData relicData) {
        manager = relicManager;
        data = relicData;
    }

    public virtual void AddStack(int amount) {
        stacks += amount;
    }

    public virtual void RemoveStack() {
        stacks--;
        if (stacks <= 0) {
            manager.RemoveRelic(data);
        }
    }

    public virtual void Remove() {
        
    }

    public virtual void Tick() {
        
    }
    
    // -- Helpers --
    protected void ReplaceModifier(Statboard.VariableType statType, ModifierType modifierType, float baseValue, float stackValue, object source, ref Modifier currentModifier) {
        Stat stat = stats.GetStatByEnum(statType);
        if (currentModifier == null) {
            stat.RemoveAllModifiersFromSource($"{GetType()}");
        }
        else {
            stat.RemoveModifier(currentModifier);
        }
        currentModifier = new Modifier(baseValue + (stackValue * (stacks -1)), modifierType, source);
        stat.AddModifier(currentModifier);
    }

    protected void ReplaceModifier(ref StackableEffect stackableEffect, object source) {
        ReplaceModifier(stackableEffect.statType, stackableEffect.modifierType, stackableEffect.baseValue, stackableEffect.stackValue, source, ref stackableEffect.currentModifier);
    }
}

[Serializable] public struct StackableEffect {
    public Statboard.VariableType statType;
    public float baseValue;
    public float stackValue;
    public ModifierType modifierType;
    [HideInInspector] public Modifier currentModifier;
    public float effectValue(int stacks) => baseValue + (stackValue * (stacks - 1));
}

[Serializable] public class BasicModifierRelic : Relic {
    [SerializeField] protected string source;
    [SerializeField] protected StackableEffect[] stackableEffects;

    public override void Initialise(RelicManager relicManager, RelicData relicData) {
        base.Initialise(relicManager, relicData);
        
        BasicModifierRelic config = data.relicClass as BasicModifierRelic;
        source = string.IsNullOrEmpty(config.source) ? GetType().ToString() : config.source;
        stackableEffects = config.stackableEffects ?? Array.Empty<StackableEffect>();
    }

    public override void AddStack(int amount) {
        base.AddStack(amount);

        for (int i = 0; i < stackableEffects.Length; i++) {
            StackableEffect effect = stackableEffects[i];
            ReplaceModifier(effect.statType, effect.modifierType, effect.baseValue, effect.stackValue, source, ref effect.currentModifier);
            stackableEffects[i] = effect;
        }
    }

    public override void RemoveStack() {
        base.RemoveStack();
        
        for (int i = 0; i < stackableEffects.Length; i++) {
            StackableEffect effect = stackableEffects[i];
            ReplaceModifier(effect.statType, effect.modifierType, effect.baseValue, effect.stackValue, source, ref effect.currentModifier);
            stackableEffects[i] = effect;
        }
    }

    public override void Remove() {
        base.Remove();

        foreach (StackableEffect effect in stackableEffects) {
            stats.GetStatByEnum(effect.statType).RemoveModifier(effect.currentModifier);
        }
    }
}