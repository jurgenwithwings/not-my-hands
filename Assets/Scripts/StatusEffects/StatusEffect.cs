using System;
using System.Collections;
using System.Collections.Generic;
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
    protected float lastTick;
    
    protected DamageInfo highestDamageReceived;
    
    protected List<DamageInstance> finalDamage {
        get {
            if (isDirty) {
                finalDamageCache = CalculateFinalDamage();
                isDirty = false;
            }

            return finalDamageCache;
        }
    }
    protected bool isDirty = true;
    private List<DamageInstance> finalDamageCache = new();
    
    protected virtual List<DamageInstance> CalculateFinalDamage() {
        Hashtable totalDamage = new();
        
        //Get the base damage
        foreach (var damage in Data.baseDamage) {
            float damageAmount = damage.baseAmount;
            totalDamage[damage.damageType] = damageAmount;
        }
        
        //Calculate damage per stack
        foreach (var damage in Data.damagePerStack) {
            float damageAmount = damage.baseAmount * currentStacks;
            if (totalDamage[damage.damageType] != null) {
                totalDamage[damage.damageType] = 0f;
            }
            totalDamage[damage.damageType] = ((float)totalDamage[damage.damageType] + damageAmount) * highestDamageReceived.finalDamage;
        }
        
        //Convert hashtable to list
        List<DamageInstance> result = new();
        foreach (DictionaryEntry entry in totalDamage) {
            result.Add(new DamageInstance((DamageInstance.DamageType)entry.Key, (float)entry.Value));
        }
        
        return result;
    }
    
    public virtual void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        Data = data;
        attachedManager = manager;
        highestDamageReceived = damageInfo;
        
        //TEMP TEMP TEMP
        highestDamageReceived.debug = false;
        
        currentDuration = Data.maxDuration;
        lastTick = Time.time;
    }
    
    public virtual void AddStack(DamageInfo damageInfo, int stacks) {
        currentStacks += stacks;
        isDirty = true;
        if (currentStacks > Data.maxStacks) {
            currentStacks = Data.maxStacks;
        }
        
        if (damageInfo.finalDamage > highestDamageReceived.finalDamage) {
            highestDamageReceived = damageInfo;
        }
    }
    
    public virtual void Tick() {
        if (currentDuration <= 0) {
            RemoveStacks(Data.StacksLostOnDurationEnd);
            currentDuration = Data.maxDuration;
        }
        currentDuration -= Time.deltaTime;
    }
    
    public virtual void RemoveStacks(int stacks) {
        currentStacks -= stacks;
        isDirty = true;
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

    public override void Tick() {
        if (currentDuration <= 0) {
            RemoveStacks(Data.StacksLostOnDurationEnd);
            currentDuration = Data.maxDuration;
        }
        currentDuration -= Time.deltaTime / durationMult;
    }
}

[Serializable]
public class Bleed : StatusEffect {
    
}

[Serializable]
public class Burn : StatusEffect {
    private float totalDuration;
    private float bonusDamage;
    [SerializeField] private float damageIncreasePerTick = 0.02f;
    [SerializeField] private float totalDurationFactor = 0.15f;

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        
        bonusDamage = Data.baseDamage[0].baseAmount;
        currentDuration = Data.maxDuration;
    }

    public override void Tick() {
        if (Time.time - lastTick > Data.tickInterval) {
            lastTick = Time.time;
            
            bonusDamage = 1 + (damageIncreasePerTick * (totalDuration));
            
            float damagePercent = Data.baseDamage[0].baseAmount + (Data.damagePerStack[0].baseAmount * currentStacks);

            DamageInstance[] damageResult = { new(Data.baseDamage[0].damageType, (damagePercent * bonusDamage) * highestDamageReceived.finalDamage)  };
            
            DamageInfo damage = new(damageResult, highestDamageReceived.source) {
                hitPoint = stats.transform.position,
                ignoreResistances = true,
            };

            stats.health.TakeDamage(damage);
            
            totalDuration += Data.tickInterval;
        }
        
        base.Tick();
    }
}

[Serializable]
public class Freeze : StatusEffect {
    string source = "Freeze";
    private Modifier mod;
    
    [SerializeField] private float baseSpeedMultiplier = 0.1f;
    [SerializeField] private float perStackSpeedMultiplier = 0.05f;
    
    protected override List<DamageInstance> CalculateFinalDamage() {
        return null;
    }

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        mod = new Modifier(baseSpeedMultiplier, ModifierType.Additive, source);
        ReplaceModifier();
        
        stats.moveSpeed.AddModifier(mod);
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        
        ReplaceModifier();
        
        currentDuration = Data.maxDuration;
    }

    private void ReplaceModifier() {
        mod = new Modifier(baseSpeedMultiplier + (perStackSpeedMultiplier * currentStacks), mod.Type, mod.Source);
        stats.moveSpeed.RemoveAllModifiersFromSource(source);
        stats.moveSpeed.AddModifier(mod);
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);
        ReplaceModifier();
    }

    public override void RemoveEffect() {
        stats?.moveSpeed.RemoveAllModifiersFromSource(source);
    }
}

[Serializable]
public class Poison : StatusEffect {
    
}

[Serializable]
public class Charged : StatusEffect {
    
}

[Serializable]
public class Judged : StatusEffect {
    private float damageAbsorbed;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite eyeSprite;
    [SerializeField] private Color eyeColor = new Color(0.96f, 0.67f, 0.04f, 0.5f) * 4f;
    [SerializeField] private float offset = 0.35f;
    
    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);

        Judged judgedData = (Judged)Data.statusEffectClass;
        eyeSprite = judgedData.eyeSprite;
        eyeColor = judgedData.eyeColor;
        offset = judgedData.offset;
        
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
    
    public override void Tick() {
        if (currentDuration <= 0) {
            attachedManager.RemoveEffect(GetType());
        }
        currentDuration -= Time.deltaTime;
        if (spriteRenderer == null || stats.healthBar == null) return;
        spriteRenderer.transform.position = stats.healthBar.transform.position + (Vector3.up * offset);
        spriteRenderer.transform.LookAt(highestDamageReceived.source.transform.position + Vector3.up * 0.7f);
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        currentStacks += stacks;
        isDirty = true;
        if (currentStacks > Data.maxStacks) {
            currentStacks = Data.maxStacks;
        }
    }
    
    public override void RemoveEffect() {
        stats.eventManager.OnDamageTaken -= OnTakeDamage;
        
        float damage = damageAbsorbed * (Data.baseDamage[0].baseAmount + (Data.damagePerStack[0].baseAmount * currentStacks));
        DamageInstance[] damageInstance = { new(DamageInstance.DamageType.Light, damage) };
        DamageInfo damageInfo = new(damageInstance, highestDamageReceived.source);
        damageInfo.ignoreResistances = true;
        damageInfo.hitPoint = spriteRenderer.transform.position;
        stats.health.TakeDamage(damageInfo);
        
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
    
    protected override List<DamageInstance> CalculateFinalDamage() {
        return null;
    }

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