using System;
using System.Collections;
using System.Collections.Generic;
using Stats;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "StatusEffectData", menuName = "ScriptableObjects/StatusEffectData", order = 1)]
public class StatusEffectData : ScriptableObject {
    public Sprite Icon;
    public List<VisualEffect> Vfx;
    public List<object> AdditionalResources;
}

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
    
    public virtual void Initialize(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        attachedManager = manager;
        highestDamageReceived = damageInfo;
        currentDuration = maxDuration;
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
        if (Time.time - lastTick >= tickInterval) {
            lastTick = Time.time;

            DamageInfo damage = new DamageInfo(finalDamage.ToArray(), highestDamageReceived.source);

            stats.health.TakeDamage(damage);
        }
        if (currentDuration <= 0) {
            RemoveStacks(StacksLostOnDurationEnd);
            currentDuration = maxDuration;
        }
        currentDuration -= Time.deltaTime;
    }
    
    public virtual void RemoveStacks(int stacks) {
        currentStacks -= stacks;
        isDirty = true;
        if (currentStacks < 0) {
            currentStacks = 0;
            attachedManager.RemoveEffect(GetType());
        }
    }
    
    public virtual void RemoveEffect() {
        
    }
}

public class Burn : StatusEffect {
    private float totalDuration;
    
    public override void Initialize(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialize(manager, damageInfo);
        
        tickInterval = 1.0f;
        
        baseDamage = new List<DamageInstance>();
        baseDamage.Add(new DamageInstance(DamageInstance.Type.Fire, 0.25f));
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
            DamageInstance damageIncrease = baseDamage[0];
            damageIncrease.amount += 0.02f * (totalDuration * 0.15f);
            baseDamage[0] = damageIncrease;
            
            totalDuration += tickInterval;

            isDirty = true;
        }
        base.Tick();
    }
}

public class Freeze : StatusEffect {
    public override void Initialize(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialize(manager, damageInfo);

        maxStacks = 10;
        maxDuration = 6;
        currentDuration = maxDuration;

        stats.moveSpeed.AddModifier(new Modifier(-0.5f, ModifierType.PercentAdd, "Freeze"));
    }

    public override void AddStack(DamageInfo damageInfo, int stacks) {
        base.AddStack(damageInfo, stacks);

        currentDuration = maxDuration;
    }

    public override void RemoveEffect() {
        stats.moveSpeed.RemoveAllModifiersFromSource("Freeze");
    }
}

public class Poison : StatusEffect {
    
}

public class Charged : StatusEffect {
    
}

public class Judged : StatusEffect {
    private float damageAbsorbed;
    
    public override void Initialize(EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialize(manager, damageInfo);

        baseDamage = new List<DamageInstance>();
        baseDamage.Add(new DamageInstance(DamageInstance.Type.Light, 0.35f));
        damagePerStack = new List<DamageInstance>();
        damagePerStack.Add(new DamageInstance(DamageInstance.Type.Light, 0.1f));

        maxDuration = 6;
        currentDuration = maxDuration;
        
        OnTakeDamage(damageInfo);
        
        stats.eventManager.OnTakeDamage += OnTakeDamage;
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

    public override void Tick() {
        if (currentDuration <= 0) {
            float damage = damageAbsorbed * (baseDamage[0].amount + (damagePerStack[0].amount * currentStacks));
            DamageInstance[] damageInstance = { new(DamageInstance.Type.Light, damage) };
            DamageInfo damageInfo = new DamageInfo(damageInstance, highestDamageReceived.source);
            stats.health.TakeDamage(damageInfo);
        }
    }
    
    public override void RemoveEffect() {
        stats.eventManager.OnTakeDamage -= OnTakeDamage;
    }
}