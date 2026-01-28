using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatusEffectManager : MonoBehaviour, IStatboard
{
    public Statboard statboard { get; set; }
    public void StatboardFinishedSet() {
        statboard.eventManager.OnDamageTaken += HandleStatusEffectsFromDamage;
    }

    public List<StatusEffect> StatusEffects { get; private set; } = new();
    public List<BuffEffect> BuffEffects { get; private set; } = new();
    
    public int TotalEffects => BuffEffects.Count + StatusEffects.Count;

    private void HandleStatusEffectsFromDamage(DamageInfo damageInfo) {
        foreach (int effectNum in damageInfo.statusEffects.Values) {
            AddStacks(damageInfo, effectNum);
        }
    }

    private void OnDestroy() {
        statboard.eventManager.OnDamageTaken -= HandleStatusEffectsFromDamage;
        
        foreach (var effect in StatusEffects) {
            effect.RemoveEffect();
        }
        foreach (var effect in BuffEffects) {
            effect.RemoveEffect();
        }
    }
    
    public StatusEffect AddStacks(DamageInfo damageInfo, int stacks = 1) {
        foreach (StatusEffectData effectData in damageInfo.statusEffects.Keys) {
            if (typeof(BuffEffect).IsAssignableFrom(effectData.Type())) {
                if (GetBuffFromList(effectData, out BuffEffect effect)) {
                    effect.AddStack(damageInfo, stacks);
                    return effect;
                }
                else {
                    BuffEffect newEffect = Activator.CreateInstance(effectData.Type()) as BuffEffect;
                    if (newEffect != null) {
                        newEffect.Initialise(effectData, this, damageInfo);
                        if (stacks > 1) {
                            newEffect.AddStack(damageInfo, stacks - 1);
                        }
                        BuffEffects.Add(newEffect);
                        return newEffect;
                    }
                }
            }
            else {
                if (GetEffectFromList(effectData, out StatusEffect effect)) {
                    effect.AddStack(damageInfo, stacks);
                    return effect;
                }
                else {
                    StatusEffect newEffect = Activator.CreateInstance(effectData.Type()) as StatusEffect;
                    if (newEffect != null) {
                        newEffect.Initialise(effectData, this, damageInfo);
                        if (stacks > 1) {
                            newEffect.AddStack(damageInfo, stacks - 1);
                        }
                        StatusEffects.Add(newEffect);
                        return newEffect;
                    }
                }
            }
        }
        return null;
    }

    public void RemoveStacks(StatusEffectData type, int stacks = 1) {
        if (typeof(BuffEffect).IsAssignableFrom(type.Type())) {
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

    public void RemoveEffect(StatusEffectData type) {
        if (typeof(BuffEffect).IsAssignableFrom(type.Type())) {
            if (GetBuffFromList(type, out BuffEffect effect)) {
                effect.RemoveEffect();
                BuffEffects.Remove(effect);
            }
        }
        else {
            if (GetEffectFromList(type, out StatusEffect effect)) {
                effect.RemoveEffect();
                StatusEffects.Remove(effect);
            }
        }
    }

    private void Update() {
        for (int i = StatusEffects.Count - 1; i >= 0; i--) {
            StatusEffects[i].Update();
        }

        for (int i = BuffEffects.Count - 1; i >= 0; i--) {
            BuffEffects[i].Update();
        }
    }

    public bool GetEffectFromList(StatusEffectData dataType) {
        return GetEffectFromList(dataType, out _);
    }

    public bool GetEffectFromList(StatusEffectData dataType, out StatusEffect effect) {
        effect = StatusEffects.Find(e => e.Data == dataType);
        return effect != null;
    }

    public bool GetBuffFromList(StatusEffectData dataType) {
        return GetBuffFromList(dataType, out _);
    }

    public bool GetBuffFromList(StatusEffectData dataType, out BuffEffect effect) {
        effect = BuffEffects.Find(e => e.Data == dataType);
        return effect != null;
    }
}