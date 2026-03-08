using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityStatusEffectManager : MonoBehaviour, IStatboard {
    public Statboard statboard { get; set; }
    public void StatboardFinishedSet() {
        statboard.eventManager.OnDamageTaken += HandleStatusEffectsFromDamage;
    }

    public List<StatusEffect> StatusEffects { get; private set; } = new();
    public List<BuffEffect> BuffEffects { get; private set; } = new();
    
    public int TotalEffects => BuffEffects.Count + StatusEffects.Count;

    private void HandleStatusEffectsFromDamage(DamageInfo damageInfo) {
        // Handle Additional Status Effects
        foreach (StatusEffectData effect in damageInfo.additionalStatusEffects) {
            AddStacks(effect, damageInfo);
        }
        
        // Handle Status Effects From Damage
        if (damageInfo.ignoreResistances) return;
        
        float statusChance = damageInfo.source.statusChanceMultiplier * damageInfo.sourceStatusChance;
        //print($"Status Chance: {statusChance}"); //Pass
        int numOfEffects = Mathf.FloorToInt(statusChance);
        statusChance -= numOfEffects;

        float random = Random.value;
        if (random < statusChance) {
            numOfEffects++;
        }
        //print($"Number of Effects: {numOfEffects}"); //Pass
        
        float[] percentages = damageInfo.GetDamagePercentages();
        for (int i = 0; i < numOfEffects; i++) {
            // Calculates not including Physical.
            random = Random.Range(0, 1 - percentages[DamageType.Physical.Index()]);

            float runningTotal = 0;
            for (int j = 1; j < percentages.Length; j++) {
                runningTotal += percentages[j];
                if (random < runningTotal) {
                    switch (j) {
                        case 1:
                            AddStacks(GameConfig.Instance.burn, damageInfo);
                            break;
                        case 2:
                            AddStacks(GameConfig.Instance.freeze, damageInfo);
                            break;
                        case 3:
                            AddStacks(GameConfig.Instance.charged, damageInfo);
                            break;
                        case 4:
                            AddStacks(GameConfig.Instance.poison, damageInfo);
                            break;
                        case 5:
                            AddStacks(GameConfig.Instance.judged, damageInfo);
                            break;
                    }
                    break;
                }
            }
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
    
    public StatusEffect AddStacks(StatusEffectData effectData, DamageInfo damageInfo) {
        if (typeof(BuffEffect).IsAssignableFrom(effectData.Type())) {
            if (GetBuffFromList(effectData, out BuffEffect effect)) {
                effect.AddStack(damageInfo);
                return effect;
            }
            else {
                BuffEffect newEffect = Activator.CreateInstance(effectData.Type()) as BuffEffect;
                if (newEffect != null) {
                    newEffect.Initialise(effectData, this, damageInfo);
                    BuffEffects.Add(newEffect);
                    return newEffect;
                }
            }
        }
        else {
            if (GetEffectFromList(effectData, out StatusEffect effect)) {
                effect.AddStack(damageInfo);
                return effect;
            }
            else {
                StatusEffect newEffect = Activator.CreateInstance(effectData.Type()) as StatusEffect;
                if (newEffect != null) {
                    newEffect.Initialise(effectData, this, damageInfo);
                    StatusEffects.Add(newEffect);
                    return newEffect;
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