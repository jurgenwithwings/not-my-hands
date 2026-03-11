using System;
using System.Collections.Generic;
using ObjectPooling;
using UnityEngine;

public class EffectBar : MonoBehaviour {
    protected Statboard statboard;
    protected EntityStatusEffectManager effectManager => statboard.statusEffectManager;
    
    protected Dictionary<Type, EffectBarIcon> effects = new();

    private void Awake() {
        ObjectPool.InitialisePool<EffectBarIcon>();
    }

    public virtual void Init(Statboard board) {
        if (statboard != null) return;
        statboard = board;
        
        statboard.eventManager.OnReceivedStatusEffect += OnReceivedStatusEffect;
        statboard.eventManager.OnRemovedStatusEffect += OnRemovedStatusEffect;
    }

    protected void OnDestroy() {
        statboard.eventManager.OnReceivedStatusEffect -= OnReceivedStatusEffect;
        statboard.eventManager.OnRemovedStatusEffect -= OnRemovedStatusEffect;
    }

    private void OnReceivedStatusEffect(DamageInfo info) {
        foreach (StatusEffect effect in info.resultingAppliedEffects) {
            if (effect.Data.icon == null) return;
            
            if (effects.ContainsKey(effect.GetType())) return;
            
            // New Status Effect
            if (!ObjectPool.TryPull(out EffectBarIcon icon)) return;
            icon.transform.SetParent(transform, false);
            icon.Init(effect);
            effects.Add(effect.GetType(), icon);
        }
    }
    
    private void OnRemovedStatusEffect(StatusEffect effect) {
        if (effect.Data.icon == null) return;

        if (effects.ContainsKey(effect.GetType())) {
            effects.Remove(effect.GetType());
        }
    }
}