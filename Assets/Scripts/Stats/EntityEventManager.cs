using System;
using UnityEngine;

public class EntityEventManager : MonoBehaviour
{
    public Statboard statboard { get; private set; }
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }
    
    public Action<DamageInfo> OnDamageTaken;
    public void DamageTaken(DamageInfo damageInfo) {
        OnDamageTaken?.Invoke(damageInfo);
    }

    public Action<DamageInfo, Statboard> OnReceivedYourDamage;
    public void ReceivedYourDamage(DamageInfo damageInfo, Statboard victim) {
        OnReceivedYourDamage?.Invoke(damageInfo, victim);
    }
}
