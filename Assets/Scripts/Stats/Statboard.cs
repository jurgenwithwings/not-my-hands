using System;
using System.Collections.Generic;
using System.Reflection;
using Stats;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EntityEventManager))]
[RequireComponent(typeof(EntityStatusEffectManager))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(RelicManager))] 
public class Statboard : MonoBehaviour {
    [Section("References")]
    [Button(nameof(TryGetReferences), "Retrieve References", r:0.2f, g:0.2f, b:0.2f)]
    public EntityEventManager eventManager;
    public EntityStatusEffectManager statusEffectManager;
    public Health health;
    public Mana mana;
    public RelicManager relicManager;
    public OrganManager organManager;
    public HealthBar healthBar;
    public new Rigidbody rigidbody;
    
    public List<Stat> allStats { get; private set; } = new();
    
    [Section("Heart")]
    public Stat maxHealth = 100;
    public Stat damageResistance = 0;
    public Stat healingEffectiveness = 1;
    public Stat passiveRegenRate = 0;
    
    [Section("Brain")]
    public Stat criticalChanceMultiplier = 1;
    public Stat criticalDamageMultiplier = 1;
    public Stat luck = 1;

    [Section("Liver")] 
    public Stat statusChanceMultiplier = 1;
    public Stat buffDurationMultiplier = 1;
    public Stat buffPotencyMultiplier = 1;
    
    [Section("Movement")]
    public Stat moveSpeed = 8;
    public Stat jumpCount = 1;
    
    [Section("Mana")]
    public Stat maxMana = 30;
    public Stat manaRegenRate = 2;
    
    [Section("Combat")]
    public Stat damageMultiplier = 1;
    public Stat meleeDamageMultiplier = 1;
    public Stat rangedDamageMultiplier = 1;
    public Stat elementalDamageMultiplier = 1;
    public Stat projectileSpeedMultiplier = 1;
    
    [Section("Elemental")]
    public DamageTypeStats damageMultipliers = new(1);
    public DamageTypeStats damageResistances = new(0);

    [Section("Misc")] 
    public Stat currencyMultiplier = 1;

    private void Awake() {
        TryAssignReferences();
        
        CollectAllStatsToList();
    }
    
    private void CollectAllStatsToList() {
        foreach (FieldInfo field in typeof(Statboard).GetFields()) {
            if (field.FieldType == typeof(Stat)) {
                Stat stat = (Stat)field.GetValue(this);
                allStats.Add(stat);
            }else if (field.FieldType == typeof(DamageTypeStats)) {
                DamageTypeStats stats = (DamageTypeStats)field.GetValue(this);
                allStats.AddRange(stats.GetStats());
            }
        }
    }

    private void Update() {
        foreach (Stat stat in allStats) {
            stat.UpdateTimers();
        }
    }

    public Stat GetStatByName(string statName)
    {
        var field = typeof(Statboard).GetField(statName);
        if (field != null && field.FieldType == typeof(Stat))
        {
            return (Stat)field.GetValue(this);
        }
        Debug.LogWarning($"Stat '{statName}' not found on Statboard.");
        return null;
    }

    public enum VariableType {
        //Heart
        MaxHealth,
        DamageResistance,
        HealingEffectiveness,
        PassiveRegenRate,
        
        //Brain
        CriticalChanceMultiplier,
        CriticalDamageMultiplier,
        Luck,
        
        //Liver
        StatusChanceMultiplier,
        BuffDurationMultiplier,
        BuffPotencyMultiplier,
        
        //Movement
        MoveSpeed,
        JumpCount,
        
        //Mana
        MaxMana,
        ManaRegenRate,
        
        //Combat
        DamageMultiplier,
        MeleeDamageMultiplier,
        RangedDamageMultiplier,
        ElementalDamageMultiplier,
        ProjectileSpeedMultiplier,
        
        //Elemental
            //Damage Multipliers
        PhysicalDamageMultiplier,
        FireDamageMultiplier,
        IceDamageMultiplier,
        ElectricDamageMultiplier,
        PoisonDamageMultiplier,
        LightDamageMultiplier,
            //Resistances
        PhysicalDamageResistance,
        FireResistance,
        IceResistance,
        ElectricResistance,
        PoisonResistance,
        LightResistance,
        
        //Misc
        CurrencyMultiplier
    }
    
    public Stat GetStatByEnum(VariableType variableType) => variableType switch {
        //Heart
        VariableType.MaxHealth => maxHealth,
        VariableType.DamageResistance => damageResistance,
        VariableType.HealingEffectiveness => healingEffectiveness,
        VariableType.PassiveRegenRate => passiveRegenRate,
        
        //Brain
        VariableType.CriticalChanceMultiplier => criticalChanceMultiplier,
        VariableType.CriticalDamageMultiplier => criticalDamageMultiplier,
        VariableType.Luck => luck,
        
        //Liver
        VariableType.StatusChanceMultiplier => statusChanceMultiplier,
        VariableType.BuffDurationMultiplier => buffDurationMultiplier,
        VariableType.BuffPotencyMultiplier => buffPotencyMultiplier,
        
        //Movement
        VariableType.MoveSpeed => moveSpeed,
        VariableType.JumpCount => jumpCount,
        
        //Mana
        VariableType.MaxMana => maxMana,
        VariableType.ManaRegenRate => manaRegenRate,
        
        //Combat
        VariableType.DamageMultiplier => damageMultiplier,
        VariableType.MeleeDamageMultiplier => meleeDamageMultiplier,
        VariableType.RangedDamageMultiplier => rangedDamageMultiplier,
        VariableType.ElementalDamageMultiplier => elementalDamageMultiplier,
        VariableType.ProjectileSpeedMultiplier => projectileSpeedMultiplier,
        
        //Elemental
            //Damage Multipliers
        VariableType.FireDamageMultiplier => damageMultipliers.fire,
        VariableType.IceDamageMultiplier => damageMultipliers.ice,
        VariableType.ElectricDamageMultiplier => damageMultipliers.electric,
        VariableType.PoisonDamageMultiplier => damageMultipliers.poison,
        VariableType.LightDamageMultiplier => damageMultipliers.light,
        VariableType.PhysicalDamageMultiplier => damageMultipliers.physical,
            //Resistances
        VariableType.PhysicalDamageResistance => damageResistances.physical,
        VariableType.FireResistance => damageResistances.fire,
        VariableType.IceResistance => damageResistances.ice,
        VariableType.ElectricResistance => damageResistances.electric,
        VariableType.PoisonResistance => damageResistances.poison,
        VariableType.LightResistance => damageResistances.light,
        
        //Misc
        VariableType.CurrencyMultiplier => currencyMultiplier,
        
        //default
        _ => null
    };
    
    private void TryGetReferences() {
        eventManager ??= GetComponent<EntityEventManager>();
        statusEffectManager ??= GetComponent<EntityStatusEffectManager>();
        health ??= GetComponent<Health>();
        mana ??= GetComponent<Mana>();
        relicManager ??= GetComponent<RelicManager>();
        organManager ??= GetComponent<OrganManager>();
        healthBar ??= GetComponent<HealthBar>();
        //if (rigidbody == null) { rigidbody = GetComponent<Rigidbody>(); }
    }
    
    private void TryAssignReferences() {
        IStatboard[] statboardMembers = { eventManager, statusEffectManager, health, mana, relicManager, organManager, healthBar };
        
        foreach (IStatboard member in statboardMembers) {
            member?.SetStatboard(this);
        }
        
        foreach (IStatboard member in statboardMembers) {
            member?.StatboardFinishedSet();
        }
    }
}

public interface IStatboard {
    public Statboard statboard { get; set; }

    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }

    /// <summary>
    /// Implement to execute logic once all <c>IStatboards</c> have finished their assignment.
    /// </summary>
    public virtual void StatboardFinishedSet() {
        
    }
}

[Serializable] public struct DamageTypeStats {
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