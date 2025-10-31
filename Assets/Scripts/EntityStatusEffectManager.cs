using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatusEffectManager : MonoBehaviour
{
    public Statboard statboard { get; private set; }
    public void SetStatboard(Statboard board) {
        statboard ??= board;
        statboard.eventManager.OnDamageTaken += HandleStatusEffects;
    }
    
    public List<StatusEffect> activeEffects { get; private set; } = new();

    private void HandleStatusEffects(DamageInfo damageInfo) {
        foreach (var effectType in damageInfo.statusEffects.Keys) {
            AddStacks(damageInfo, damageInfo.statusEffects[effectType]);
        }
    }

    private void OnDestroy() {
        statboard.eventManager.OnDamageTaken -= HandleStatusEffects;
    }

    public void AddStacks(DamageInfo damageInfo, int stacks = 1) {
        foreach (var effectType in damageInfo.statusEffects.Keys) {
            if (GetEffectFromList(effectType, out StatusEffect effect)) {
                effect.AddStack(damageInfo, stacks);
            }
            else {
                StatusEffect newEffect = effectType.CreateInstance();
                if (newEffect != null) {
                    newEffect.Initialize(this, damageInfo);
                    activeEffects.Add(newEffect);
                }
            }
        }
    }

    public void RemoveStacks(ClassReference<StatusEffect> type, int stacks = 1) {
        if (GetEffectFromList(type, out StatusEffect effect)) {
            effect.RemoveStacks(stacks);
        }
    }

    public void RemoveEffect(ClassReference<StatusEffect> type) {
        if (GetEffectFromList(type, out StatusEffect effect)) {
            effect.RemoveEffect();
            activeEffects.Remove(effect);
        }
    }

    private void Update() {
        for (int i = activeEffects.Count - 1; i >= 0; i--) {
            activeEffects[i].Tick();
        }
    }
    
    private void LoadDataAsync(string key, Action onComplete) {
        
    }

    private bool GetEffectFromList(ClassReference<StatusEffect> type, out StatusEffect effect) {
        effect = activeEffects.Find(e => e.GetType() == type);
        return effect != null;
    }
}