using Stats;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class Statboard : MonoBehaviour {
    [Section("References")]
    [Button(nameof(TryGetReferences), "Retrieve References", r:0.1f, g:0.1f, b:1f)]
    public EntityStatusEffectManager statusEffectManager;
    public EntityEventManager eventManager;
    public Health health;
    public new Rigidbody rigidbody;

    [Section("Heart")]
    public Stat maxHealth = 100;
    public Stat damageResistance = 0;
    public Stat defense = 0;
    public Stat healingEffectiveness = 1;
    public Stat passiveRegenRate = 0;

    [Section("Lungs")] 
    public Stat moveSpeed = 8;
    public Stat attackSpeedMultiplier = 1;
    public Stat activeAbilityCooldownRate = 1;
    
    [Section("Brain")]
    public Stat criticalChanceMultiplier = 1;
    public Stat criticalDamageMultiplier = 1;
    public Stat luck = 1;

    [Section("Liver")] 
    public Stat statusChanceMultiplier = 1;
    public Stat flaskCooldown = 30;
    public Stat buffDurationMultiplier = 1;
    public Stat buffPotencyMultiplier = 1;
    
    [Section("Combat")]
    public Stat damageMultiplier = 1;
    public Stat meleeDamageMultiplier = 1;
    public Stat activeAbilityDamageMultiplier = 1;
    public Stat elementalDamageMultiplier = 1;
    public Stat projectileSpeedMultiplier = 1;
    
    [Section("Elemental")]
    public DamageTypeStats damageMultipliers = new(1);
    public DamageTypeStats damageResistances = new(0);

    [Section("Misc")] 
    public Stat jumpCount = 1;
    public Stat currencyMultiplier = 1;

    private void Update() {
        foreach (var field in typeof(Statboard).GetFields()) {
            if (field.FieldType == typeof(Stat)) {
                Stat stat = (Stat)field.GetValue(this);
                stat.UpdateTimers();
            }else if (field.FieldType == typeof(DamageTypeStats)) {
                DamageTypeStats stats = (DamageTypeStats)field.GetValue(this);
                stats.TickStats();
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

    public enum VariableType {
        //Heart
        MaxHealth,
        DamageResistance,
        Defense,
        HealingEffectiveness,
        PassiveRegenRate,
        
        //Lungs
        MoveSpeed,
        AttackSpeedMultiplier,
        ActiveAbilityCooldownRate,
        
        //Brain
        CriticalChanceMultiplier,
        CriticalDamageMultiplier,
        Luck,
        
        //Liver
        StatusChanceMultiplier,
        FlaskCooldown,
        BuffDurationMultiplier,
        BuffPotencyMultiplier,
        
        //Combat
        DamageMultiplier,
        MeleeDamageMultiplier,
        ActiveAbilityDamageMultiplier,
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
        JumpCount,
        CurrencyMultiplier
    }
    
    public Stat GetStatByEnum(VariableType variableType) => variableType switch {
        //Heart
        VariableType.MaxHealth => maxHealth,
        VariableType.DamageResistance => damageResistance,
        VariableType.Defense => defense,
        VariableType.HealingEffectiveness => healingEffectiveness,
        VariableType.PassiveRegenRate => passiveRegenRate,
        
        //Lungs
        VariableType.MoveSpeed => moveSpeed,
        VariableType.AttackSpeedMultiplier => attackSpeedMultiplier,
        VariableType.ActiveAbilityCooldownRate => activeAbilityCooldownRate,
        
        //Brain
        VariableType.CriticalChanceMultiplier => criticalChanceMultiplier,
        VariableType.CriticalDamageMultiplier => criticalDamageMultiplier,
        VariableType.Luck => luck,
        
        //Liver
        VariableType.StatusChanceMultiplier => statusChanceMultiplier,
        VariableType.FlaskCooldown => flaskCooldown,
        VariableType.BuffDurationMultiplier => buffDurationMultiplier,
        VariableType.BuffPotencyMultiplier => buffPotencyMultiplier,
        
        //Combat
        VariableType.DamageMultiplier => damageMultiplier,
        VariableType.MeleeDamageMultiplier => meleeDamageMultiplier,
        VariableType.ActiveAbilityDamageMultiplier => activeAbilityDamageMultiplier,
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
        VariableType.JumpCount => jumpCount,
        VariableType.CurrencyMultiplier => currencyMultiplier,
        
        //default
        _ => null
    };

    private void Awake() {
        TryGetReferences();
        TryAssignReferences();
    }
    
    private void TryGetReferences() {
        statusEffectManager ??= GetComponent<EntityStatusEffectManager>();
        eventManager ??= GetComponent<EntityEventManager>();
        health ??= GetComponent<Health>();
        if (rigidbody == null) {
            rigidbody = GetComponent<Rigidbody>();
        }
    }
    
    private void TryAssignReferences() {
        statusEffectManager?.SetStatboard(this);
        eventManager?.SetStatboard(this);
        health?.SetStatboard(this);
    }
}