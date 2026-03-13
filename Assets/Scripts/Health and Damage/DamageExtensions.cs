using System;
using System.Collections.Generic;
using Stats;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public enum DamageType {
    Physical = 0,
    Fire = 1,
    Ice = 2,
    Electric = 3,
    Poison = 4,
    Light = 5,
}

[Serializable] public struct DamageInstance {
    public float baseAmount;
    
    public List<Modifier> modifiers;

    public DamageInstance(float baseAmount) {
        this.baseAmount = baseAmount;
        modifiers = new List<Modifier>();
        
        Initialised = true;
    }

    public bool Initialised { get; private set; }
    public void SetDefault() {
        if (!Initialised) {
            modifiers = new List<Modifier>();
            
            Initialised = true;
        }
    }
    
    // Resolve instance damage (no global modifiers applied yet)
    public float Resolve() {
        return Stat.CalculateFinalValue(baseAmount, modifiers.ToArray());
    }

    public void SetBaseAmount(float amount) {
        baseAmount = amount;
    }

    public void AddModifier(float amount, ModifierType modifierType = ModifierType.Additive, object source = null) {
        if (!Initialised) {
            SetDefault();
        }
        
        modifiers.Add(new Modifier(amount, modifierType, source));
    }
    
    // --- Operators ---
    public static implicit operator float(DamageInstance damage) {
        return damage.baseAmount;
    }

    public static implicit operator DamageInstance(float amount) {
        return new DamageInstance(amount);
    }
}

//Used to easily assign base damage in the inspector or script.
[Serializable] public struct Damage {
    public float physical;
    public float fire;
    public float ice;
    public float electric;
    public float poison;
    public float light;
    [Space] 
    public float criticalChance;
    public float statusChance;
    
    public DamageInstance[] ToArray() {
        return new DamageInstance[] { physical, fire, ice, electric, poison, light };
    }

    public static implicit operator DamageInstance[](Damage damage) {
        return damage.ToArray();
    }

    // Create Damage from DamageInstance Array.
    public Damage(DamageInstance[] damage) : this() { 
        physical = damage[DamageType.Physical.Index()].baseAmount;
        fire = damage[DamageType.Fire.Index()].baseAmount;
        ice = damage[DamageType.Ice.Index()].baseAmount;
        electric = damage[DamageType.Electric.Index()].baseAmount;
        poison = damage[DamageType.Poison.Index()].baseAmount;
        light = damage[DamageType.Light.Index()].baseAmount;
    }
}

[Serializable] public struct DamageInfo {
    // Damage Types
    public DamageInstance physicalDamage;
    public DamageInstance fireDamage;
    public DamageInstance iceDamage;
    public DamageInstance electricDamage;
    public DamageInstance poisonDamage;
    public DamageInstance lightDamage;
    
    // Critical
    public float sourceCriticalChance;
    
    // Status Effect
    public float sourceStatusChance;
    public List<StatusEffectData> additionalStatusEffects;
    
    // Additional Data
    public Statboard source;
    public Vector3 hitPoint;
    public bool selfDamage;
    public bool ignoreResistances;

    // Result Info
    public int resultingCritLevel;
    public List<StatusEffect> resultingAppliedEffects;

    public List<object> tags;
    
    public bool debug;

    public DamageInfo(Damage damage, Statboard source, Vector3 hitPoint) {
        // Damage Types
        DamageInstance[] dInst = damage.ToArray();
        
        physicalDamage = dInst[DamageType.Physical.Index()];
        fireDamage = dInst[DamageType.Fire.Index()];
        iceDamage = dInst[DamageType.Ice.Index()];
        electricDamage = dInst[DamageType.Electric.Index()];
        poisonDamage = dInst[DamageType.Poison.Index()];
        lightDamage = dInst[DamageType.Light.Index()];
    
        // Critical
        sourceCriticalChance = damage.criticalChance;
        
        // Status Effect
        sourceStatusChance = damage.statusChance;
        additionalStatusEffects = new();
        
        // Additional Data
        this.source = source;
        this.hitPoint = hitPoint;
        selfDamage = false;
        ignoreResistances = false;
        
        Initialised = true;
        
        resultingCritLevel = 0;
        resultingAppliedEffects = new();

        tags = new();
        
        debug = false;
    }
    
    public bool Initialised { get; private set; }
    public void SetDefault() {
        if (!Initialised) {
            
            Initialised = true;
        }
    }

    // Sum base without modifiers (optional convenience)
    public float baseDamage {
        get {
            DamageInstance[] damageInstances = this.DamageToArray();
            
            float total = 0f;
            foreach (var instance in damageInstances) {
                total += instance.baseAmount;
            }
            
            return total;
        }
    }

    public float finalDamage => ResolveFinalDamage().finalDamage;
    // Fully resolve including instance-level and hit-level modifiers
    public (float finalDamage, float[] individualDamage) ResolveFinalDamage() {
        DamageInstance[] damageInstances = this.DamageToArray();
        float[] individualDamage = new float[damageInstances.Length];
        
        float finalDamage = 0f;
        for (int i = 0; i < damageInstances.Length; i++) {
            var inst = damageInstances[i];
            if (!inst.Initialised) {
                inst.SetDefault();
            }
            individualDamage[i] = inst.Resolve();
            finalDamage += individualDamage[i];
        }

        return (finalDamage, individualDamage);
    }

    public DamageInfo Copy() {
        DamageInfo copy = this;
        copy.additionalStatusEffects = new(additionalStatusEffects);

        return copy;
    }

    public float[] GetDamagePercentages() {
        float[] result = new float[Enum.GetValues(typeof(DamageType)).Length];

        var resolve = ResolveFinalDamage();
        if (resolve.finalDamage == 0) {
            return result;
        }

        for (int i = 0; i < result.Length; i++) {
            result[i] = resolve.individualDamage[i] / resolve.finalDamage;
        }
        
        return result;
    }
    
    public void AddModifier(float amount, ModifierType modifierType = ModifierType.Additive, object source = null) {
        if (!Initialised) {
            SetDefault();
        }

        physicalDamage.AddModifier(amount, modifierType, source);
        fireDamage.AddModifier(amount, modifierType, source);
        iceDamage.AddModifier(amount, modifierType, source);
        electricDamage.AddModifier(amount, modifierType, source);
        poisonDamage.AddModifier(amount, modifierType, source);
        lightDamage.AddModifier(amount, modifierType, source);
        
    }

    // --- Helpers ---

    public static DamageInfo Empty(Statboard source, Vector3 hitPoint = default) {
        if (hitPoint == default) {
            hitPoint = source.transform.position;
        }

        return new DamageInfo(new Damage(), source, hitPoint);
    }

    public void SetDamageMultipliers(DamageTypeStats stats) {
        physicalDamage.AddModifier(stats.physical, ModifierType.FinalMultiplicative);
        fireDamage.AddModifier(stats.fire, ModifierType.FinalMultiplicative);
        iceDamage.AddModifier(stats.ice, ModifierType.FinalMultiplicative);
        electricDamage.AddModifier(stats.electric, ModifierType.FinalMultiplicative);
        poisonDamage.AddModifier(stats.poison, ModifierType.FinalMultiplicative);
        lightDamage.AddModifier(stats.light, ModifierType.FinalMultiplicative);
    }

    public void ApplyDamageMultipliersFromSource() {
        SetDamageMultipliers(source.damageMultipliers);
    }

    public void SetResistanceMultipliers(DamageTypeStats stats) {
        physicalDamage.AddModifier(1- stats.physical, ModifierType.FinalMultiplicative);
        fireDamage.AddModifier(1 - stats.fire, ModifierType.FinalMultiplicative);
        iceDamage.AddModifier(1 - stats.ice, ModifierType.FinalMultiplicative);
        electricDamage.AddModifier(1 - stats.electric, ModifierType.FinalMultiplicative);
        poisonDamage.AddModifier(1 - stats.poison, ModifierType.FinalMultiplicative);
        lightDamage.AddModifier(1 - stats.light, ModifierType.FinalMultiplicative);
    }
}

public static class DamageExtensions {
    public static string damageNumberFormat = "###,###,###,##0.#";
    
    public static string AbbreviateNumber(float number) {
        return AbbreviateNumber(number, out _);
    }
    
    public static string AbbreviateNumber(float number, out float resultingNumber) {
        if (number < 1000) {
            resultingNumber = number;
            return number.ToString(damageNumberFormat);
        }

        // Limited to Qi due to 64-bit constraints
        string[] suffixes = { "", "k", "M", "B", "T", "Qa", "Qi" };
        int suffixIndex = 0;
        float abbreviatedNumber = number;

        while (abbreviatedNumber >= 1000 && suffixIndex < suffixes.Length - 1) {
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

    public static DamageInstance[] DamageToArray(this DamageInfo damageInfo) {
        DamageInstance[] result = new[] {
            damageInfo.physicalDamage,
            damageInfo.fireDamage,
            damageInfo.iceDamage,
            damageInfo.electricDamage,
            damageInfo.poisonDamage,
            damageInfo.lightDamage,
        };
        return result;
    }
}