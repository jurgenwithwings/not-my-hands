using System;
using ObjectPooling;
using UnityEngine;

public class Health : MonoBehaviour
{
    private Statboard statboard;
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }

    public float currentHealth;
    
    public Action<Statboard> onDeath;
    
    private void Start() {
        currentHealth = statboard.maxHealth;
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
        currentHealth -= totalDamageTaken;
        
        //Publish that we have takenDamage
        statboard.eventManager.OnDamageTaken?.Invoke(damageInfo.Copy());
        
        //Tell the source we received their damage
        damageInfo.source.eventManager.OnReceivedYourDamage?.Invoke(damageInfo.Copy(), statboard);

        if (currentHealth < 0) {
            Die(damageInfo.source);
        }
        
        //TEMP TEMP TEMP TEMP
        onDeath?.Invoke(damageInfo.source);
        
        return totalDamageTaken;
    }

    private void Die(Statboard killer) {
        onDeath?.Invoke(killer);
        Destroy(gameObject);
    }
}