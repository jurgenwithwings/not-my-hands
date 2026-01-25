using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffect", menuName = "ScriptableObjects/StatusEffect", order = 1)]
public class StatusEffectData : ScriptableObject {
    public string displayName;
    public Sprite icon;
    public float maxDuration = 5;
    public int StacksLostOnDurationEnd = 999;
    public int maxStacks = 10;
    public float tickInterval = 1;
    
    public List<DamageInstance> baseDamage;
    public List<DamageInstance> damagePerStack;
    [SerializeReference, SubclassSelector] public StatusEffect statusEffectType;

    public Type Type() {
        return statusEffectType.GetType();
    }
}