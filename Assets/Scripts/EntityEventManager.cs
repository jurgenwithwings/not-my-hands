using System;
using UnityEngine;

public class EntityEventManager : MonoBehaviour
{
    public Statboard statboard { get; private set; }
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }
    
    public Action<DamageInfo> OnTakeDamage;
    
    public void TakeDamage(DamageInfo damageInfo) {
        OnTakeDamage?.Invoke(damageInfo);
    }
}
