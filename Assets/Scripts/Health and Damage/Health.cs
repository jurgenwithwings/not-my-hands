using System;
using ObjectPooling;
using UnityEngine;

public class Health : MonoBehaviour
{
    private Statboard statboard;
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }

    public float CurrentHealth {get; private set;}
    
    [SerializeField] private float healthRegenDelay = 5f;
    private float lastTimeDamaged;
    
    public Action<Statboard> onDeath;
    
    private void Start() {
        CurrentHealth = statboard.maxHealth;
        statboard.maxHealth.OnValueChanged = (newMax) => {
            if (CurrentHealth > newMax) {
                CurrentHealth = newMax;
            }
        };
        
        ObjectPool.InitialisePool<DamageNumber>(10);
    }
    
    public float TakeDamage(DamageInfo damageInfo) {
        if (damageInfo.source == statboard && !damageInfo.selfDamage) {
            return 0;
        }

        if (!damageInfo.ignoreResistances) {
            damageInfo.SetResistanceMultipliers(statboard.damageResistances);
        }
        
        float totalDamageTaken = damageInfo.totalDamage;
        CurrentHealth -= totalDamageTaken;
        lastTimeDamaged = Time.time;
        
        //Publish that we have takenDamage
        statboard.eventManager.OnDamageTaken?.Invoke(damageInfo.Copy());
        
        //Tell the source we received their damage
        damageInfo.source.eventManager.OnReceivedYourDamage?.Invoke(damageInfo.Copy(), statboard);

        if (CurrentHealth < 0) {
            Die(damageInfo.source);
        }
        
        //TEMP TEMP TEMP TEMP
        onDeath?.Invoke(damageInfo.source);
        
        return totalDamageTaken;
    }

    private void Update() {
        if (CurrentHealth < statboard.maxHealth && Time.time - lastTimeDamaged > healthRegenDelay) {
            CurrentHealth += statboard.passiveRegenRate * Time.deltaTime;
            CurrentHealth = Mathf.Min(CurrentHealth, statboard.maxHealth);
        }
    }

    private void Die(Statboard killer) {
        onDeath?.Invoke(killer);
        Destroy(gameObject);
    }
}