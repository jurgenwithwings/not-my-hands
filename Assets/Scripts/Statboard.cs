using System;
using Stats;
using UnityEngine;

public class Statboard : MonoBehaviour
{
    //[Section("References")]
    //public EntityStatusEffectManager statusEffectManager;
    //public EntityEventManager eventManager;

    [Section("Heart")]
    public Stat MaxHealth = 100;
    public Stat DamageResistance = 0;
    public Stat Defense = 0;
    public Stat HealingEffectiveness = 1;
    public Stat PassiveRegenRate = 0;

    [Section("Lungs")] 
    public Stat MoveSpeed = 8;
    public Stat AttackSpeedMultiplier = 1;
    public Stat ActiveAbilityCooldownRate = 1;
    
    [Section("Brain")]
    public Stat CriticalChanceMultiplier = 1;
    public Stat CriticalDamageMultiplier = 1;
    public Stat Luck = 1;

    [Section("Liver")] 
    public Stat StatusChanceMultiplier = 1;
    public Stat FlaskCooldown = 30;
    public Stat BuffDurationMultiplier = 1;
    public Stat BuffPotencyMultiplier = 1;
    
    [Section("Combat")]
    public Stat DamageMultiplier = 1;
    public Stat MeleeDamageMultiplier = 1;
    public Stat ActiveAbilityDamageMultiplier = 1;
    public Stat ElementalDamageMultiplier = 1;
    public Stat ProjectileSpeedMultiplier = 1;
    
    [Section("Elemental")]
    [Header("Damage Multipliers")]
    public Stat FireDamageMultiplier = 1;
    public Stat IceDamageMultiplier = 1;
    public Stat ElectricDamageMultiplier = 1;
    public Stat PoisonDamageMultiplier = 1;
    public Stat LightDamageMultiplier = 1;
    [Header("Resistances")]
    public Stat FireResistance = 1;
    public Stat IceResistance = 1;
    public Stat ElectricResistance = 1;
    public Stat PoisonResistance = 1;
    public Stat LightResistance = 1;

    [Section("Misc")] 
    public Stat JumpCount = 1;
    public Stat CurrencyMultiplier = 1;

    private void Update() {
        foreach (var field in typeof(Statboard).GetFields()) {
            if (field.FieldType == typeof(Stat)) {
                Stat stat = (Stat)field.GetValue(this);
                stat.UpdateTimers();
            }
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

    public Stat GetStatByEnum(VariableType variableType) => variableType switch {
        VariableType.Health => MaxHealth,
        VariableType.MoveSpeed => MoveSpeed,
        VariableType.FlaskCooldown => FlaskCooldown,
        _ => null
    };
    
    public enum VariableType {
        Health,
        MoveSpeed,
        FlaskCooldown
    }
}