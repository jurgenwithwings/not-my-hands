using System;
using System.Collections.Generic;
using Stats;
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
    
    // Modifier buckets
    [HideInInspector] public float flat;            // +X damage
    [HideInInspector] public float additive;        // +X% damage (as decimal, 0.25 = +25%)
    [HideInInspector] public float multiplicative;  // x multiplier
    [HideInInspector] public float final;           // applied at the end

    public DamageInstance(float baseAmount) {
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
                if (amount <= 0.01f) break;
                multiplicative *= (1 + amount);
                break;
            case ModifierType.Final:
                if (amount <= 0.01f) break;
                final *= amount;
                break;
        }
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
    public float statusChance;
    
    public DamageInstance[] ToArray() {
        return new DamageInstance[] { physical, fire, ice, electric, poison, light };
    }

    public static implicit operator DamageInstance[](Damage damage) {
        return damage.ToArray();
    }

    // Create Damage from DamageInstance Array.
    public Damage(DamageInstance[] damage) : this() {
        for (int i = 0; i < damage.Length; i++) {
            switch (i) {
                case 0:
                    physical = damage[i].baseAmount; break;
                case 1:
                    fire = damage[i].baseAmount; break;
                case 2:
                    ice = damage[i].baseAmount; break;
                case 3:
                    electric = damage[i].baseAmount; break;
                case 4:
                    poison = damage[i].baseAmount; break;
                case 5:
                    light = damage[i].baseAmount; break;
            }
        }
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
    
    // Status Effect
    public float sourceStatusChance;
    public List<StatusEffectData> additionalStatusEffects;
    
    // Additional Data
    public Statboard source;
    public Vector3 hitPoint;
    public bool selfDamage;
    public bool ignoreResistances;

    // Hit-level Universal Modifiers
    public float flatAll;            // +X to all types
    public float additiveAll;        // +X% to all types (0.20 = +20%)
    public float multiplicativeAll;  // x multiplier to all damage
    public float finalAll;           // final multiplier for whole hit
    public float finalFlatAll;       // final +X damage to the whole hit.

    
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
    
        // Status Effect
        sourceStatusChance = damage.statusChance;
        additionalStatusEffects = new();
        
        // Additional Data
        this.source = source;
        this.hitPoint = hitPoint;
        selfDamage = false;
        ignoreResistances = false;
        
        // Hit-level Universal Modifiers
        flatAll = 0f;
        additiveAll = 0f;
        multiplicativeAll = 1f;
        finalAll = 1f;
        finalFlatAll = 0f;
        
        Initialised = true;
        
        debug = false;
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
            individualDamage[i] = inst.baseAmount.GetModifiedValue(inst.flat + flatAll, inst.additive + additiveAll, 
                inst.multiplicative * multiplicativeAll, inst.final * finalAll, debug) + finalFlatAll;;
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
                if (amount <= 0.01f) break;
                multiplicativeAll *= (1 +amount);
                break;
            case ModifierType.Final:
                if (amount <= 0.01f) break;
                finalAll *= amount;
                break;
        }
    }

    public void AddFinalFlatModifier(float amount) {
        finalFlatAll += amount;
    }

    // --- Helpers ---

    /*public static DamageInstance[] EmptyDamageArray() {
        return new DamageInstance[Enum.GetValues(typeof(DamageType)).Length];
    }*/

    public static DamageInfo Empty(Statboard source, Vector3 hitPoint = default) {
        if (hitPoint == default) {
            hitPoint = source.transform.position;
        }

        return new DamageInfo(new Damage(), source, hitPoint);
    }

    public void SetDamageMultipliers(DamageTypeStats stats) {
        physicalDamage.AddModifier(stats.physical, ModifierType.Final);
        fireDamage.AddModifier(stats.fire, ModifierType.Final);
        iceDamage.AddModifier(stats.ice, ModifierType.Final);
        electricDamage.AddModifier(stats.electric, ModifierType.Final);
        poisonDamage.AddModifier(stats.poison, ModifierType.Final);
        lightDamage.AddModifier(stats.light, ModifierType.Final);
    }

    public void SetResistanceMultipliers(DamageTypeStats stats)
    {
        physicalDamage.AddModifier(1- stats.physical, ModifierType.Final);
        fireDamage.AddModifier(1 - stats.fire, ModifierType.Final);
        iceDamage.AddModifier(1 - stats.ice, ModifierType.Final);
        electricDamage.AddModifier(1 - stats.electric, ModifierType.Final);
        poisonDamage.AddModifier(1 - stats.poison, ModifierType.Final);
        lightDamage.AddModifier(1 - stats.light, ModifierType.Final);
    }
}

public static class DamageExtensions {
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

    public static int Index(this DamageType damageType) {
        return (int)damageType;
    }

    public static DamageInstance[] DamageToArray(this DamageInfo damageInfo) {
        DamageInstance[] result = new[] {
            damageInfo.physicalDamage,
            damageInfo.fireDamage,
            damageInfo.iceDamage,
            damageInfo.poisonDamage,
            damageInfo.electricDamage,
            damageInfo.poisonDamage,
        };
        return result;
    }
}