using System;
using System.Collections.Generic;
using System.Reflection;
using Stats;
using UnityEngine;

[Serializable]
public struct DamageTypeStats {
    public Stat physical;
    public Stat fire;
    public Stat ice;
    public Stat electric;
    public Stat poison;
    public Stat light;

    public DamageTypeStats(float defaultValue) {
        physical = defaultValue;
        fire = defaultValue;
        ice = defaultValue;
        electric = defaultValue;
        poison = defaultValue;
        light = defaultValue;
    }

    public List<Stat> GetStats() {
        List<Stat> stats = new List<Stat>();
        
        foreach (FieldInfo field in typeof(DamageTypeStats).GetFields()) {
            if (field.FieldType == typeof(Stat)) {
                Stat stat = (Stat)field.GetValue(this);
                stats.Add(stat);
            }
        }

        return stats;
    }
}

[Serializable]
public struct DamageInstance {
    public enum DamageType {
        Physical,
        Fire,
        Ice,
        Electric,
        Poison,
        Light
    }

    public DamageType damageType;
    public float amount;

    public DamageInstance(DamageType damageType, float amount) {
        this.damageType = damageType;
        this.amount = amount;
    }
}

[Serializable]
public struct DamageInfo {
    public DamageInstance[] damageInstances;
    public Statboard source;
    public Dictionary<StatusEffectData, int> statusEffects;
    public float knockback;
    public Vector3 direction;
    public Vector3 hitPoint;
    public bool selfDamage;
    public bool ignoreResistances;

    public float totalDamage
    {
        get
        {
            float total = 0f;
            foreach (var instance in damageInstances)
                total += instance.amount;
            return total;
        }
    }

    public DamageInfo(DamageInstance[] damageInstances, Statboard statboard)
    {
        knockback = 0f;
        direction = Vector3.zero;
        hitPoint = Vector3.zero;
        selfDamage = false;
        statusEffects = new();
        ignoreResistances = false;
        source = statboard;
        this.damageInstances = damageInstances;
    }

    public DamageInfo Copy() {
        DamageInfo copy = new DamageInfo();
        copy.damageInstances = (DamageInstance[])damageInstances.Clone();
        copy.source = source;
        copy.statusEffects = statusEffects;
        copy.knockback = knockback;
        copy.direction = direction;
        copy.hitPoint = hitPoint;
        copy.selfDamage = selfDamage;
        copy.ignoreResistances = ignoreResistances;

        return copy;
    }

    public DamageInstance[] GetDamagePercentages()
    {
        DamageInstance[] damagePercentages = new DamageInstance[damageInstances.Length];
        for (int i = 0; i < damageInstances.Length; i++)
        {
            damagePercentages[i] = damageInstances[i];
            damagePercentages[i].amount = totalDamage > 0 ? damageInstances[i].amount / totalDamage : 0;
        }
        return damagePercentages;
    }

    // --- PURE METHODS ---

    public void SetDamageMultipliers(DamageTypeStats stats)
    {
        for (int i = 0; i < damageInstances.Length; i++)
        {
            var damage = damageInstances[i];
            switch (damage.damageType)
            {
                case DamageInstance.DamageType.Physical:
                    damage.amount *= stats.physical.Value;
                    break;
                case DamageInstance.DamageType.Fire:
                    damage.amount *= stats.fire.Value;
                    break;
                case DamageInstance.DamageType.Ice:
                    damage.amount *= stats.ice.Value;
                    break;
                case DamageInstance.DamageType.Electric:
                    damage.amount *= stats.electric.Value;
                    break;
                case DamageInstance.DamageType.Poison:
                    damage.amount *= stats.poison.Value;
                    break;
                case DamageInstance.DamageType.Light:
                    damage.amount *= stats.light.Value;
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
                    damage.amount *= 1 - stats.physical.Value;
                    break;
                case DamageInstance.DamageType.Fire:
                    damage.amount *= 1 - stats.fire.Value;
                    break;
                case DamageInstance.DamageType.Ice:
                    damage.amount *= 1 - stats.ice.Value;
                    break;
                case DamageInstance.DamageType.Electric:
                    damage.amount *= 1 - stats.electric.Value;
                    break;
                case DamageInstance.DamageType.Poison:
                    damage.amount *= 1 - stats.poison.Value;
                    break;
                case DamageInstance.DamageType.Light:
                    damage.amount *= 1 - stats.light.Value;
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