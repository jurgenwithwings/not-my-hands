using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffect", menuName = "ScriptableObjects/StatusEffect", order = 1)]
public class StatusEffectData : ScriptableObject {
    public string displayName;
    public Sprite icon;
    public float maxDuration = 5;
    public int StacksLostOnDurationEnd = 999;
    public int maxStacks = 10;
    public bool refillDurationWhenGainingStack = true;
    [SerializeReference, SubclassSelector] public StatusEffect statusEffectClass;

    public Type Type() {
        return statusEffectClass.GetType();
    }
}