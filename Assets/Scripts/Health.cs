using ObjectPooling;
using UnityEngine;

public class Health : MonoBehaviour
{
    private Statboard statboard;
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }

    public float currentHealth;
    
    private void Start() {
        currentHealth = statboard.maxHealth;
        ObjectPool.InitialisePool<DamageNumber>(100);
    }
    
    public float TakeDamage(DamageInfo damageInfo) {
        if (damageInfo.source == statboard && !damageInfo.selfDamage) {
            return 0;
        }

        if (!damageInfo.ignoreResistances) {
            damageInfo /= statboard.damageResistances;
        }
        Debug.LogWarning(damageInfo.totalDamage);
        
        statboard.eventManager.TakeDamage(damageInfo);
        
        float totalDamageTaken = damageInfo.totalDamage;
        currentHealth -= totalDamageTaken;

        if (ObjectPool.TryPull(transform.position, transform.rotation, out DamageNumber damageNumber))
        {
            damageNumber.gameObject.name = damageInfo.totalDamage.ToString("F1");
        }
        
        if (currentHealth < 0) {
            Die();
        }
        return totalDamageTaken;
    }

    private void Die() {
        Destroy(gameObject);
    }
}