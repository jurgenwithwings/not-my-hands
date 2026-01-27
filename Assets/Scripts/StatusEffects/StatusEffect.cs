using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Stats;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

[Serializable]
public abstract class StatusEffect {
    public string key { get; private set; } = "";
    protected EntityStatusEffectManager attachedManager;
    protected Statboard stats => attachedManager.statboard;
    public StatusEffectData Data { get; protected set; }
    
    protected float currentDuration;
    protected int currentStacks = 1;
    
    protected DamageInfo highestDamageReceived;
    
    public virtual void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        Data = data;
        attachedManager = manager;
        highestDamageReceived = damageInfo;
        currentDuration = Data.maxDuration;
        PlayerHUDEvents.DebugText($"Entered Damage: {damageInfo.finalDamage}");
    }
    
    public virtual void AddStack(DamageInfo damageInfo, int stacks) {
        currentStacks += stacks;
        if (currentStacks > Data.maxStacks) {
            currentStacks = Data.maxStacks;
        }
        
        if (damageInfo.finalDamage > highestDamageReceived.finalDamage) {
            highestDamageReceived = damageInfo;
        }

        if (Data.refillDurationWhenGainingStack) {
            currentDuration = Data.maxDuration;
        }
    }
    
    public virtual void Update() {
        if (currentDuration <= 0) {
            RemoveStacks(Data.StacksLostOnDurationEnd);
            currentDuration = Data.maxDuration;
        }
        currentDuration -= Time.deltaTime;
    }
    
    public virtual void RemoveStacks(int stacks) {
        currentStacks -= stacks;
        if (currentStacks <= 0) {
            currentStacks = 0;
            attachedManager.RemoveEffect(GetType());
        }
    }
    
    public virtual void RemoveEffect() {
        
    }
}

public abstract class BuffEffect : StatusEffect {
    protected float durationMult => highestDamageReceived.source.buffDurationMultiplier;
    protected float potencyMult => highestDamageReceived.source.buffPotencyMultiplier;

    public override void Update() {
        if (currentDuration <= 0) {
            RemoveStacks(Data.StacksLostOnDurationEnd);
            currentDuration = Data.maxDuration;
        }
        currentDuration -= Time.deltaTime / durationMult;
    }
}

[Serializable] public struct DoT {
    public DamageInstance[] baseDamage;
    public DamageInstance[] damagePerStack;

    public float tickInterval;
    
    [HideInInspector] public float timer;
    
    public bool CanTick() {
        return timer >= tickInterval;
    }

    public void ResetTimer() {
        timer -= tickInterval;
    }

    public void Update() {
        timer += Time.deltaTime;
    }

    public DamageInstance[] GetTickDamage(int currentStacks, float scale = 1) {
        DamageInstance[] result = new DamageInstance[GameConfig.DamageTypesCount];
        foreach (DamageInstance inst in baseDamage) {
            result[(int)inst.damageType] = inst;
            result[(int)inst.damageType].baseAmount *= scale;
        }

        foreach (DamageInstance inst in damagePerStack) {
            result[(int)inst.damageType].baseAmount += inst.baseAmount * (currentStacks - 1) * scale;
        }
        return result;
    }
}

[Serializable]
public class Bleed : StatusEffect {
    [SerializeField] private DoT dot = new DoT();
    [SerializeField] private float healthPercentDamage = 5f;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Bleed config = data.statusEffectClass as Bleed;
        dot = config.dot;
        healthPercentDamage = config.healthPercentDamage;
        
        PlayerHUDEvents.DebugText($"Entered Bleed: {damageInfo.finalDamage}");
        PlayerHUDEvents.DebugText($"Dot Damage: {dot.baseDamage[0].baseAmount}");
        PlayerHUDEvents.DebugText($"Dot Calculated Damage: {dot.GetTickDamage(currentStacks, highestDamageReceived.finalDamage)[0].baseAmount}");
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        if (currentStacks >= Data.maxStacks) {
            DamageInstance inst = new (DamageInstance.DamageType.Physical, stats.maxHealth * healthPercentDamage);
            DamageInfo info = new (new[]{inst}, highestDamageReceived.source) {
                hitPoint = stats.transform.position,
                ignoreResistances = true
            };
            stats.health.TakeDamage(info);
            attachedManager.RemoveEffect(GetType());
        }
    }

    public override void Update() {
        base.Update();
        dot.Update();
        if (dot.CanTick()) {
            DamageInfo info = new(dot.GetTickDamage(currentStacks, highestDamageReceived.finalDamage), highestDamageReceived.source) {
                hitPoint = stats.transform.position,
                ignoreResistances = true,
            };
            PlayerHUDEvents.DebugText($"Damage Info Damage: {info.finalDamage} | Current Stacks: {currentStacks}");
            stats?.health?.TakeDamage(info);
            dot.ResetTimer();
        }
    }
}

[Serializable]
public class Burn : StatusEffect {
    private float totalDuration;
    private float bonusMultiplier;
    [SerializeField] private DoT dot;
    [SerializeField] private float percentDamageIncreasePerSecond = 0.02f;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Burn config = data.statusEffectClass as Burn;
        dot = config.dot;
        percentDamageIncreasePerSecond = config.percentDamageIncreasePerSecond;
    }

    public override void Update() {
        dot.Update();
        if (dot.CanTick()) {
            bonusMultiplier = 1 + (percentDamageIncreasePerSecond * totalDuration);
            
            DamageInfo info = new(dot.GetTickDamage(currentStacks, bonusMultiplier * highestDamageReceived.finalDamage), highestDamageReceived.source) {
                hitPoint = stats.transform.position,
                ignoreResistances = true,
            };

            stats.health.TakeDamage(info);
            
            totalDuration += dot.tickInterval;
            
            dot.ResetTimer();
        }
        
        base.Update();
    }
}

[Serializable]
public class Freeze : StatusEffect {
    string source = "Freeze";
    private Modifier mod;
    [SerializeField] private float baseSpeedMultiplier = 0.1f;
    [SerializeField] private float perStackSpeedMultiplier = 0.05f;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Freeze config = data.statusEffectClass as Freeze;
        baseSpeedMultiplier = config.baseSpeedMultiplier;
        perStackSpeedMultiplier = config.perStackSpeedMultiplier;
        
        mod = new Modifier(baseSpeedMultiplier, ModifierType.Final, source);
        ReplaceModifier();
        
        stats.moveSpeed.AddModifier(mod);
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        
        ReplaceModifier();
    }

    private void ReplaceModifier() {
        stats.moveSpeed.RemoveModifier(mod);
        mod = new Modifier(baseSpeedMultiplier + (perStackSpeedMultiplier * currentStacks), mod.Type, mod.Source);
        stats.moveSpeed.AddModifier(mod);
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);
        ReplaceModifier();
    }

    public override void RemoveEffect() {
        stats?.moveSpeed.RemoveModifier(mod);
    }
}

[Serializable]
public class Poison : StatusEffect {
    [SerializeField] DoT dot;
    [SerializeField] private float damageReductionPerStack = 0.02f;
    private Modifier mod;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Poison config = data.statusEffectClass as Poison;
        dot = config.dot;
        damageReductionPerStack = config.damageReductionPerStack;

        mod = new Modifier(damageReductionPerStack * currentStacks, ModifierType.Final, "Poison");
        ReplaceModifier();
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        ReplaceModifier();
    }

    private void ReplaceModifier() {
        stats.moveSpeed.RemoveModifier(mod);
        mod = new Modifier(damageReductionPerStack * currentStacks, mod.Type, mod.Source);
        stats.moveSpeed.AddModifier(mod);
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);
        ReplaceModifier();
    }

    public override void RemoveEffect() {
        base.RemoveEffect();
        stats?.moveSpeed.RemoveModifier(mod);
    }

    public override void Update() {
        base.Update();
        dot.Update();
        if (dot.CanTick()) {
            DamageInfo info = new(dot.GetTickDamage(currentStacks, highestDamageReceived.finalDamage), highestDamageReceived.source) {
                hitPoint = stats.transform.position,
                ignoreResistances = true,
            };
            stats?.health?.TakeDamage(info);
            dot.ResetTimer();
        }
    }
}

[Serializable]
public class Charged : StatusEffect {
    [SerializeField] DoT dot;
    [SerializeField] private float chanceToArc = 0.35f;
    [SerializeField] private float arcRadius = 5.5f;
    private float arcTimer;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Charged config = data.statusEffectClass as Charged;
        dot = config.dot;
        chanceToArc = config.chanceToArc;
        arcRadius = config.arcRadius;
    }

    public override void Update() {
        base.Update();
        arcTimer += Time.deltaTime;
        if (arcTimer >= 1) {
            Collider[] collisions = Physics.OverlapSphere(stats.transform.position, arcRadius, LayerMask.NameToLayer("Pawn"));
            if (collisions.Length > 0) {
                foreach (Collider collider in collisions) {
                    if (collider.TryGetComponent(out Statboard statboard) && statboard != stats) {
                        DamageInfo info = new(dot.GetTickDamage(currentStacks, highestDamageReceived.finalDamage), highestDamageReceived.source) {
                            hitPoint = statboard.transform.position,
                            statusEffects = new() { { Data, 1 } },
                        };
                        statboard.health.TakeDamage(info);
                    }
                }
            }
            arcTimer--;
        }
        dot.Update();
        if (dot.CanTick()) {
            DamageInfo info = new(dot.GetTickDamage(currentStacks, highestDamageReceived.finalDamage), highestDamageReceived.source) {
                hitPoint = stats.transform.position,
                ignoreResistances = true,
            };
            stats?.health?.TakeDamage(info);
            dot.ResetTimer();
        }
    }
}

[Serializable]
public class Judged : StatusEffect {
    private float damageAbsorbed;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite eyeSprite;
    [SerializeField] private Color eyeColor = new Color(0.96f, 0.67f, 0.04f, 0.5f) * 4f;
    [SerializeField] private float offset = 0.35f;
    [SerializeField] private DamageInstance basePercentDamage;
    [SerializeField] private DamageInstance perStackPercentDamage;
    
    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);

        Judged config = (Judged)Data.statusEffectClass;
        eyeSprite = config.eyeSprite;
        eyeColor = config.eyeColor;
        offset = config.offset;
        basePercentDamage = config.basePercentDamage;
        perStackPercentDamage = config.perStackPercentDamage;
        
        OnTakeDamage(damageInfo);
        stats.eventManager.OnDamageTaken += OnTakeDamage;

        spriteRenderer = GameObject.Instantiate(GameConfig.Instance.emptyGameObject, stats.healthBar.transform).AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = eyeSprite;
        spriteRenderer.color = eyeColor;
        spriteRenderer.transform.position = stats.healthBar.transform.position + (Vector3.up * offset);
        Debug.Log(eyeSprite.name);
    }
    
    private void OnTakeDamage(DamageInfo damageInfo) {
        damageAbsorbed += damageInfo.finalDamage;
    }
    
    public override void Update() {
        if (currentDuration <= 0 || damageAbsorbed * (basePercentDamage.baseAmount + (perStackPercentDamage.baseAmount * (currentStacks - 1))) >= stats?.health?.CurrentHealth) {
            Detonate();
        }
        if (spriteRenderer == null || stats.healthBar == null) return;
        spriteRenderer.transform.position = stats.healthBar.transform.position + (Vector3.up * offset);
        spriteRenderer.transform.LookAt(highestDamageReceived.source.transform.position + Vector3.up * 0.7f);
        base.Update();
    }

    private void Detonate() {
        float damage = damageAbsorbed * (basePercentDamage.baseAmount + (perStackPercentDamage.baseAmount * (currentStacks - 1)));
        DamageInstance[] damageInstance = { new(basePercentDamage.damageType, damage) };
        DamageInfo damageInfo = new(damageInstance, highestDamageReceived.source) {
            hitPoint = spriteRenderer.transform.position,
            ignoreResistances = true,
        };
        stats?.health?.TakeDamage(damageInfo);
    }
    
    public override void RemoveEffect() {
        stats.eventManager.OnDamageTaken -= OnTakeDamage;
        
        if (stats.healthBar != null || spriteRenderer != null) {
            Object.Destroy(spriteRenderer.gameObject);
        }
    }
}



public class SpeedBoost : BuffEffect {
    string source = "SpeedBoost";
    private Modifier mod;
    [SerializeField] float baseSpeedBoost = 0.1f;
    [SerializeField] float perStackSpeedBoost = 0.1f;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        mod = new Modifier(baseSpeedBoost, ModifierType.Multiply, source);
        
        stats.moveSpeed.AddModifier(mod);
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        
        ReplaceModifier();
    }

    private void ReplaceModifier() {
        mod = new Modifier(baseSpeedBoost + (perStackSpeedBoost * currentStacks), mod.Type, mod.Source);
        stats.moveSpeed.RemoveAllModifiersFromSource(source);
        stats.moveSpeed.AddModifier(mod);
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);
        ReplaceModifier();
    }

    public override void RemoveEffect() {
        stats.moveSpeed.RemoveAllModifiersFromSource(source);
    }
}