using System;
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
public struct DamageInfo {
    public DamageInstance[] damageInstances;
    public Statboard source;
    public float knockback;
    public Vector3 direction;
    public Vector3 hitPoint;
    public bool selfDamage;
    
    public float totalDamage {
        get {
            float total = 0;
            foreach (var instance in damageInstances) {
                total += instance.amount;
            }
            return total;
        }
    }

    public DamageInfo(DamageInstance[] damageInstances, Statboard statboard) {
        knockback = 0;
        direction = Vector3.zero;
        hitPoint = Vector3.zero;
        selfDamage = false;
        
        source = statboard;
        
        this.damageInstances = damageInstances;
    }
    
    public DamageInstance[] GetDamagePercentages() {
        DamageInstance[] damagePercentages = new DamageInstance[damageInstances.Length];
        for (int i = 0; i < damageInstances.Length; i++) {
            damagePercentages[i] = damageInstances[i];
            damagePercentages[i].amount = totalDamage > 0 ? damageInstances[i].amount / totalDamage : 0;
        }

        return damagePercentages;
    }

    public void ScaleDamage(DamageTypeStats stats) {
        for (var i = 0; i < damageInstances.Length; i++) {
            var damage = damageInstances[i];
            switch (damage.type) {
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
            damageInstances[i] = damage;
        }
    }
}