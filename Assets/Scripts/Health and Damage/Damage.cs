using System;
using System.Collections.Generic;
using System.Reflection;
using Stats;
using UnityEngine;

[Serializable] public struct DamageInstance {
    public enum DamageType {
        Physical,
        Fire,
        Ice,
        Electric,
        Poison,
        Light
    }

    public DamageType damageType;
    public float baseAmount;
    // Modifier buckets
    [HideInInspector] public float flat;            // +X damage
    [HideInInspector] public float additive;        // +X% damage (as decimal, 0.25 = +25%)
    [HideInInspector] public float multiplicative;  // x multiplier
    [HideInInspector] public float final;           // applied at the end

    public DamageInstance(DamageType damageType, float baseAmount) {
        this.damageType = damageType;
        this.baseAmount = baseAmount;
        flat = 0f;
        additive = 0f;
        multiplicative = 1f;
        final = 1f;
        Initialised = true;
    }

    public bool Initialised { get; private set; }
    public void SetDefault() {
        if (!Initialised) {
            flat = 0f;
            additive = 0f;
            multiplicative = 1f;
            final = 1f;
            
            Initialised = true;
        }
    }
    
    // Resolve instance damage (no global modifiers applied yet)
    public float Resolve() {
        float value = baseAmount + flat;
        value *= (1f + additive);
        value *= multiplicative;
        value *= final;
        return value;
    }

    public void SetBaseAmount(float amount) {
        baseAmount = amount;
    }

    public void AddModifier(float amount, ModifierType modifierType = ModifierType.Additive) {
        if (!Initialised) {
            SetDefault();
        }

        switch (modifierType) {
            case ModifierType.BaseAdd:
                flat += amount;
                break;
            case ModifierType.Additive:
                additive += amount;
                break;
            case ModifierType.Multiply:
                multiplicative *= (1 +amount);
                break;
            case ModifierType.Final:
                final *= amount;
                break;
        }
    }
    
    // --- Operators ---
    public static implicit operator float(DamageInstance damage) {
        return damage.baseAmount;
    }
}

[Serializable] public struct DamageInfo {
    public DamageInstance[] damageInstances;
    public Statboard source;
    public Dictionary<StatusEffectData, int> statusEffects;
    public Vector3 hitPoint;
    public bool selfDamage;
    public bool ignoreResistances;

    // Hit-level modifier buckets
    public float flatAll;            // +X to all types
    public float additiveAll;        // +X% to all types (0.20 = +20%)
    public float multiplicativeAll;  // x multiplier to all damage
    public float finalAll;           // final multiplier for whole hit

    public bool debug;

    public DamageInfo(DamageInstance[] damageInstances, Statboard statboard) {
        hitPoint = Vector3.zero;
        selfDamage = false;
        ignoreResistances = false;
        statusEffects = new();

        flatAll = 0f;
        additiveAll = 0f;
        multiplicativeAll = 1f;
        finalAll = 1f;

        Initialised = true;
        
        debug = false;
        
        source = statboard;
        this.damageInstances = damageInstances;
    }
    
    public bool Initialised { get; private set; }
    public void SetDefault() {
        if (!Initialised) {
            flatAll = 0f;
            additiveAll = 0f;
            multiplicativeAll = 1f;
            finalAll = 1f;
            
            Initialised = true;
        }
    }

    // Sum base without modifiers (optional convenience)
    public float baseDamage {
        get {
            float total = 0f;
            foreach (var instance in damageInstances)
                total += instance.baseAmount;
            return total;
        }
    }

    public float finalDamage => ResolveFinalDamage();
    // Fully resolve including instance-level and hit-level buckets
    public float ResolveFinalDamage() {
        float total = 0f;

        for (int i = 0; i < damageInstances.Length; i++) {
            var inst = damageInstances[i];
            if (!inst.Initialised) {
                inst.SetDefault();
            }
            total += inst.baseAmount.GetModifiedValue(inst.flat + flatAll, inst.additive + additiveAll, 
                inst.multiplicative * multiplicativeAll, inst.final * finalAll, debug);
        }

        return total;
    }

    public DamageInfo Copy() {
        DamageInfo copy = this;
        copy.damageInstances = damageInstances.Clone() as DamageInstance[];

        return copy;
    }

    /*public DamageInstance[] GetDamagePercentages()
    {
        DamageInstance[] damagePercentages = new DamageInstance[damageInstances.Length];
        for (int i = 0; i < damageInstances.Length; i++)
        {
            damagePercentages[i] = damageInstances[i];
            damagePercentages[i].baseAmount = finalDamage > 0 ? damageInstances[i].FinalDamage / finalDamage : 0;
        }
        return damagePercentages;
    }*/
    
    public void AddModifier(float amount, ModifierType modifierType = ModifierType.Additive) {
        if (!Initialised) {
            SetDefault();
        }

        switch (modifierType) {
            case ModifierType.BaseAdd:
                flatAll += amount;
                break;
            case ModifierType.Additive:
                additiveAll += amount;
                break;
            case ModifierType.Multiply:
                multiplicativeAll *= (1 +amount);
                break;
            case ModifierType.Final:
                finalAll *= amount;
                break;
        }
    }

    // --- Helpers ---

    public void SetDamageMultipliers(DamageTypeStats stats)
    {
        for (int i = 0; i < damageInstances.Length; i++)
        {
            var damage = damageInstances[i];
            switch (damage.damageType)
            {
                case DamageInstance.DamageType.Physical:
                    damage.SetBaseAmount(damage.baseAmount * stats.physical.Value);
                    break;
                case DamageInstance.DamageType.Fire:
                    damage.SetBaseAmount(damage.baseAmount * stats.fire.Value);
                    break;
                case DamageInstance.DamageType.Ice:
                    damage.SetBaseAmount(damage.baseAmount * stats.ice.Value);
                    break;
                case DamageInstance.DamageType.Electric:
                    damage.SetBaseAmount(damage.baseAmount * stats.electric.Value);
                    break;
                case DamageInstance.DamageType.Poison:
                    damage.SetBaseAmount(damage.baseAmount * stats.poison.Value);
                    break;
                case DamageInstance.DamageType.Light:
                    damage.SetBaseAmount(damage.baseAmount * stats.light.Value);
                    break;
            }
            damageInstances[i] = damage;
        }
    }

    public void SetResistanceMultipliers(DamageTypeStats stats)
    {
        for (int i = 0; i < damageInstances.Length; i++)
        {
            var damage = damageInstances[i];
            switch (damage.damageType)
            {
                case DamageInstance.DamageType.Physical:
                    damage.SetBaseAmount(damage.baseAmount * (1 - stats.physical.Value));
                    break;
                case DamageInstance.DamageType.Fire:
                    damage.SetBaseAmount(damage.baseAmount * (1 - stats.fire.Value));
                    break;
                case DamageInstance.DamageType.Ice:
                    damage.SetBaseAmount(damage.baseAmount * (1 - stats.ice.Value));
                    break;
                case DamageInstance.DamageType.Electric:
                    damage.SetBaseAmount(damage.baseAmount * (1 - stats.electric.Value));
                    break;
                case DamageInstance.DamageType.Poison:
                    damage.SetBaseAmount(damage.baseAmount * (1 - stats.poison.Value));
                    break;
                case DamageInstance.DamageType.Light:
                    damage.SetBaseAmount(damage.baseAmount * (1 - stats.light.Value));
                    break;
            }
            damageInstances[i] = damage;
        }
    }
}

public static class Damage {
    public static string AbbreviateNumber(float number)
    {
        return AbbreviateNumber(number, out _);
    }
    
    public static string AbbreviateNumber(float number, out float resultingNumber)
    {
        if (number < 1000) {
            resultingNumber = number;
            return number.ToString();
        }

        // Limited to Qi due to 64-bit constraints
        string[] suffixes = { "", "k", "M", "B", "T", "Qa", "Qi" };
        int suffixIndex = 0;
        float abbreviatedNumber = number;

        while (abbreviatedNumber >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            abbreviatedNumber /= 1000;
            suffixIndex++;
        }

        // Format string depends on the value
        string format;
        if (suffixIndex == 0) {
            format = "0";
        }
        else {
            format = /*abbreviatedNumber >= 100 ? "0.##" : */"0.#";
        }
        
        resultingNumber = abbreviatedNumber;
        return $"{abbreviatedNumber.ToString(format)}{suffixes[suffixIndex]}";
    }
}