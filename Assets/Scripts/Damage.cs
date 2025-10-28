using System;
using System.Collections.Generic;
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

    public void TickStats() {
        foreach (var field in typeof(DamageTypeStats).GetFields()) {
            if (field.FieldType == typeof(Stat)) {
                Stat stat = (Stat)field.GetValue(this);
                stat.UpdateTimers();
            }
        }
    }
}

[Serializable]
public struct DamageInstance {
    public enum Type {
        Physical,
        Fire,
        Ice,
        Electric,
        Poison,
        Light
    }

    public Type type;
    public float amount;

    public DamageInstance(Type type, float amount) {
        this.type = type;
        this.amount = amount;
    }
}

[Serializable]
public struct StatusEffectInstance {
    public string key;
    public float duration;
    public int stacks;

    public StatusEffectInstance(string key, float duration, int stacks) {
        this.key = key;
        this.duration = duration;
        this.stacks = stacks;
    }
}

[Serializable]
public struct DamageInfo {
    public DamageInstance[] damageInstances;
    public Statboard source;
    public Dictionary<ClassReference<StatusEffect>, int> statusEffects;
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

    public DamageInfo(DamageInfo damageInfo)
    {
        damageInstances = (DamageInstance[])damageInfo.damageInstances.Clone();
        source = damageInfo.source;
        statusEffects = new(damageInfo.statusEffects);
        knockback = damageInfo.knockback;
        direction = damageInfo.direction;
        hitPoint = damageInfo.hitPoint;
        selfDamage = damageInfo.selfDamage;
        ignoreResistances = damageInfo.ignoreResistances;
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

    // --- PURE OPERATORS ---

    public static DamageInfo operator *(DamageInfo a, DamageTypeStats b)
    {
        return a.GetMultiplied(b);
    }

    public static DamageInfo operator /(DamageInfo a, DamageTypeStats b)
    {
        return a.GetResisted(b);
    }

    // --- PURE METHODS ---

    public DamageInfo GetMultiplied(DamageTypeStats stats)
    {
        DamageInfo result = new DamageInfo(this);

        for (int i = 0; i < result.damageInstances.Length; i++)
        {
            var damage = result.damageInstances[i];
            switch (damage.type)
            {
                case DamageInstance.Type.Physical:
                    damage.amount *= stats.physical.Value;
                    break;
                case DamageInstance.Type.Fire:
                    damage.amount *= stats.fire.Value;
                    break;
                case DamageInstance.Type.Ice:
                    damage.amount *= stats.ice.Value;
                    break;
                case DamageInstance.Type.Electric:
                    damage.amount *= stats.electric.Value;
                    break;
                case DamageInstance.Type.Poison:
                    damage.amount *= stats.poison.Value;
                    break;
                case DamageInstance.Type.Light:
                    damage.amount *= stats.light.Value;
                    break;
            }
            result.damageInstances[i] = damage;
        }

        return result;
    }

    public DamageInfo GetResisted(DamageTypeStats stats)
    {
        DamageInfo result = new DamageInfo(this);

        for (int i = 0; i < result.damageInstances.Length; i++)
        {
            var damage = result.damageInstances[i];
            switch (damage.type)
            {
                case DamageInstance.Type.Physical:
                    damage.amount *= 1 - stats.physical.Value;
                    break;
                case DamageInstance.Type.Fire:
                    damage.amount *= 1 - stats.fire.Value;
                    break;
                case DamageInstance.Type.Ice:
                    damage.amount *= 1 - stats.ice.Value;
                    break;
                case DamageInstance.Type.Electric:
                    damage.amount *= 1 - stats.electric.Value;
                    break;
                case DamageInstance.Type.Poison:
                    damage.amount *= 1 - stats.poison.Value;
                    break;
                case DamageInstance.Type.Light:
                    damage.amount *= 1 - stats.light.Value;
                    break;
            }
            result.damageInstances[i] = damage;
        }

        return result;
    }
}