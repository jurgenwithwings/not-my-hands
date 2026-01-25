using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatusEffectManager : MonoBehaviour, IStatboard
{
    public Statboard statboard { get; set; }
    public void StatboardFinishedSet() {
        statboard.eventManager.OnDamageTaken += HandleStatusEffects;
    }

    public List<StatusEffect> PrimaryStatusEffects { get; private set; } = new();
    public List<BuffEffect> BuffEffects { get; private set; } = new();
    
    public int TotalEffects => BuffEffects.Count + PrimaryStatusEffects.Count;

    private void HandleStatusEffects(DamageInfo damageInfo) {
        foreach (int effectNum in damageInfo.statusEffects.Values) {
            AddStacks(damageInfo, effectNum);
        }
    }

    private void OnDestroy() {
        statboard.eventManager.OnDamageTaken -= HandleStatusEffects;
        
        foreach (var effect in PrimaryStatusEffects) {
            effect.RemoveEffect();
        }
        foreach (var effect in BuffEffects) {
            effect.RemoveEffect();
        }
    }

    public void AddStacks(DamageInfo damageInfo, int stacks = 1) {
        foreach (var effectData in damageInfo.statusEffects.Keys) {
            if (typeof(BuffEffect).IsAssignableFrom(effectData.Type())) {
                if (GetBuffFromList(effectData.Type(), out BuffEffect effect)) {
                    effect.AddStack(damageInfo, stacks);
                }
                else {
                    BuffEffect newEffect = Activator.CreateInstance(effectData.Type()) as BuffEffect;
                    if (newEffect != null) {
                        newEffect.Initialise(effectData, this, damageInfo);
                        if (stacks > 1) {
                            newEffect.AddStack(damageInfo, stacks - 1);
                        }
                        BuffEffects.Add(newEffect);
                    }
                }
            }
            else {
                if (GetEffectFromList(effectData.Type(), out StatusEffect effect)) {
                    effect.AddStack(damageInfo, stacks);
                }
                else {
                    StatusEffect newEffect = Activator.CreateInstance(effectData.Type()) as StatusEffect;
                    if (newEffect != null) {
                        newEffect.Initialise(effectData, this, damageInfo);
                        if (stacks > 1) {
                            newEffect.AddStack(damageInfo, stacks - 1);
                        }
                        PrimaryStatusEffects.Add(newEffect);
                    }
                }
            }
        }
    }

    public void RemoveStacks(Type type, int stacks = 1) {
        if (typeof(BuffEffect).IsAssignableFrom(type)) {
            if (GetBuffFromList(type, out BuffEffect effect)) {
                effect.RemoveStacks(stacks);
            }
        }
        else {
            if (GetEffectFromList(type, out StatusEffect effect)) {
                effect.RemoveStacks(stacks);
            }
        }
    }

    public void RemoveEffect(Type type) {
        if (typeof(BuffEffect).IsAssignableFrom(type)) {
            if (GetBuffFromList(type, out BuffEffect effect)) {
                effect.RemoveEffect();
                BuffEffects.Remove(effect);
            }
        }
        else {
            if (GetEffectFromList(type, out StatusEffect effect)) {
                effect.RemoveEffect();
                PrimaryStatusEffects.Remove(effect);
            }
        }
    }

    private void Update() {
        for (int i = PrimaryStatusEffects.Count - 1; i >= 0; i--) {
            PrimaryStatusEffects[i].Tick();
        }

        for (int i = BuffEffects.Count - 1; i >= 0; i--) {
            BuffEffects[i].Tick();
        }
    }

    private bool GetEffectFromList(Type type, out StatusEffect effect) {
        effect = PrimaryStatusEffects.Find(e => e.GetType() == type);
        return effect != null;
    }
    
    private bool GetBuffFromList(Type type, out BuffEffect effect) {
        effect = BuffEffects.Find(e => e.GetType() == type);
        return effect != null;
    }
}