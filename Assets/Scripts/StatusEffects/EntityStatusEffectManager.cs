using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatusEffectManager : MonoBehaviour
{
    public Statboard statboard { get; private set; }
    public void SetStatboard(Statboard board) {
        if (statboard == null) {
            statboard = board;
        }
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
        foreach (var effectType in damageInfo.statusEffects.Keys) {
            if (typeof(BuffEffect).IsAssignableFrom(effectType)) {
                if (GetBuffFromList(effectType, out BuffEffect effect)) {
                    effect.AddStack(damageInfo, stacks);
                }
                else {
                    BuffEffect newEffect = (BuffEffect)effectType.CreateInstance();
                    if (newEffect != null) {
                        newEffect.Initialise(this, damageInfo);
                        if (stacks > 1) {
                            newEffect.AddStack(damageInfo, stacks - 1);
                        }
                        BuffEffects.Add(newEffect);
                    }
                }
            }
            else {
                if (GetEffectFromList(effectType, out StatusEffect effect)) {
                    effect.AddStack(damageInfo, stacks);
                }
                else {
                    StatusEffect newEffect = effectType.CreateInstance();
                    if (newEffect != null) {
                        newEffect.Initialise(this, damageInfo);
                        if (stacks > 1) {
                            newEffect.AddStack(damageInfo, stacks - 1);
                        }
                        PrimaryStatusEffects.Add(newEffect);
                    }
                }
            }
        }
    }

    public void RemoveStacks(ClassReference<StatusEffect> type, int stacks = 1) {
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

    public void RemoveEffect(ClassReference<StatusEffect> type) {
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

    private bool GetEffectFromList(ClassReference<StatusEffect> type, out StatusEffect effect) {
        effect = PrimaryStatusEffects.Find(e => e.GetType() == type);
        return effect != null;
    }
    
    private bool GetBuffFromList(ClassReference<StatusEffect> type, out BuffEffect effect) {
        effect = BuffEffects.Find(e => e.GetType() == type);
        return effect != null;
    }
}