using System;
using Stats;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public abstract class StatusEffect {
    public string key { get; private set; } = "";
    protected EntityStatusEffectManager attachedManager;
    protected Statboard stats => attachedManager.statboard;
    public StatusEffectData Data { get; protected set; }
    
    protected float currentDuration;
    protected int stacks = 1;
    
    protected DamageInfo highestDamageReceived;
    
    public virtual void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        Data = data;
        attachedManager = manager;
        highestDamageReceived = damageInfo;
        currentDuration = Data.maxDuration;
    }
    
    public virtual void AddStack(DamageInfo damageInfo) {
        stacks++;
        if (stacks > Data.maxStacks) {
            stacks = Data.maxStacks;
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
        this.stacks -= stacks;
        if (this.stacks <= 0) {
            this.stacks = 0;
            attachedManager.RemoveEffect(Data);
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
    public Damage damage;
    public Damage damagePerStack;

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

    public Damage GetTickDamage(int currentStacks, float scale = 1) {
        DamageInstance[] result = damage.ToArray();
        DamageInstance[] stackDamage = damagePerStack.ToArray();
        
        for (int i = 0; i < result.Length; i++) {
            result[i].baseAmount *= scale;
            result[i].baseAmount += stackDamage[i].baseAmount * (currentStacks - 1) * scale;
        }
        
        return new Damage(result);
    }
}

[Serializable] public class Bleed : StatusEffect {
    [SerializeField] private DoT dot = new DoT();
    [SerializeField] private float healthPercentDamage = 5f;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Bleed config = data.statusEffectClass as Bleed;
        dot = config.dot;
        healthPercentDamage = config.healthPercentDamage;
    }

    public override void AddStack(DamageInfo damageInfo) {
        base.AddStack(damageInfo);
        if (stacks >= Data.maxStacks) {
            Damage damage = new() {
                physical = stats.maxHealth * healthPercentDamage
            };
            DamageInfo info = new (damage, highestDamageReceived.source, stats.transform.position) {
                ignoreResistances = true
            };
            stats.health.TakeDamage(info);
            attachedManager.RemoveEffect(Data);
        }
    }

    public override void Update() {
        base.Update();
        dot.Update();
        if (dot.CanTick()) {
            DamageInfo info = new(dot.GetTickDamage(stacks, highestDamageReceived.finalDamage), highestDamageReceived.source, stats.transform.position) {
                ignoreResistances = true,
            };
            stats?.health?.TakeDamage(info);
            dot.ResetTimer();
        }
    }
}

[Serializable] public class Burn : StatusEffect {
    private float totalDuration;
    private float bonusMultiplier;
    [SerializeField] private DoT dot;
    [SerializeField] private float percentDamageIncreasePerSecond = 0.02f;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        Debug.Log("Initialised Burn");
        
        Burn config = data.statusEffectClass as Burn;
        dot = config.dot;
        percentDamageIncreasePerSecond = config.percentDamageIncreasePerSecond;
    }

    public override void Update() {
        dot.Update();
        if (dot.CanTick()) {
            Debug.Log("Tick Burn");
            bonusMultiplier = 1 + (percentDamageIncreasePerSecond * totalDuration);
            
            DamageInfo info = new(dot.GetTickDamage(stacks, bonusMultiplier * highestDamageReceived.finalDamage), highestDamageReceived.source, stats.transform.position) {
                ignoreResistances = true,
            };
            Debug.Log($"Tick Damage: {info.finalDamage}");

            stats.health.TakeDamage(info);
            
            totalDuration += dot.tickInterval;
            
            dot.ResetTimer();
        }
        
        base.Update();
    }
}

[Serializable] public class Freeze : StatusEffect {
    string source = "Freeze";
    private Modifier mod;
    [SerializeField] private StackableEffect effect;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Freeze config = data.statusEffectClass as Freeze;
        effect = config.effect;
        
        mod = new Modifier(effect.effectValue(stacks), effect.modifierType, source);
        ReplaceModifier();
    }

    public override void AddStack(DamageInfo damageInfo) {
        base.AddStack(damageInfo);
        
        ReplaceModifier();
    }

    private void ReplaceModifier() {
        stats.moveSpeed.RemoveModifier(mod);
        mod = new Modifier(1 - effect.effectValue(stacks), effect.modifierType, source);
        stats.moveSpeed.AddModifier(mod);
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);
        if (this.stacks > 0) {
            ReplaceModifier();
        }
    }

    public override void RemoveEffect() {
        stats?.moveSpeed.RemoveModifier(mod);
    }
}

[Serializable] public class Poison : StatusEffect {
    private string source = "Poison";
    [SerializeField] DoT dot;
    [SerializeField] private StackableEffect effect;
    private Modifier mod;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Poison config = data.statusEffectClass as Poison;
        dot = config.dot;
        effect = config.effect;

        mod = new Modifier(effect.effectValue(stacks), effect.modifierType, source);
        ReplaceModifier();
    }

    public override void AddStack(DamageInfo damageInfo) {
        base.AddStack(damageInfo);
        ReplaceModifier();
    }

    private void ReplaceModifier() {
        stats.damageMultiplier.RemoveModifier(mod);
        mod = new Modifier(1 - effect.effectValue(stacks), effect.modifierType, source);
        stats.damageMultiplier.AddModifier(mod);
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);
        if (this.stacks > 0) {
            ReplaceModifier();
        }
    }

    public override void RemoveEffect() {
        base.RemoveEffect();
        stats?.moveSpeed.RemoveModifier(mod);
    }

    public override void Update() {
        base.Update();
        dot.Update();
        if (dot.CanTick()) {
            DamageInfo info = new(dot.GetTickDamage(stacks, highestDamageReceived.finalDamage), highestDamageReceived.source, stats.transform.position) {
                ignoreResistances = true,
            };
            stats?.health?.TakeDamage(info);
            dot.ResetTimer();
        }
    }
}

[Serializable] public class Charged : StatusEffect {
    [SerializeField] DoT dot;
    [SerializeField] private float chanceToArc = 0.35f;
    [SerializeField] private float arcRadius = 5.5f;
    [SerializeField] private float damageTransferPercent = 0.2f;
    [SerializeField] private float damageArcThreshold = 10f;
    private float arcTimer;

    public override void Initialise(StatusEffectData data, EntityStatusEffectManager manager, DamageInfo damageInfo) {
        base.Initialise(data, manager, damageInfo);
        
        Charged config = data.statusEffectClass as Charged;
        dot = config.dot;
        chanceToArc = config.chanceToArc;
        arcRadius = config.arcRadius;
        damageArcThreshold = config.damageArcThreshold;
    }

    public override void Update() {
        base.Update();
        arcTimer += Time.deltaTime;
        if (arcTimer >= 1 && highestDamageReceived.finalDamage > damageArcThreshold) {
            Collider[] collisions = Physics.OverlapSphere(stats.transform.position, arcRadius);
            if (collisions.Length > 0) {
                foreach (Collider collider in collisions) {
                    if (collider.TryGetComponent(out Statboard statboard) && statboard != stats) {
                        if (!statboard.statusEffectManager.GetEffectFromList(Data)) {
                            Damage damage = new() {
                                electric = highestDamageReceived.finalDamage
                            };
                            DamageInfo info = new(damage, highestDamageReceived.source, statboard.transform.position);
                            
                            info.additionalStatusEffects.Add(Data);
                            info.AddModifier(damageTransferPercent, ModifierType.Final);
                            statboard.health.TakeDamage(info);
                        }
                    }
                }
            }
            arcTimer -= 1;
        }
        dot.Update();
        if (dot.CanTick()) {
            DamageInfo info = new(dot.GetTickDamage(stacks, highestDamageReceived.finalDamage), highestDamageReceived.source, stats.transform.position) {
                ignoreResistances = true,
            };
            stats?.health?.TakeDamage(info);
            dot.ResetTimer();
        }
    }
}

[Serializable] public class Judged : StatusEffect {
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
        if (currentDuration <= 0 || damageAbsorbed * (basePercentDamage.baseAmount + (perStackPercentDamage.baseAmount * (stacks - 1))) >= stats?.health?.CurrentHealth) {
            Detonate();
        }
        if (spriteRenderer == null || stats.healthBar == null) return;
        spriteRenderer.transform.position = stats.healthBar.transform.position + (Vector3.up * offset);
        spriteRenderer.transform.LookAt(highestDamageReceived.source.transform.position + Vector3.up * 0.7f);
        base.Update();
    }

    private void Detonate() {
        float damageAmount = damageAbsorbed * (basePercentDamage.baseAmount + (perStackPercentDamage.baseAmount * (stacks - 1)));
        Damage damage = new() {
            light = damageAmount
        };
        DamageInfo damageInfo = new(damage, highestDamageReceived.source, stats.transform.position) {
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


// TEMP TEMP TEMP
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

    public override void AddStack(DamageInfo damageInfo) {
        base.AddStack(damageInfo);
        
        ReplaceModifier();
    }

    private void ReplaceModifier() {
        mod = new Modifier(baseSpeedBoost + (perStackSpeedBoost * stacks), mod.Type, mod.Source);
        stats.moveSpeed.RemoveAllModifiersFromSource(source);
        stats.moveSpeed.AddModifier(mod);
    }

    public override void RemoveStacks(int stacks) {
        base.RemoveStacks(stacks);
        if (this.stacks > 0) {
            ReplaceModifier();
        }
    }

    public override void RemoveEffect() {
        stats.moveSpeed.RemoveAllModifiersFromSource(source);
    }
}