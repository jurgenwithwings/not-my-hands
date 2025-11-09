using System;
using UnityEngine;

public class EntityEventManager : MonoBehaviour
{
    public Statboard statboard { get; private set; }
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }
    
    /// <summary>
    /// <para><c>DamageInfo</c> - The damage that was taken</para>
    /// </summary>
    public Action<DamageInfo> OnDamageTaken;

    /// <summary>
    /// <para><c>DamageInfo</c> - The Damage that was received</para>
    /// <para><c>Statboard</c> - Victim's statboard</para>
    /// </summary>
    public Action<DamageInfo, Statboard> OnReceivedYourDamage;
    
    /// <summary>
    /// <para><c>DamageInfo</c> - DamageInfo to be modified,</para>
    /// <para><c>Statboard</c> - Victim's statboard</para>
    /// <para><c>Statboard</c> - Attacker's statboard</para>
    /// </summary>
    public Action<DamageInfo, Statboard, Statboard> OnDealingDamage;
}
