using System;
using System.Collections;
using System.Collections.Generic;
using Stats;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "StatusEffectData", menuName = "ScriptableObjects/StatusEffectData", order = 1)]
public class StatusEffectData : ScriptableObject {
    public Sprite Icon;
    public List<VisualEffect> Vfx;
    public List<object> AdditionalResources;
}

[Serializable]
public abstract class StatusEffect {
    public string key { get; private set; } = "";
    protected EntityStatusEffectManager attachedManager;
    protected Statboard stats => attachedManager.statboard;
    public StatusEffectData Data { get; protected set; }
    protected float maxDuration;
    protected float currentDuration;
    protected int StacksLostOnDurationEnd = 999;
    protected int maxStacks = 10;
    protected int currentStacks = 1;
    
    
    protected DamageInfo highestDamageReceived;
    protected List<DamageInstance> baseDamage;
    protected List<DamageInstance> damagePerStack;
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

    
    protected float tickInterval;
    protected float lastTick;

    public void GiveData(StatusEffectData data) {
        Data = data;
    }
    
    protected virtual List<DamageInstance> CalculateFinalDamage() {
        Hashtable totalDamage = new();
        
        //Calculate base damage
        foreach (var damage in baseDamage) {
            float damageAmount = damage.amount;
            totalDamage[damage.type] = damageAmount;
        }
        
        //Calculate damage per stack
        foreach (var damage in damagePerStack) {
            float damageAmount = damage.amount * currentStacks;
            if (totalDamage[damage.type] != null) {
                totalDamage[damage.type] = ((float)totalDamage[damage.type] + damageAmount) * highestDamageReceived.totalDamage;
            }
            else {
                totalDamage[damage.type] = damageAmount * highestDamageReceived.totalDamage;
            }
        }
        
        //Convert hashtable to list
        List<DamageInstance> result = new();
        foreach (DictionaryEntry entry in totalDamage) {
            result.Add(new DamageInstance((DamageInstance.Type)entry.Key, (float)entry.Value));
        }
        
        return result;
    }
    
    public virtual void Initialise(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        attachedManager = manager;
        highestDamageReceived = damageInfo;
        currentDuration = maxDuration;
        lastTick = Time.time;
    }
    
    public virtual void AddStack(DamageInfo damageInfo, int stacks) {
        currentStacks += stacks;
        isDirty = true;
        if (currentStacks > maxStacks) {
            currentStacks = maxStacks;
        }
        
        if (damageInfo.totalDamage > highestDamageReceived.totalDamage) {
            highestDamageReceived = damageInfo;
        }
    }
    
    public virtual void Tick() {
        if (currentDuration <= 0) {
            RemoveStacks(StacksLostOnDurationEnd);
            currentDuration = maxDuration;
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

public class Burn : StatusEffect {
    private float totalDuration;
    
    public override void Initialise(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(manager, damageInfo);
        
        tickInterval = 1.0f;
        lastTick = Time.time;
        
        baseDamage = new List<DamageInstance>();
        baseDamage.Add(new DamageInstance(DamageInstance.Type.Fire, 0.15f));
        damagePerStack = new List<DamageInstance>();
        damagePerStack.Add(new DamageInstance(DamageInstance.Type.Fire, 0.1f));

        maxDuration = 5;
        currentDuration = maxDuration;
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        
        currentDuration = maxDuration;
    }

    public override void Tick() {
        if (Time.time - lastTick > tickInterval) {
            lastTick = Time.time;
            
            DamageInstance damageIncrease = baseDamage[0];
            damageIncrease.amount += 0.02f * (totalDuration * 0.15f);
            baseDamage[0] = damageIncrease;

            DamageInfo damage = new DamageInfo(finalDamage.ToArray(), highestDamageReceived.source);
            damage.hitPoint = stats.transform.position;
            damage.ignoreResistances = true;

            stats.health.TakeDamage(damage);
            
            totalDuration += tickInterval;

            isDirty = true;
        }
        
        base.Tick();
    }
}

public class Freeze : StatusEffect {
    string source = "Freeze";
    private Modifier mod;
    
    protected override List<DamageInstance> CalculateFinalDamage() {
        return null;
    }

    public override void Initialise(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(manager, damageInfo);

        maxStacks = 10;
        maxDuration = 6;
        currentDuration = maxDuration;
        
        mod = new Modifier(-0.1f, ModifierType.SequentialMultiply, source);
        
        stats.moveSpeed.AddModifier(mod);
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        
        ReplaceModifier();
        
        currentDuration = maxDuration;
    }

    private void ReplaceModifier() {
        mod = new Modifier(-0.05f + (-0.05f * currentStacks), ModifierType.SequentialMultiply, source);
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

public class Poison : StatusEffect {
    
}

public class Charged : StatusEffect {
    
}

[Serializable]
public class Judged : StatusEffect {
    private float damageAbsorbed;
    private AsyncOperationHandle handle;
    private SpriteRenderer spriteRenderer;
    private float offset = 0.35f;
    
    public override void Initialise(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(manager, damageInfo);

        baseDamage = new List<DamageInstance>();
        baseDamage.Add(new DamageInstance(DamageInstance.Type.Light, 0.15f));
        damagePerStack = new List<DamageInstance>();
        damagePerStack.Add(new DamageInstance(DamageInstance.Type.Light, 0.1f));

        maxDuration = 6;
        currentDuration = maxDuration;
        
        OnTakeDamage(damageInfo);
        stats.eventManager.OnDamageTaken += OnTakeDamage;

        if (stats.healthBar != null) {
            handle = Addressables.LoadAssetAsync<Texture2D>("JudgedEye");
            handle.Completed += SpriteLoaded;
        }
    }

    private void SpriteLoaded(AsyncOperationHandle handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
            Texture2D tex = (Texture2D)handle.Result;
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 1000);
            spriteRenderer.color = new Color(0.96f, 0.67f, 0.04f, 0.5f) * 4f;
            spriteRenderer.transform.position = stats.healthBar.transform.position + (Vector3.up * offset);
        }
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

    private void OnTakeDamage(DamageInfo damageInfo) {
        damageAbsorbed += damageInfo.totalDamage;
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        currentStacks += stacks;
        isDirty = true;
        if (currentStacks > maxStacks) {
            currentStacks = maxStacks;
        }
    }
    
    public override void RemoveEffect() {
        stats.eventManager.OnDamageTaken -= OnTakeDamage;
        if (stats.healthBar != null || spriteRenderer != null) {
            handle.Completed -= SpriteLoaded;
            Object.Destroy(spriteRenderer.gameObject);
        }

        float damage = damageAbsorbed * (baseDamage[0].amount + (damagePerStack[0].amount * currentStacks));
        DamageInstance[] damageInstance = { new(DamageInstance.Type.Light, damage) };
        DamageInfo damageInfo = new DamageInfo(damageInstance, highestDamageReceived.source);
        damageInfo.ignoreResistances = true;
        damageInfo.selfDamage = true;
        damageInfo.hitPoint = stats.transform.position;
        stats.health.TakeDamage(damageInfo);
    }
}

public abstract class BuffEffect : StatusEffect {
    protected float durationMult => highestDamageReceived.source.buffDurationMultiplier;
    protected float potencyMult => highestDamageReceived.source.buffPotencyMultiplier;

    public override void Tick() {
        if (currentDuration <= 0) {
            RemoveStacks(StacksLostOnDurationEnd);
            currentDuration = maxDuration;
        }
        currentDuration -= Time.deltaTime / durationMult;
        Debug.Log(currentDuration * durationMult + "/" + maxDuration * durationMult);
    }
}

public class SpeedBoost : BuffEffect {
    string source = "SpeedBoost";
    private Modifier mod;
    
    protected override List<DamageInstance> CalculateFinalDamage() {
        return null;
    }

    public override void Initialise(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(manager, damageInfo);

        maxStacks = 10;
        maxDuration = 8;
        currentDuration = maxDuration;
        
        mod = new Modifier(0.1f, ModifierType.TotalMultiply, source);
        
        stats.moveSpeed.AddModifier(mod);
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);
        
        ReplaceModifier();
        
        currentDuration = maxDuration;
    }

    private void ReplaceModifier() {
        mod = new Modifier(0.05f + (0.05f * currentStacks), ModifierType.TotalMultiply, source);
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