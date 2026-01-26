using System;
using ObjectPooling;
using UnityEngine;

public class Health : MonoBehaviour, IStatboard
{
    public Statboard statboard { get; set; }
    public void StatboardFinishedSet() {
        if (useSB) {
            CurrentHealth = statboard.maxHealth;
            statboard.maxHealth.OnValueChanged = (newMax) => {
                maxHealth = statboard.maxHealth;
                if (CurrentHealth > newMax) {
                    CurrentHealth = newMax;
                }
                statboard.eventManager.OnHealthChanged?.Invoke(CurrentHealth, newMax);
            };
        }
    }

    public float CurrentHealth {get; private set;}
    
    [SerializeField] private float healthRegenDelay = 5f;
    
    private bool useSB = true;
    [Header("If set to '0' will use the statboard")]
    [SerializeField] private float maxHealth;
    
    private float lastTimeDamaged;
    
    public event Action<Statboard> OnDeath;
    
    
    
    private void Awake() {
        if (maxHealth != 0) {
            useSB = false;
        }

        if (!useSB) {
            CurrentHealth = maxHealth;
        }
        
        ObjectPool.InitialisePool<DamageNumber>();
    }
    
    public float TakeDamage(DamageInfo damageInfo) {
        if (damageInfo.source == statboard && !damageInfo.selfDamage) {
            return 0;
        }

        if (!damageInfo.ignoreResistances) {
            damageInfo.SetResistanceMultipliers(statboard.damageResistances);
        }
        
        float totalDamageTaken = damageInfo.finalDamage;
        CurrentHealth -= totalDamageTaken;
        lastTimeDamaged = Time.time;
        
        //Publish that we have takenDamage
        statboard.eventManager.OnDamageTaken?.Invoke(damageInfo.Copy());
        
        //Tell the source we received their damage
        damageInfo.source.eventManager.OnReceivedYourDamage?.Invoke(damageInfo.Copy(), statboard);

        if (CurrentHealth < 0) {
            Die(damageInfo.source);
        }
        
        //Debug.Log(damageInfo.damageInstances[0].additiveMultiplier);
        
        //TEMP TEMP TEMP TEMP
        OnDeath?.Invoke(damageInfo.source);
        
        return totalDamageTaken;
    }

    private void Update() {
        if (CurrentHealth < maxHealth && Time.time - lastTimeDamaged > healthRegenDelay) {
            CurrentHealth += statboard.passiveRegenRate * Time.deltaTime;
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        }
    }

    private void Die(Statboard killer) {
        OnDeath?.Invoke(killer);
        Destroy(gameObject);
    }
}